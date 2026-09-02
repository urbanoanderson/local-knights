using System.ComponentModel;
using System.Data;
using System.Linq.Expressions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LocalKnights.Common.Helpers;

namespace LocalKnights.Common.Services.Database
{
    /// <summary>
    /// Service for managing database connections and operations.
    /// Handles SQLCipher encrypted database access with connection lifecycle management.
    /// </summary>
    public class DatabaseService : IDatabaseService
    {
        private readonly ILogger _logger;

        private readonly IFileSystemService _fileSystemService;

        internal ChessClubContext? _context;
        private readonly SemaphoreSlim _contextOperationSemaphore = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseService"/> class.
        /// </summary>
        /// <param name="fileSystemService">The file system service used to manage database storage folders.</param>
        /// <param name="logger">The logger for recording service events.</param>
        public DatabaseService(IFileSystemService fileSystemService, ILogger logger)
        {
            SqliteNativeProvider.Initialize();
            _fileSystemService = fileSystemService;
            _logger = logger;
        }

        /// <inheritdoc/>
        public bool IsConnected => _context != null;

        private object _isReadOperationInProgressLock = new object();
        private int _readOperationsInProgress = 0;

        /// <inheritdoc/>
        public event EventHandler? DataAccessStateChanged;

        /// <inheritdoc/>
        public bool IsReadOperationInProgress
        {
            get { return _readOperationsInProgress > 0; }
            private set
            {
                lock (_isReadOperationInProgressLock)
                {
                    if (value)
                    {
                        _readOperationsInProgress++;
                    }
                    else
                    {
                        _readOperationsInProgress = Math.Max(0, _readOperationsInProgress - 1);
                    }

                    DataAccessStateChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private object _isWriteOperationInProgressLock = new object();
        private int _writeOperationsInProgress = 0;

        /// <inheritdoc/>
        public bool IsWriteOperationInProgress
        {
            get { return _writeOperationsInProgress > 0; }
            private set
            {
                lock (_isWriteOperationInProgressLock)
                {
                    if (value)
                    {
                        _writeOperationsInProgress++;
                    }
                    else
                    {
                        _writeOperationsInProgress = Math.Max(0, _writeOperationsInProgress - 1);
                    }

                    DataAccessStateChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <inheritdoc/>
        public async Task OpenConnectionAsync(string clubIdentifier, string password)
        {
            this.IsReadOperationInProgress = true;

            if (_context != null)
            {
                await CloseConnectionAsync();
            }

            string clubsDirectory = _fileSystemService.GetClubsDirectory();
            _fileSystemService.CreateDirectory(clubsDirectory);
            string dbPath = _fileSystemService.GetDbFilePath(clubIdentifier);

            SqliteConnectionStringBuilder connectionStringBuilder = new SqliteConnectionStringBuilder
            {
                DataSource = dbPath,
                Password = password,
            };

            DbContextOptions<ChessClubContext> options = new DbContextOptionsBuilder<ChessClubContext>()
                .UseSqlite(connectionStringBuilder.ConnectionString)
                .Options;

            _context = new ChessClubContext(options);

            // Ensure the schema exists before attempting to use the database
            try
            {
                await _context.Database.OpenConnectionAsync();

                // Force an encrypted schema read so an invalid key fails before schema creation or migration.
                using (var validationCommand = _context.Database.GetDbConnection().CreateCommand())
                {
                    validationCommand.CommandText = "SELECT COUNT(*) FROM sqlite_master;";
                    await validationCommand.ExecuteScalarAsync();
                }

                // Disable WAL mode so SQLite uses a single database file instead of creating .db-wal/.db-shm sidecar files.
                using (var pragmaCommand = _context.Database.GetDbConnection().CreateCommand())
                {
                    pragmaCommand.CommandText = "PRAGMA journal_mode=DELETE;";
                    await pragmaCommand.ExecuteNonQueryAsync();
                }

                await _context.Database.EnsureCreatedAsync();
                await _context.Database.CanConnectAsync();
                await EnsureSchemaCorrectnessAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to open database connection for club '{clubIdentifier}'. Possible causes: incorrect password, database file corruption, or file access issues. Exception: {ex.Message}. StackTrace: {ex.StackTrace}");
                await _context.DisposeAsync();
                _context = null;
                throw new InvalidOperationException("Unable to open database with provided password");
            }
            finally
            {
                this.IsReadOperationInProgress = false;
            }
        }

        /// <inheritdoc/>
        public async Task CloseConnectionAsync()
        {
            if (_context != null)
            {
                await _context.DisposeAsync();
                _context = null;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            CloseConnectionAsync().Wait();
        }

        /// <inheritdoc/>
        public async Task<T?> GetByIdAsync<T>(params object[] keyValues) where T : class
        {
            return await ExecuteReadAsync(async () =>
            {
                return await _context!.Set<T>().FindAsync(keyValues);
            });
        }

        /// <inheritdoc/>
        public async Task<List<T>> GetAllAsync<T>() where T : class
        {
            return await ExecuteReadAsync(async () =>
            {
                IQueryable<T> query = _context!.Set<T>().AsNoTracking();
                return await query.ToListAsync();
            });
        }

        /// <inheritdoc/>
        public async Task<List<T>> GetWhereAsync<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            return await ExecuteReadAsync(async () =>
            {
                IQueryable<T> query = _context!.Set<T>().AsNoTracking().Where(predicate);
                return await query.ToListAsync();
            });
        }

        /// <inheritdoc/>
        public async Task<List<T>> QueryAsync<T>(Func<IQueryable<T>, IQueryable<T>> queryBuilder) where T : class
        {
            return await ExecuteReadAsync(async () =>
            {
                IQueryable<T> query = queryBuilder(_context!.Set<T>().AsNoTracking());
                return await query.ToListAsync();
            });
        }

        /// <inheritdoc/>
        public async Task<T?> QuerySingleOrDefaultAsync<T>(Func<IQueryable<T>, IQueryable<T>> queryBuilder) where T : class
        {
            return await ExecuteReadAsync(async () =>
            {
                IQueryable<T> query = queryBuilder(_context!.Set<T>().AsNoTracking());
                return await query.FirstOrDefaultAsync();
            });
        }

        /// <inheritdoc/>
        public async Task<int> CountAsync<T>(Func<IQueryable<T>, IQueryable<T>>? queryBuilder = null) where T : class
        {
            return await ExecuteReadAsync(async () =>
            {
                IQueryable<T> query = _context!.Set<T>();
                if (queryBuilder != null)
                {
                    query = queryBuilder(query);
                }

                return await query.CountAsync();
            });
        }

        /// <inheritdoc/>
        public async Task<T> AddAsync<T>(T entity) where T : class
        {
            return await ExecuteWriteAsync(async () =>
            {
                _context!.Set<T>().Add(entity);
                await _context.SaveChangesAsync();
                return entity;
            });
        }

        /// <inheritdoc/>
        public async Task UpdateAsync<T>(T entity) where T : class
        {
            await ExecuteWriteAsync(async () =>
            {
                _context!.ChangeTracker.Clear();
                _context!.Set<T>().Update(entity);
                await _context.SaveChangesAsync();
            });
        }

        /// <inheritdoc/>
        public async Task DeleteAsync<T>(T entity) where T : class
        {
            await ExecuteWriteAsync(async () =>
            {
                if (_context!.Entry(entity).State == EntityState.Detached)
                {
                    _context.ChangeTracker.Clear();
                    _context.Attach(entity);
                }

                _context.Set<T>().Remove(entity);
                await _context.SaveChangesAsync();
            });
        }

        /// <inheritdoc/>
        public async Task AddRangeAsync<T>(IEnumerable<T> entities) where T : class
        {
            await ExecuteWriteAsync(async () =>
            {
                _context!.Set<T>().AddRange(entities);
                await _context.SaveChangesAsync();
            });
        }

        /// <inheritdoc/>
        public async Task DeleteRangeAsync<T>(IEnumerable<T> entities) where T : class
        {
            await ExecuteWriteAsync(async () =>
            {
                _context!.ChangeTracker.Clear();
                _context.Set<T>().RemoveRange(entities);
                await _context.SaveChangesAsync();
            });
        }

        /// <inheritdoc/>
        public async Task<int> SaveChangesAsync()
        {
            if (IsConnected)
                return await ExecuteWriteAsync(async () => await _context!.SaveChangesAsync());
            else
                return 0;
        }

        /// <inheritdoc/>
        public async Task ExecuteSqlAsync(string sql, params object[] parameters)
        {
            await ExecuteReadWriteAsync(async () => await _context!.Database.ExecuteSqlRawAsync(sql, parameters));
        }

        private async Task<T> ExecuteReadAsync<T>(Func<Task<T>> operation)
        {
            await _contextOperationSemaphore.WaitAsync();
            this.IsReadOperationInProgress = true;
            EnsureConnected();

            try
            {
                return await operation();
            }
            finally
            {
                this.IsReadOperationInProgress = false;
                _contextOperationSemaphore.Release();
            }
        }

        private async Task ExecuteWriteAsync(Func<Task> operation)
        {
            await _contextOperationSemaphore.WaitAsync();
            this.IsWriteOperationInProgress = true;
            EnsureConnected();

            try
            {
                await operation();
            }
            finally
            {
                this.IsWriteOperationInProgress = false;
                _contextOperationSemaphore.Release();
            }
        }

        private async Task<T> ExecuteWriteAsync<T>(Func<Task<T>> operation)
        {
            await _contextOperationSemaphore.WaitAsync();
            this.IsWriteOperationInProgress = true;
            EnsureConnected();

            try
            {
                return await operation();
            }
            finally
            {
                this.IsWriteOperationInProgress = false;
                _contextOperationSemaphore.Release();
            }
        }

        private async Task ExecuteReadWriteAsync(Func<Task> operation)
        {
            await _contextOperationSemaphore.WaitAsync();
            this.IsReadOperationInProgress = true;
            this.IsWriteOperationInProgress = true;
            EnsureConnected();

            try
            {
                await operation();
            }
            finally
            {
                this.IsReadOperationInProgress = false;
                this.IsWriteOperationInProgress = false;
                _contextOperationSemaphore.Release();
            }
        }

        /// <summary>
        /// Ensures that a database connection is currently open.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if no connection is currently open.</exception>
        private void EnsureConnected()
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException("Database connection is not open");
            }
        }

        /// <summary>
        /// Applies sorting to an IQueryable based on the sort parameter.
        /// </summary>
        /// <typeparam name="T">The type of entity being sorted.</typeparam>
        /// <param name="query">The queryable to apply sorting to.</param>
        /// <param name="sortParameter">The sort parameter containing column name and direction.</param>
        /// <returns>The sorted queryable.</returns>
        private IQueryable<T> ApplySorting<T>(IQueryable<T> query, SortParameter sortParameter) where T : class
        {
            // Get the property info for the sort column (public instance, ignore case)
            var bindingFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.IgnoreCase;
            System.Reflection.PropertyInfo? property = typeof(T).GetProperty(sortParameter.SortColumn, bindingFlags);

            if (property == null)
            {
                // If property not found, return unsorted query
                return query;
            }

            // Create parameter expression for the entity
            ParameterExpression parameter = Expression.Parameter(typeof(T), "x");
            Expression propertyAccess = Expression.Property(parameter, property);

            // If the property is a complex owned type (e.g., ChessRating), and it has a 'Rating' member,
            // sort by that nested numeric property instead (useful for rating columns).
            if (property.PropertyType.IsClass && property.PropertyType != typeof(string))
            {
                var nested = property.PropertyType.GetProperty("Rating", bindingFlags);
                if (nested != null)
                {
                    propertyAccess = Expression.Property(propertyAccess, nested);
                }
            }

            LambdaExpression orderByExpression = Expression.Lambda(propertyAccess, parameter);

            // Apply the appropriate sort method
            string methodName = sortParameter.SortDirection == ListSortDirection.Ascending
                ? "OrderBy"
                : "OrderByDescending";

            var method = typeof(Queryable).GetMethods()
                .First(m => m.Name == methodName && m.IsGenericMethodDefinition && m.GetParameters().Length == 2);

            var generic = method.MakeGenericMethod(typeof(T), propertyAccess.Type);
            return (IQueryable<T>)generic.Invoke(null, new object[] { query, orderByExpression })!;
        }

        private async Task EnsureSchemaCorrectnessAsync()
        {
            if (_context == null)
            {
                return;
            }

            // Example: Add InitialSeeding property to Competition.
            //await DatabaseMigrationHelper.AddColumnIfMissingAsync(_logger, _context, "Competitions", "InitialSeeding", "INTEGER", "0");

            // Add future migrations here as needed.
            // ...
        }
    }
}