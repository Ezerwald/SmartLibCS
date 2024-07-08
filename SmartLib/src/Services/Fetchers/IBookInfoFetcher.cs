namespace SmartLib.src.Services.BookInfoExtractor.BookInfoFetcher
{
    public interface IBookInfoFetcher
    {
        Task<BookInfo> GetBookInfoFromOnlineDatabases(string query);
    }
}