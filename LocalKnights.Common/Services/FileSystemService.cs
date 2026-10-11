using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;

namespace LocalKnights.Common.Services
{
    /// <summary>
    /// Default implementation of <see cref="IFileSystemService"/> that delegates to System.IO.
    /// </summary>
    public sealed class FileSystemService : IFileSystemService
    {
        /// <summary>
        /// The name of the application data folder.
        /// </summary>
        private const string AppDataFolderName = "LocalKnights";

        /// <summary>
        /// The name of the clubs folder within the application data directory.
        /// </summary>
        private const string ClubsFolderName = "Clubs";

        /// <summary>
        /// The file extension for club database files.
        /// </summary>
        private const string DbExtension = ".db";

        /// <summary>
        /// Application logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileSystemService"/> class.
        /// </summary>
        /// <param name="logger">The logger for recording service events.</param>
        public FileSystemService(ILogger logger)
        {
            _logger = logger;
        }

        /// <inheritdoc/>
        public void CreateDirectory(string path)
        {
            Directory.CreateDirectory(path);
        }

        /// <inheritdoc/>
        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        /// <inheritdoc/>
        public string ReadAllText(string path)
        {
            return File.ReadAllText(path);
        }

        /// <inheritdoc/>
        public void WriteAllText(string path, string contents)
        {
            File.WriteAllText(path, contents);
        }

        /// <inheritdoc/>
        public string[] GetFiles(string path, string searchPattern)
        {
            return Directory.GetFiles(path, searchPattern);
        }

        /// <inheritdoc/>
        public string GetAppDataDirectory()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppDataFolderName);
        }

        /// <inheritdoc/>
        public string GetClubsDirectory()
        {
            return Path.Combine(GetAppDataDirectory(), ClubsFolderName);
        }

        /// <inheritdoc/>
        public IEnumerable<string> GetExistingDbFiles()
        {
            string clubsDirectory = this.GetClubsDirectory();
            this.CreateDirectory(clubsDirectory);

            string[] clubFiles = this.GetFiles(clubsDirectory, $"*{DbExtension}");
            return clubFiles;
        }

        /// <inheritdoc/>
        public string GetDbFilePath(string filenameWithoutExtension)
        {
            return Path.Combine(this.GetClubsDirectory(), $"{filenameWithoutExtension}{DbExtension}");
        }

        /// <inheritdoc/>
        public void OpenDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                _logger.LogWarning("Cannot open directory because it does not exist: {Path}", path);
                return;
            }

            ProcessStartInfo startInfo = new()
            {
                FileName = GetDirectoryOpenCommand(),
                Arguments = BuildDirectoryOpenArguments(path),
                UseShellExecute = true
            };

            Process.Start(startInfo);
        }

        /// <inheritdoc/>
        public void OpenUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                _logger.LogWarning("Cannot open URL because it is empty.");
                return;
            }

            ProcessStartInfo startInfo = new()
            {
                FileName = url,
                UseShellExecute = true
            };

            Process.Start(startInfo);
        }

        /// <inheritdoc/>
        public void DeleteFile(string path)
        {
            File.Delete(path);
        }

        /// <inheritdoc/>
        public void CopyFile(string sourcePath, string destinationPath, bool overwrite)
        {
            File.Copy(sourcePath, destinationPath, overwrite);
        }

        private static string GetDirectoryOpenCommand()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return "explorer.exe";
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return "open";
            }

            return "xdg-open";
        }

        private static string BuildDirectoryOpenArguments(string path)
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? $"\"{path}\"" : path;
        }
    }
}
