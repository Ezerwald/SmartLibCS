using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;

public class DatabaseHandler
{
    private readonly string _dbPath;           // Path to the SQLite database file
    private SQLiteConnection _connection;      // SQLite connection object

    public DatabaseHandler(string dbPath)
    {
        _dbPath = dbPath;
        InitializeDatabase();   // Initialize the database connection
    }

    private void InitializeDatabase()
    {
        bool dbExists = File.Exists(_dbPath);   // Check if the database file exists

        try
        {
            _connection = new SQLiteConnection($"Data Source={_dbPath};Version=3;");

            if (!dbExists)
            {
                _connection.Open();       // Open a new SQLite connection if the database doesn't exist
                CreateTable();           // Create the 'books' table if it doesn't exist
                Console.WriteLine("New database was created and connection was opened");
            }
            else
            {
                _connection.Open();       // Open existing database connection
                Console.WriteLine("Existing databse connection was opened");
            }
        }
        catch (SQLiteException e)
        {
            Console.WriteLine($"Error initializing database: {e.Message}");
            _connection = null;   // Set connection to null if initialization fails
        }
    }

    private void CreateTable()
    {
        try
        {
            using (var cmd = new SQLiteCommand(_connection))
            {
                // SQL command to create 'books' table if it doesn't exist
                cmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS books (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        title TEXT NOT NULL,
                        author TEXT NOT NULL,
                        UNIQUE(title, author)
                    )";
                cmd.ExecuteNonQuery();   // Execute the SQL command to create the table
            }
        }
        catch (SQLiteException e)
        {
            Console.WriteLine($"Error creating table: {e.Message}");
        }
    }

    public void AddBook(string title, string author)
    {
        if (_connection == null)
        {
            Console.WriteLine("Database connection is not initialized.");
            return;
        }

        try
        {
            using (var cmd = new SQLiteCommand(_connection))
            {
                // SQL command to insert a new book into the 'books' table
                cmd.CommandText = @"
                    INSERT OR IGNORE INTO books (title, author)
                    VALUES (@title, @author)";

                // Parameters for the SQL command to prevent SQL injection
                cmd.Parameters.AddWithValue("@title", title);
                cmd.Parameters.AddWithValue("@author", author);

                int rowsAffected = cmd.ExecuteNonQuery();   // Execute the SQL command and get rows affected

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

        if (_connection == null)
        {
            Console.WriteLine("Database connection is not initialized.");
            return books;
        }

        try
        {
            using (var cmd = new SQLiteCommand("SELECT id, title, author FROM books", _connection))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        // Retrieve book information from the database
                        books.Add(new Tuple<int, string, string>(
                            reader.GetInt32(0),     // ID of the book
                            reader.GetString(1),    // Title of the book
                            reader.GetString(2)     // Author of the book
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
            _connection.Close();      // Close the SQLite connection
            _connection.Dispose();    // Dispose of the connection object
        }
    }
}
