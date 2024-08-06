using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SmartLib.src.Services;
using SmartLib.src.Data;
using SmartLib.src.Domain.Interfaces;
using SmartLib.src.Services.Fetchers;
using System.Printing.IndexedProperties;

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
                    var folderPathService = services.GetRequiredService<IFolderPathService>();
                    var databasePathService = services.GetRequiredService<IDatabasePathService>();
                    var bookProcessor = services.GetRequiredService<IBookProcessor>();
                    var databaseViewer = services.GetRequiredService<IDatabaseViewer>();
                    var databaseHandler = services.GetRequiredService<IDatabaseHandler>();

                    var folderPath = folderPathService.GetFolderPathFromInput();
                    if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath))
                    {
                        logger.LogError("Invalid folder path. Please ensure the path exists.");
                        return;
                    }
                    bookProcessor.FolderPath = folderPath;

                    var databasePath = databasePathService.GetDataBasePath();
                    if (string.IsNullOrEmpty(databasePath))
                    {
                        logger.LogError("Invalid database file path.");
                        return;
                    }

                    await databasePathService.EnsureDatabaseFileExistsAsync(databasePath);

                    await bookProcessor.ProcessAllBooksAsync();
                    databaseViewer.DisplayAllBooks();
//                    databaseHandler.CleanDatabase();
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
    }
}
