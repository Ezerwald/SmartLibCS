using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SmartLib.src.Services;
using SmartLib.src.Data;
using SmartLib.src.Domain.Interfaces;
using SmartLib.src.Services.Fetchers;

namespace SmartLib.src.App
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var serviceProvider = ConfigureServices();

            using (var scope = serviceProvider.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<Program>>();

                try
                {
                    CleanDatabase(services);
                    await ProcessBooksAsync(services, logger);
                    DisplayBooks(services);

                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred during processing.");
                }
                finally
                {
                    logger.LogInformation("Press Enter to exit.");
                    Console.ReadLine();
                }
            }
        }

        private static ServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddLogging(configure => configure.AddConsole())
                .AddSingleton<IFolderPathService, FolderPathService>()
                .AddSingleton<IDatabasePathService, DatabasePathService>()
                .AddSingleton<IBookProcessor, BookProcessor>()
                .AddSingleton<IDatabaseHandler, DatabaseHandler>()
                .AddSingleton<IDatabaseViewer, DatabaseViewer>()
                .AddSingleton<IBookInfoExtractor, BookInfoExtractor>()
                .AddSingleton<IBookInfoFetcher, BookInfoFetcher>()
                .BuildServiceProvider();
        }

        private static async Task ProcessBooksAsync(IServiceProvider services, ILogger logger)
        {
            var folderPathService = services.GetRequiredService<IFolderPathService>();
            var databasePathService = services.GetRequiredService<IDatabasePathService>();
            var bookProcessor = services.GetRequiredService<IBookProcessor>();

            var databasePath = databasePathService.GetDataBasePath();
            if (string.IsNullOrEmpty(databasePath))
            {
                logger.LogError("Invalid database file path.");
                return;
            }
            await databasePathService.EnsureDatabaseFileExistsAsync(databasePath);

            var folderPath = folderPathService.GetFolderPathFromInput();
            if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath))
            {
                logger.LogError("Invalid folder path. Please ensure the path exists.");
                return;
            }
            bookProcessor.FolderPath = folderPath;
            await bookProcessor.ProcessAllBooksAsync();
        }

        private static void DisplayBooks(IServiceProvider services)
        {
            var databaseViewer = services.GetRequiredService<IDatabaseViewer>();
            databaseViewer.DisplayAllBooks();
        }

        private static void CleanDatabase(IServiceProvider services)
        {
            var databaseHandler = services.GetRequiredService<IDatabaseHandler>();
            databaseHandler.CleanDatabase();
        }
    }
}
