using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;

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
        bool dbExists = File.Exists(_dbPath);

        if (dbExists)
        {
            File.Delete(_dbPath); // Delete existing database file
        }

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
                        author TEXT NOT NULL,
                        UNIQUE(title, author)
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
                cmd.CommandText = @"
                    INSERT OR IGNORE INTO books (title, author)
                    VALUES (@title, @author)";
                cmd.Parameters.AddWithValue("@title", title);
                cmd.Parameters.AddWithValue("@author", author);
                int rowsAffected = cmd.ExecuteNonQuery();

                if (rowsAffected == 0)
                {
                    Console.WriteLine($"Book '{title}' by '{author}' already exists in the database.");
                }
                else
                {
                    Console.WriteLine($"Book '{title}' by '{author}' added to the database.");
                }
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
        _connection?.Close();
    }
}
