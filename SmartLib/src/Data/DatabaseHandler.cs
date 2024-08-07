using SmartLib.src.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using Microsoft.Extensions.Logging;

namespace SmartLib.src.Data
{
    public class DatabaseHandler : IDatabaseHandler, IDisposable
    {
        private readonly string _dbPath;
        private SQLiteConnection _connection;
        private readonly ILogger<DatabaseHandler> _logger;
        private bool _disposed = false;

        private const string CreateTableSQL = @"
            CREATE TABLE IF NOT EXISTS books (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                title TEXT NOT NULL,
                author TEXT NOT NULL,
                UNIQUE(title, author)
            )";

        public DatabaseHandler(IDatabasePathService databasePathService, ILogger<DatabaseHandler> logger)
        {
            _dbPath = databasePathService.GetDataBasePath();
            _logger = logger;
            EnsureConnectionIsOpen();
        }

        private void EnsureConnectionIsOpen()
        {
            if (_connection == null)
            {
                try
                {
                    _logger.LogInformation($"Attempting to open database at path: {_dbPath}");
                    _connection = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
                    _connection.Open();
                    _logger.LogInformation("Database connection was opened.");
                    CreateTableIfNotExists();
                }
                catch (SQLiteException e)
                {
                    _logger.LogError(e, "Error initializing database.");
                    _connection = null;
                }
            }
            else if (_connection.State != System.Data.ConnectionState.Open)
            {
                try
                {
                    _connection.Open();
                    _logger.LogInformation("Database connection was reopened.");
                }
                catch (SQLiteException e)
                {
                    _logger.LogError(e, "Error reopening database connection.");
                    _connection = null;
                }
            }
        }

        private void CreateTableIfNotExists()
        {
            if (_connection == null)
            {
                _logger.LogWarning("Database connection is not initialized.");
                return;
            }

            try
            {
                using (var cmd = new SQLiteCommand(CreateTableSQL, _connection))
                {
                    cmd.ExecuteNonQuery();
                }
            }
            catch (SQLiteException e)
            {
                _logger.LogError(e, "Error creating table.");
            }
        }

        public void AddBook(string title, string author)
        {
            EnsureConnectionIsOpen();
            if (_connection == null) return;

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
                        _logger.LogInformation($"Book '{title}' by '{author}' already exists in the database.");
                    }
                    else
                    {
                        _logger.LogInformation($"Book '{title}' by '{author}' added to the database.");
                    }
                }
            }
            catch (SQLiteException e)
            {
                _logger.LogError(e, "Error adding book to database.");
            }
        }

        public List<Tuple<int, string, string>> GetAllBooks()
        {
            EnsureConnectionIsOpen();
            if (_connection == null) return new List<Tuple<int, string, string>>();

            _logger.LogInformation("Getting all books...");
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
                _logger.LogError(e, "Error retrieving books from database.");
            }

            return books;
        }

        public void CleanDatabase()
        {
            EnsureConnectionIsOpen();
            if (_connection == null) return;

            try
            {
                using (var cmd = new SQLiteCommand(_connection))
                {
                    cmd.CommandText = "DROP TABLE IF EXISTS books";
                    cmd.ExecuteNonQuery();
                    _logger.LogInformation("Existing tables have been dropped.");

                    CreateTableIfNotExists();
                    _logger.LogInformation("Database tables have been recreated.");
                }
            }
            catch (SQLiteException e)
            {
                _logger.LogError(e, "Error cleaning database.");
            }
        }

        public void CloseConnection()
        {
            if (_connection != null)
            {
                try
                {
                    _connection.Close();
                }
                catch (SQLiteException e)
                {
                    _logger.LogError(e, "Error closing database connection.");
                }
                finally
                {
                    _connection.Dispose();
                    _connection = null;
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    CloseConnection();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
            _logger.LogInformation("Database connection was closed and disposed.");
        }
    }
}
