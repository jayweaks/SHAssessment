using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using DMEExtractor.Configuration;
using DMEExtractor.Interfaces;

try
{
    // Build configuration
    var configuration = new ConfigurationBuilder()
        .SetBasePath(AppContext.BaseDirectory)
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .Build();

    // Configure dependency injection
    var services = new ServiceCollection();
    services.ConfigureServices(configuration);
    
    using var serviceProvider = services.BuildServiceProvider();
    
    // Get logger for the entry point
    var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
    
    // Get the main service and app settings
    var dmeExtractorService = serviceProvider.GetRequiredService<IDMEExtractorService>();
    var appSettings = serviceProvider.GetRequiredService<AppSettings>();
    
    // Get input files from command args or all files in BaseInputDirectory
    string[] inputFiles;
    if (args.Length > 0)
    {
        inputFiles = args;
        logger.LogInformation("Processing {FileCount} file(s) from command line arguments", inputFiles.Length);
    }
    else
    {
        // Get all files from BaseInputDirectory
        var baseDir = AppContext.BaseDirectory;
        var fullBasePath = Path.GetFullPath(Path.Combine(baseDir, appSettings.BaseInputDirectory));
        
        if (Directory.Exists(fullBasePath))
        {
            inputFiles = Directory.GetFiles(fullBasePath, "*.txt")
                .Where(f => !string.IsNullOrWhiteSpace(f))
                .ToArray();
            logger.LogInformation("Processing {FileCount} .txt file(s) from: {DirectoryPath}", inputFiles.Length, fullBasePath);
        }
        else
        {
            logger.LogError("Base input directory not found: {DirectoryPath}", fullBasePath);
            return 1;
        }
    }

    var result = 0;
    foreach (var inputFile in inputFiles.Where(f => !string.IsNullOrWhiteSpace(f)))
    {
        var fileResult = await dmeExtractorService.RunAsync(inputFile);
        if (fileResult != 0)
        {
            result = fileResult;
        }
    }

    return result;
}
catch (Exception ex)
{
    // Fallback to console if logging isn't available during startup
    Console.Error.WriteLine($"Application startup failed: {ex.Message}");
    return 1;
}