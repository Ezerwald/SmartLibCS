using System;
using System.Threading.Tasks;

public class BookInfoFetcher : IBookInfoFetcher
{
    /// <summary>
    /// Fetches book info from different online databases.
    /// </summary>
    public async Task<BookInfo> GetBookInfoFromOnlineDatabases(string query)
    {
        // Check in Google Books database
        var googleBooksInfo = await GoogleBooksFetcher.FetchBookInfo(query);
        if (googleBooksInfo != null)
        {
            return googleBooksInfo;
        }

        // Check in Open Library database
        var openLibraryInfo = await OpenLibraryFetcher.FetchBookInfo(query);
        if (openLibraryInfo != null)
        {
            return openLibraryInfo;
        }

        return null;
    }
}