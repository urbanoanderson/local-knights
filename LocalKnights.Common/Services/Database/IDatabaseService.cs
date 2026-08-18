using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace LocalKnights.Common.Services.Database
{
    /// <summary>
    /// Service interface for database access operations.
    /// Provides methods for opening/closing connections and performing CRUD operations.
    /// </summary>
    public interface IDatabaseService : IDisposable
    {
        /// <summary>
        /// Raised whenever the read/write operation in-progress state changes.
        /// </summary>
        event EventHandler? DataAccessStateChanged;

        /// <summary>
        /// Gets a value indicating whether a database read operation is currently in progress.
        /// </summary>
        bool IsReadOperationInProgress { get; }

        /// <summary>
        /// Gets a value indicating whether a database write operation is currently in progress.
        /// </summary>
        bool IsWriteOperationInProgress { get; }

        /// <summary>
        /// Opens a connection to the database asynchronously using SQLCipher encryption with the specified password.
        /// </summary>
        /// <param name="clubIdentifier">The identifier of the club whose database to open.</param>
        /// <param name="password">The password to decrypt the database file.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task OpenConnectionAsync(string clubIdentifier, string password);

        /// <summary>
        /// Gets a value indicating whether a database connection is currently open.
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Closes the current database connection asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task CloseConnectionAsync();

        /// <summary>
        /// Gets an entity by its primary key asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of entity to retrieve.</typeparam>
        /// <param name="keyValues">The primary key values of the entity.</param>
        /// <returns>The entity if found; otherwise null.</returns>
        Task<T?> GetByIdAsync<T>(params object[] keyValues) where T : class;

        /// <summary>
        /// Gets all entities of the specified type asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of entities to retrieve.</typeparam>
        /// <returns>A list of all entities of the specified type.</returns>
        Task<List<T>> GetAllAsync<T>() where T : class;

        /// <summary>
        /// Gets entities matching the specified predicate asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of entities to retrieve.</typeparam>
        /// <param name="predicate">The condition to filter entities.</param>
        /// <returns>A list of entities matching the condition.</returns>
        Task<List<T>> GetWhereAsync<T>(Expression<Func<T, bool>> predicate) where T : class;

        /// <summary>
        /// Gets entities by composing and executing a query against the requested entity set asynchronously.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <param name="queryBuilder">A function that builds the query to execute.</param>
        /// <returns>The query results.</returns>
        Task<List<T>> QueryAsync<T>(Func<IQueryable<T>, IQueryable<T>> queryBuilder) where T : class;

        /// <summary>
        /// Gets the first entity matching a composed query asynchronously.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <param name="queryBuilder">A function that builds the query to execute.</param>
        /// <returns>The first matching entity, or null if none exist.</returns>
        Task<T?> QuerySingleOrDefaultAsync<T>(Func<IQueryable<T>, IQueryable<T>> queryBuilder) where T : class;

        /// <summary>
        /// Counts entities by composing and executing a query asynchronously.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <param name="queryBuilder">A function that builds the query to count.</param>
        /// <returns>The number of matching entities.</returns>
        Task<int> CountAsync<T>(Func<IQueryable<T>, IQueryable<T>>? queryBuilder = null) where T : class;

        /// <summary>
        /// Adds a new entity to the database asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of entity to add.</typeparam>
        /// <param name="entity">The entity to add.</param>
        /// <returns>The added entity.</returns>
        Task<T> AddAsync<T>(T entity) where T : class;

        /// <summary>
        /// Updates an existing entity in the database asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of entity to update.</typeparam>
        /// <param name="entity">The entity with updated values.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UpdateAsync<T>(T entity) where T : class;

        /// <summary>
        /// Deletes an entity from the database asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of entity to delete.</typeparam>
        /// <param name="entity">The entity to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task DeleteAsync<T>(T entity) where T : class;

        /// <summary>
        /// Adds multiple entities to the database asynchronously.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <param name="entities">The entities to add.</param>
        Task AddRangeAsync<T>(IEnumerable<T> entities) where T : class;

        /// <summary>
        /// Deletes multiple entities from the database asynchronously.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <param name="entities">The entities to delete.</param>
        Task DeleteRangeAsync<T>(IEnumerable<T> entities) where T : class;

        /// <summary>
        /// Saves all changes to the database asynchronously.
        /// </summary>
        /// <returns>The number of entities written to the database.</returns>
        Task<int> SaveChangesAsync();

        /// <summary>
        /// Executes raw SQL against the currently connected database.
        /// </summary>
        /// <param name="sql">The SQL command to execute.</param>
        /// <param name="parameters">The parameters used by the SQL command.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task ExecuteSqlAsync(string sql, params object[] parameters);

    }
}