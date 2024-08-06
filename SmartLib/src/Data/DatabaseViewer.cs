using SmartLib.src.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace SmartLib.src.Data
{
    public class DatabaseViewer : IDatabaseViewer
    {
        private readonly IDatabaseHandler _dbHandler;
        private readonly ILogger<DatabaseViewer> _logger;

        public DatabaseViewer(IDatabaseHandler dbHandler, ILogger<DatabaseViewer> logger)
        {
            _dbHandler = dbHandler ?? throw new ArgumentNullException(nameof(dbHandler));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void DisplayAllBooks()
        {
            try
            {
                var books = _dbHandler.GetAllBooks();
                if (books.Count == 0)
                {
                    Console.WriteLine("No books found in the database.");
                    return;
                }

                Console.WriteLine($"{"ID",-5} {"Title",-90} {"Author",-30}");
                Console.WriteLine(new string('=', 135));

                foreach (var book in books)
                {
                    Console.WriteLine($"{book.Item1,-5} {book.Item2,-90} {book.Item3,-30}");
                }

                    _dbHandler.CloseConnection();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while displaying books.");
            }
        }
    }
}
