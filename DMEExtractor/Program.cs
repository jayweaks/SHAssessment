using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
    
    // Get the main service and app settings
    var dmeExtractorService = serviceProvider.GetRequiredService<IDMEExtractorService>();
    var appSettings = serviceProvider.GetRequiredService<AppSettings>();
    
    // Get input files from command args or all files in BaseInputDirectory
    string[] inputFiles;
    if (args.Length > 0)
    {
        inputFiles = args;
        Console.WriteLine($"Processing {inputFiles.Length} file(s) from command line arguments.");
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
            Console.WriteLine($"Processing {inputFiles.Length} .txt file(s) from: {fullBasePath}");
        }
        else
        {
            Console.WriteLine($"Base input directory not found: {fullBasePath}");
            Console.WriteLine("No input files to process.");
            return 1;
        }
    }

    foreach (var inputFile in inputFiles.Where(f => !string.IsNullOrWhiteSpace(f)))
    {
        var result = await dmeExtractorService.RunAsync(inputFile);
        // Optionally handle result here (e.g., log, aggregate, etc.)
    }
    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Application startup failed: {ex.Message}");
    return 1;
}
