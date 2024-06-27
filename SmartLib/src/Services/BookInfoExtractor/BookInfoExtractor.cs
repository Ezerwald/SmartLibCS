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
        var metadataResult = await AttemptExtraction(MetadataExtractor.ExtractBookInfo, filepath, "Metadata");
        string title = metadataResult.Title;
        string author = metadataResult.Author;

        if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(author))
        {
            string query = !string.IsNullOrEmpty(title) ? title : filename;
            var onlineResult = await AttemptExtraction(bookInfoFetcher.GetBookInfoFromOnlineDatabases, query, "Online Databases");
            title = onlineResult.Title;
            author = onlineResult.Author;
        }

        if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(author))
        {
            var filenameResult = FilenameExtractor.ExtractBookInfo(filename);
            title = filenameResult.Title;
            author = filenameResult.Author;
        }

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
            LogBookData(result.Title, result.Author);
            Console.WriteLine($"Extraction method '{methodName}' returned Title: {result.Title}, Author: {result.Author}");
            return result;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error during '{methodName}' extraction: {e.Message}");
            return new BookInfo();
        }
    }

    private void LogBookData(string title, string author)
    {
        Console.WriteLine($"Extracted Title: {title}, Author: {author}");
    }

    private BookInfo FinalizeBookInfo(string title, string author, string filepath)
    {
        var finalizedResult = new BookInfo
        {
            Title = string.IsNullOrEmpty(title) ? Path.GetFileNameWithoutExtension(filepath) : title,
            Author = string.IsNullOrEmpty(author) ? "Unknown" : author
        };
        LogBookData(finalizedResult.Title, finalizedResult.Author);
        return finalizedResult;
    }
}
