using System;
using System.IO;
using System.Threading.Tasks;
using System.Data.SQLite;
using SmartLib.src.Services;
using SmartLib.src.Data;

namespace SmartLib.src.App
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                // Get folder path from input
                var folderPath = GetFolderPathFromInput();
                if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath))
                {
                    Console.WriteLine("Invalid folder path. Please ensure the path exists.");
                    return;
                }

                // Path to the SQLite database file
                var dataBasePath = GetDataBasePath();

                // Ensure the data directory exists
                EnsureDataDirectoryExists(Path.GetDirectoryName(dataBasePath));

                // Check if database file exists, if not create it
                await EnsureDatabaseFileExistsAsync(dataBasePath);

                // Process the books
                await ProcessBooksAsync(folderPath, dataBasePath);

                // Display the books
                DisplayAllBooks(dataBasePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
            finally
            {
                Console.WriteLine("Press Enter to exit.");
                Console.ReadLine(); // Wait for the user to press Enter before closing
            }
        }

        private static string GetFolderPathFromInput()
        {
            try
            {
                Console.WriteLine("Please specify the path to your books folder:");
                return Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while reading the folder path: {ex.Message}");
                return null;
            }
        }

        private static string GetDataBasePath(string dataBasePath = null)
        {
            // Base directory where the application is running
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            if (baseDir == null)
            {
                dataBasePath = Path.Combine(baseDir, "data", "library.db");
            }
            return dataBasePath;
        }

        private static void EnsureDataDirectoryExists(string directoryPath)
        {
            if (directoryPath == null)
            {
                throw new ArgumentNullException(nameof(directoryPath));
            }

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
                Console.WriteLine($"Created data directory: {directoryPath}");
            }
            else
            {
                Console.WriteLine($"Data directory already exists: {directoryPath}");
            }
        }

        private static async Task EnsureDatabaseFileExistsAsync(string dbFilePath)
        {
            if (dbFilePath == null)
            {
                throw new ArgumentNullException(nameof(dbFilePath));
            }

            if (!File.Exists(dbFilePath))
            {
                // Create SQLite database file
                using (var connection = new SQLiteConnection($"Data Source={dbFilePath};Version=3;"))
                {
                    await connection.OpenAsync();
                    connection.Close();
                }
                Console.WriteLine($"Created SQLite database file: {dbFilePath}");
            }
            else
            {
                Console.WriteLine($"SQLite database file already exists: {dbFilePath}");
            }
        }

        private static async Task ProcessBooksAsync(string folderPath, string dbFilePath)
        {
            if (string.IsNullOrEmpty(folderPath) || string.IsNullOrEmpty(dbFilePath))
            {
                throw new ArgumentException("Folder path and database file path cannot be null or empty.");
            }

            var bookProcessor = new BookProcessor(folderPath, dbFilePath);
            await bookProcessor.ProcessBooksAsync();
        }

        private static void DisplayAllBooks(string dbFilePath)
        {
            if (string.IsNullOrEmpty(dbFilePath))
            {
                throw new ArgumentException("Database file path cannot be null or empty.");
            }

            var databaseViewer = new DatabaseViewer(dbFilePath);
            databaseViewer.ShowAllBooks();
        }
    }
}
