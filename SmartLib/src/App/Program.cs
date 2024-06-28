using System;
using System.IO;
using System.Data.SQLite;
using System.Threading.Tasks;

namespace SmartLib.src.App
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                // Path to folder with books to process
                var folderPath = "C:/Users/kyshc/Books";

                // Base directory where the application is running
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;

                // Path to the SQLite database file
                var dbFilePath = Path.Combine(baseDir, "data", "library.db");

                //// Ensure the data directory exists
                EnsureDataDirectoryExists(Path.GetDirectoryName(dbFilePath));

                //// Check if database file exists, if not create it
                EnsureDatabaseFileExists(dbFilePath);

                //// Process the books
                await ProcessBooksAsync(folderPath, dbFilePath);

                //// Display the books
                ShowAllBooks(dbFilePath);
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
        private static void EnsureDataDirectoryExists(string directoryPath)
        {
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

        private static void EnsureDatabaseFileExists(string dbFilePath)
        {
            if (!File.Exists(dbFilePath))
            {
                // Create SQLite database file
                using (var connection = new SQLiteConnection($"Data Source={dbFilePath};Version=3;"))
                {
                    connection.Open();
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
            var processor = new BookProcessor(folderPath, dbFilePath);
            await processor.ProcessBooksAsync();
        }

        private static void ShowAllBooks(string dbFilePath)
        {
            var viewer = new DatabaseViewer(dbFilePath);
            viewer.ShowAllBooks();
        }
    }
}
