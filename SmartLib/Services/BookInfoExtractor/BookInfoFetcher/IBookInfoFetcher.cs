using System;
using System.IO;

public interface IBookInfoFetcher
{
    Task<BookInfo> GetBookInfoFromOnlineDatabases(string query);
}