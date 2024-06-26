using System;
using System.Collections.Generic;
using System.Data.SQLite;

public class DatabaseHandler
{
    private readonly string _dbPath;
    private SQLiteConnection _connection;

    public DatabaseHandler(string dbPath)
    {
        _dbPath = dbPath;
        _connection = CreateConnection();
        CreateTable();
    }

    private SQLiteConnection CreateConnection()
    {
        try
        {
            var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
            conn.Open();
            return conn;
        }
        catch (SQLiteException e)
        {
            Console.WriteLine($"Error connecting to database: {e.Message}");
            return null;
        }
    }

    private void CreateTable()
    {
        try
        {
            using (var cmd = new SQLiteCommand(_connection))
            {
                cmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS books (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        title TEXT NOT NULL,
                        author TEXT NOT NULL
                    )";
                cmd.ExecuteNonQuery();
            }
        }
        catch (SQLiteException e)
        {
            Console.WriteLine($"Error creating table: {e.Message}");
        }
    }

    public void AddBook(string title, string author)
    {
        try
        {
            using (var cmd = new SQLiteCommand(_connection))
            {
                cmd.CommandText = "INSERT INTO books (title, author) VALUES (@title, @author)";
                cmd.Parameters.AddWithValue("@title", title);
                cmd.Parameters.AddWithValue("@author", author);
                cmd.ExecuteNonQuery();
            }
        }
        catch (SQLiteException e)
        {
            Console.WriteLine($"Error adding book to database: {e.Message}");
        }
    }

    public List<Tuple<int, string, string>> GetAllBooks()
    {
        var books = new List<Tuple<int, string, string>>();

        try
        {
            using (var cmd = new SQLiteCommand("SELECT id, title, author FROM books", _connection))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        books.Add(new Tuple<int, string, string>(
                            reader.GetInt32(0),
                            reader.GetString(1),
                            reader.GetString(2)
                        ));
                    }
                }
            }
        }
        catch (SQLiteException e)
        {
            Console.WriteLine($"Error retrieving books from database: {e.Message}");
        }

        return books;
    }

    public void CloseConnection()
    {
        if (_connection != null)
        {
            _connection.Close();
        }
    }
}
