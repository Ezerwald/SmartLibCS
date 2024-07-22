using System.IO;
using HeyRed.Mime;
using Microsoft.Extensions.Logging;
using SmartLib.src.Domain.Interfaces;
using SmartLib.src.Services.Fetchers;

namespace SmartLib.src.Services
{
    public class BookProcessor : IBookProcessor
    {
        private readonly string _folderPath;
        private readonly IDatabaseHandler _dbHandler;
        private readonly IBookInfoFetcher _bookInfoFetcher;
        private readonly IBookInfoExtractor _bookInfoExtractor;
        private readonly ILogger<BookProcessor> _logger;

        public BookProcessor(string folderPath, IDatabaseHandler dbHandler, IBookInfoFetcher bookInfoFetcher, IBookInfoExtractor bookInfoExtractor, ILogger<BookProcessor> logger)
        {
            _folderPath = folderPath ?? throw new ArgumentNullException(nameof(folderPath));
            _dbHandler = dbHandler ?? throw new ArgumentNullException(nameof(dbHandler));
            _bookInfoFetcher = bookInfoFetcher ?? throw new ArgumentNullException(nameof(bookInfoFetcher));
            _bookInfoExtractor = bookInfoExtractor ?? throw new ArgumentNullException(nameof(bookInfoExtractor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task ProcessAllBooksAsync()
        {
            try
            {
                var files = GetAllBookFiles(_folderPath);
                foreach (var filepath in files)
                {
                    await ProcessBookAsync(filepath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing all books.");
            }
            finally
            {
                _dbHandler.CloseConnection();
            }
        }

        public List<string> GetAllBookFiles(string folderPath)
        {
            return Directory.EnumerateFiles(folderPath, "*.*", SearchOption.AllDirectories)
                            .Where(IsDigitalBook)
                            .ToList();
        }

        public bool IsDigitalBook(string filepath)
        {
            var extensions = new HashSet<string> { ".pdf", ".epub", ".mobi", ".chm" };
            var mimeTypes = new HashSet<string>
            {
                "application/pdf",
                "application/epub+zip",
                "application/x-mobipocket-ebook",
                "application/vnd.ms-htmlhelp"
            };

            var fileExtension = Path.GetExtension(filepath).ToLower();
            if (!extensions.Contains(fileExtension))
            {
                return false;
            }

            var mimeType = MimeTypesMap.GetMimeType(fileExtension);
            return mimeTypes.Contains(mimeType);
        }

        public async Task ProcessBookAsync(string filepath)
        {
            try
            {
                var bookInfo = await _bookInfoExtractor.GetBookInfoAsync(filepath);
                _dbHandler.AddBook(bookInfo.Title, bookInfo.Author);
                _logger.LogInformation($"Processed digital book '{filepath}' - Title: '{bookInfo.Title}', Author: '{bookInfo.Author}'");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing digital book '{filepath}'");
            }
        }

        public Task ProcessAllBooksAsync()
        {
            throw new NotImplementedException();
        }
    }
}
