using Microsoft.Extensions.Logging;
using SmartLib.src.Domain.Interfaces;
using System.Data.SQLite;
using System.IO;

namespace SmartLib.src.Services
{
    public class DatabasePathService : IDatabasePathService
    {
        private readonly ILogger<DatabasePathService> _logger;

        public DatabasePathService(ILogger<DatabasePathService> logger)
        {
            _logger = logger;
        }

        public string GetDataBasePath()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var dataBasePath = Path.Combine(baseDir, "data", "library.db");

            EnsureDataDirectoryExists(Path.GetDirectoryName(dataBasePath));
            return dataBasePath;
        }

        private void EnsureDataDirectoryExists(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
                _logger.LogInformation($"Created data directory: {directoryPath}");
            }
            else
            {
                _logger.LogInformation($"Data directory already exists: {directoryPath}");
            }
        }

        public async Task EnsureDatabaseFileExistsAsync(string dbFilePath)
        {
            if (!File.Exists(dbFilePath))
            {
                using (var connection = new SQLiteConnection($"Data Source={dbFilePath};Version=3;"))
                {
                    await connection.OpenAsync();
                    connection.Close();
                }
                _logger.LogInformation($"Created SQLite database file: {dbFilePath}");
            }
            else
            {
                _logger.LogInformation($"SQLite database file already exists: {dbFilePath}");
            }
        }
    }
}
