using System;
using System.IO;
using System.Threading.Tasks;

namespace SmartLib.src.App
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            var folderPath = "C:/Users/kyshc/Books";
            var dbPath = "data/library.db";

            // Ensure the data directory exists
            if (!Directory.Exists("data"))
            {
                Directory.CreateDirectory("data");
            }

            // Process the books
            var processor = new BookProcessor(folderPath, dbPath);
            await processor.ProcessBooksAsync();

            // Display the books
            var viewer = new DatabaseViewer(dbPath);
            viewer.ShowAllBooks();
        }
    }
}
