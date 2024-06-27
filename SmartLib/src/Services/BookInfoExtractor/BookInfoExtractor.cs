using System;
using System.IO;
using System.Threading.Tasks;

public class BookInfoExtractor
{
    private readonly IBookInfoFetcher bookInfoFetcher;

    public BookInfoExtractor(IBookInfoFetcher bookInfoFetcher)
    {
        this.bookInfoFetcher = bookInfoFetcher;
    }

    public async Task<BookInfo> GetBookInfoAsync(string filepath)
    {
        string filename = Path.GetFileName(filepath);

        // Try extracting from metadata first
        var metadataResult = await AttemptExtraction(MetadataExtractor.ExtractBookInfo, filepath, "Metadata");

        // Check if we have a valid title and author
        string title = metadataResult?.Title;
        string author = metadataResult?.Author;

        // If not found or incomplete, try fetching from online databases
        if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(author))
        {
            string query = !string.IsNullOrEmpty(title) ? title : filename;
            var onlineResult = await AttemptExtraction(bookInfoFetcher.GetBookInfoFromOnlineDatabases, query, "Online Databases");
            title = onlineResult?.Title;
            author = onlineResult?.Author;
        }

        // If still not found or incomplete, try extracting from filename
        if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(author))
        {
            var filenameResult = FilenameExtractor.ExtractBookInfo(filename);
            title = filenameResult?.Title;
            author = filenameResult?.Author;
        }

        // Finalize book info ensuring no empty values are returned
        var finalizedResult = FinalizeBookInfo(title, author, filepath);
        title = finalizedResult.Title;
        author = finalizedResult.Author;

        return new BookInfo { Title = title, Author = author };
    }

    private async Task<BookInfo> AttemptExtraction(Func<string, Task<BookInfo>> extractionMethod, string inputData, string methodName)
    {
        try
        {
            var result = await extractionMethod(inputData);
            if (result != null)
            {
                LogBookData(result.Title, result.Author);
                Console.WriteLine($"Extraction method '{methodName}' returned Title: {result.Title}, Author: {result.Author}");
            }
            else
            {
                Console.WriteLine($"Extraction method '{methodName}' returned no data.");
            }           
            return result;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Extraction method '{methodName}' failed: {e.Message}");
            return new BookInfo { Title = null, Author = null };
        }
    }

    private BookInfo FinalizeBookInfo(string title, string author, string filepath)
    {
        if (string.IsNullOrEmpty(title))
        {
            Console.WriteLine("No title found");
            string filename = Path.GetFileName(filepath);
            title = filename;
        }

        if (string.IsNullOrEmpty(author))
        {
            Console.WriteLine("No author found");
            author = "No author found";
        }

        return new BookInfo { Title = title, Author = author };
    }

    private void LogBookData(string title, string author)
    {
        // Implement your logging logic here
        Console.WriteLine($"Logging Book Data - Title: {title}, Author: {author}");
    }
}
