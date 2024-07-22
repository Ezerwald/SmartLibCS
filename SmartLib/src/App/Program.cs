using System.IO;
using SmartLib.src.Services;
using SmartLib.src.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SmartLib.src.Domain.Interfaces;

namespace SmartLib.src.App
{
    internal class Program
    {
        private static readonly ServiceProvider ServiceProvider = ConfigureServices();

        public static async Task Main(string[] args)
        {
            var logger = ServiceProvider.GetService<ILogger<Program>>();

            try
            {
                var folderPathService = ServiceProvider.GetService<IFolderPathService>();
                var databasePathService = ServiceProvider.GetService<IDatabasePathService>();
                var bookProcessor = ServiceProvider.GetService<IBookProcessor>();
                var databaseViewer = ServiceProvider.GetService<IDatabaseViewer>();

                var folderPath = folderPathService.GetFolderPathFromInput();
                if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath))
                {
                    logger.LogError("Invalid folder path. Please ensure the path exists.");
                    return;
                }

                var dataBasePath = databasePathService.GetDataBasePath();
                if (dataBasePath != null)
                {
                    await databasePathService.EnsureDatabaseFileExistsAsync(dataBasePath);

                    await bookProcessor.ProcessAllBooksAsync();
                    databaseViewer.DisplayAllBooks(dataBasePath);
                }
                else
                {
                    logger.LogError("Invalid database file path.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"An error occurred: {ex.Message}");
            }
            finally
            {
                logger.LogInformation("Press Enter to exit.");
                Console.ReadLine();
            }
        }

        private static ServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddLogging(configure => configure.AddConsole())
                .AddSingleton<IFolderPathService, FolderPathService>()
                .AddSingleton<IDatabasePathService, DatabasePathService>()
                .AddSingleton<IBookProcessor, BookProcessor>()
                .AddSingleton<IDatabaseViewer, DatabaseViewer>()
                .BuildServiceProvider();
        }
    }
}