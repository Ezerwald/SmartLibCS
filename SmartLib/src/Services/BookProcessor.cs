using System.IO;
using HeyRed.Mime;
using Microsoft.Extensions.Logging;
using SmartLib.src.Domain.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;

namespace SmartLib.src.Services
{
    public class BookProcessor : IBookProcessor
    {
        private readonly IDatabaseHandler _dbHandler;
        private readonly IBookInfoExtractor _bookInfoExtractor;
        private readonly ILogger<BookProcessor> _logger;

        public string FolderPath { get; set; }   // FolderPath is now a property

        public BookProcessor(IDatabaseHandler dbHandler, IBookInfoExtractor bookInfoExtractor, ILogger<BookProcessor> logger)
        {
            _dbHandler = dbHandler ?? throw new ArgumentNullException(nameof(dbHandler));
            _bookInfoExtractor = bookInfoExtractor ?? throw new ArgumentNullException(nameof(bookInfoExtractor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task ProcessAllBooksAsync()
        {
            if (string.IsNullOrEmpty(FolderPath) || !Directory.Exists(FolderPath))
            {
                _logger.LogError("Invalid or empty folder path.");
                return;
            }

            try
            {
                var files = GetAllBookFiles(FolderPath);
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
                _logger.LogInformation("All books have been processed.");
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
    }
}
