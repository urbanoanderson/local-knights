namespace LocalKnights.Common.Services.Database
{
    /// <summary>
    /// Represents an attempt to open a database whose schema is newer than the application supports.
    /// </summary>
    public sealed class UnsupportedDatabaseSchemaVersionException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnsupportedDatabaseSchemaVersionException"/> class.
        /// </summary>
        /// <param name="storedVersion">The schema version stored in the database.</param>
        /// <param name="supportedVersion">The newest schema version supported by the application.</param>
        public UnsupportedDatabaseSchemaVersionException(int storedVersion, int supportedVersion)
            : base($"Database schema version {storedVersion} is newer than supported version {supportedVersion}.")
        {
            StoredVersion = storedVersion;
            SupportedVersion = supportedVersion;
        }

        /// <summary>
        /// Gets the schema version stored in the database.
        /// </summary>
        public int StoredVersion { get; }

        /// <summary>
        /// Gets the newest schema version supported by the application.
        /// </summary>
        public int SupportedVersion { get; }
    }
}