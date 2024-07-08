namespace SmartLib.src.Services.Fetchers
{
    public interface IBookInfoFetcher
    {
        Task<BookInfo> GetBookInfoFromOnlineDatabases(string query);
    }
}