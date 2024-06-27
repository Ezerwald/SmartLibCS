using System;
using System.Collections.Generic;

public class DatabaseViewer
{
    private readonly DatabaseHandler _dbHandler;

    public DatabaseViewer(string dbPath)
    {
        _dbHandler = new DatabaseHandler(dbPath);
    }

    public void ShowAllBooks()
    {
        var books = _dbHandler.GetAllBooks();
        if (books.Count == 0)
        {
            Console.WriteLine("No books found in the database.");
            return;
        }

        Console.WriteLine($"{"ID",-5} {"Title",-50} {"Author",-30}");
        Console.WriteLine(new string('=', 85));

        foreach (var book in books)
        {
            Console.WriteLine($"{book.Item1,-5} {book.Item2,-50} {book.Item3,-30}");
        }

        _dbHandler.CloseConnection();
    }
}
