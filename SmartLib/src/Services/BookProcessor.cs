using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HeyRed.Mime;
public class BookProcessor
{
    /// <summary>
    /// The path to the folder containing digital books.
    /// </summary>
    private readonly string _folderPath;

    /// <summary>
    /// Instance of DatabaseHandler for managing database operations.
    /// </summary>
    private readonly DatabaseHandler _dbHandler;

    /// <summary>
    /// Instance of BookInfoFetcher for fetching book info.
    /// </summary>
    private readonly BookInfoFetcher _bookInfoFetcher;

    /// <summary>
    /// Instance of BookInfoExtractor for extracting book information.
    /// </summary>
    private readonly BookInfoExtractor _bookInfoExtractor;

    /// <summary>
    /// Initializes the BookProcessor with the folder path and database path.
    /// </summary>
    /// <param name="folderPath">The path to the folder containing digital books.</param>
    /// <param name="dbPath">The path to the SQLite database file.</param>
    public BookProcessor(string folderPath, string dbPath)
    {
        _folderPath = folderPath;
        _dbHandler = new DatabaseHandler(dbPath);
        _bookInfoFetcher = new BookInfoFetcher();
        _bookInfoExtractor = new BookInfoExtractor(_bookInfoFetcher);
    }

    /// <summary>
    /// Processes all digital books in the specified folder and adds their information to the database.
    /// </summary>
    public async Task ProcessBooksAsync()
    {
        var files = GetAllBookFiles(_folderPath);
        foreach (var filepath in files)
        {
            await ProcessBookAsync(filepath);
        }
        _dbHandler.CloseConnection();
    }

    /// <summary>
    /// Recursively collects all file paths of digital books within a folder.
    /// </summary>
    /// <param name="folderPath">The path to the folder.</param>
    /// <returns>A list of all file paths of digital books within the folder.</returns>
    private List<string> GetAllBookFiles(string folderPath)
    {
        var bookFiles = new List<string>();
        var files = Directory.EnumerateFiles(folderPath, "*.*", SearchOption.AllDirectories)
                             .Where(file => IsDigitalBook(file));

        bookFiles.AddRange(files);
        return bookFiles;
    }

    /// <summary>
    /// Checks if a file is a digital book based on its file extension and MIME type.
    /// </summary>
    /// <param name="filepath">The path to the file.</param>
    /// <returns>True if the file is a digital book, False otherwise.</returns>
    private bool IsDigitalBook(string filepath)
    {
        // Set of recognized digital book file extensions
        var extensions = new HashSet<string> { ".pdf", ".epub", ".mobi", ".chm" };

        // Set of recognized digital book MIME types
        var mimeTypes = new HashSet<string>
        {
            "application/pdf",
            "application/epub+zip",
            "application/x-mobipocket-ebook",
            "application/vnd.ms-htmlhelp"
        };

        // Get the file extension and convert it to lowercase
        var fileExtension = Path.GetExtension(filepath).ToLower();

        // Check if the file extension is in the set of recognized extensions
        if (!extensions.Contains(fileExtension))
        {
            return false;
        }

        // Get the MIME type corresponding to the file extension
        var mimeType = MimeTypesMap.GetMimeType(fileExtension);

        // Check if the MIME type is in the set of recognized MIME types
        return mimeTypes.Contains(mimeType);
    }

    /// <summary>
    /// Extracts book information from a digital book file and adds it to the database.
    /// </summary>
    /// <param name="filepath">The path to the digital book file to be processed.</param>
    private async Task ProcessBookAsync(string filepath)
    {
        try
        {
            var bookInfo = await _bookInfoExtractor.GetBookInfoAsync(filepath);
            string bookTitle = bookInfo.Title;
            string bookAuthor = bookInfo.Author;
            _dbHandler.AddBook(bookTitle, bookAuthor);
            Console.WriteLine($"Processed digital book '{filepath}' - Title: '{bookTitle}', Author: '{bookAuthor}'");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error processing digital book '{filepath}': {e.Message}");
        }
    }
}

