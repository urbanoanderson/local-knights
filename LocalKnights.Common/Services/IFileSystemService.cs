using System;

namespace LocalKnights.Common.Services
{
    /// <summary>
    /// Provides an abstraction for file system operations.
    /// </summary>
    public interface IFileSystemService
    {
        /// <summary>
        /// Creates all directories and subdirectories in the specified path.
        /// </summary>
        /// <param name="path">The directory path to create.</param>
        void CreateDirectory(string path);

        /// <summary>
        /// Returns a value indicating whether the specified file exists.
        /// </summary>
        /// <param name="path">The path of the file to check.</param>
        /// <returns>True if the file exists; otherwise, false.</returns>
        bool FileExists(string path);

        /// <summary>
        /// Reads all text from the specified file.
        /// </summary>
        /// <param name="path">The file path to read.</param>
        /// <returns>The content of the file.</returns>
        string ReadAllText(string path);

        /// <summary>
        /// Writes text to the specified file, creating it if necessary.
        /// </summary>
        /// <param name="path">The file path to write.</param>
        /// <param name="contents">The text content to write.</param>
        void WriteAllText(string path, string contents);

        /// <summary>
        /// Gets the names of files that match the specified search pattern in the given directory.
        /// </summary>
        /// <param name="path">The directory path to search.</param>
        /// <param name="searchPattern">The search string to match against the names of files.</param>
        /// <returns>An array of file names.</returns>
        string[] GetFiles(string path, string searchPattern);

        /// <summary>
        /// Gets the path to the application's local application data directory.
        /// </summary>
        /// <returns>The application data directory path.</returns>
        string GetAppDataDirectory();

        /// <summary>
        /// Gets the path to the directory where club database files are stored.
        /// </summary>
        /// <returns>The club databases directory path.</returns>
        string GetClubsDirectory();

        /// <summary>
        /// Gets a collection of existing filenames of application database files
        /// </summary>
        /// <returns>A collection of existing database file names with extensions.</returns>
        IEnumerable<string> GetExistingDbFiles();

        /// <summary>
        /// Gets the file path for a club's database file.
        /// </summary>
        /// <param name="filenameWithoutExtension">The identifier name of the club.</param>
        /// <returns>The full file path to the club's database file.</returns>
        string GetDbFilePath(string filenameWithoutExtension);

        /// <summary>
        /// Opens the specified directory in the user's default file explorer.
        /// </summary>
        /// <param name="path">The directory path to open.</param>
        void OpenDirectory(string path);

        /// <summary>
        /// Deletes the specified file.
        /// </summary>
        /// <param name="path">The path of the file to delete.</param>
        void DeleteFile(string path);
    }
}
