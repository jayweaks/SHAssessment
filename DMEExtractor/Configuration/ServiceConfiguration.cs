using Microsoft.Extensions.DependencyInjection;
using DMEExtractor.Interfaces;
using DMEExtractor.Services;
using DMEExtractor.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Http;
using DMEExtractor.Configuration;
using System.Collections.Generic;

namespace DMEExtractor.Configuration
{
    public static class ServiceConfiguration
    {
        // Backward-compatible overload for tests
        public static IServiceCollection ConfigureServices(this IServiceCollection services)
        {
            // Create a minimal configuration for tests
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["AppSettings:ApiEndpoint"] = "https://alert-api.com/DrExtract",
                    ["AppSettings:BaseInputDirectory"] = "../../../../data/input"
                })
                .Build();
            
            return services.ConfigureServices(configuration);
        }
        
        // Main configuration method
        public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure logging
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            });
            
            // Configure HttpClient
            services.AddHttpClient();
            
            // Bind AppSettings and register IOptions<AppSettings>
            services.Configure<AppSettings>(configuration.GetSection("AppSettings"));
            // Optional: register the concrete AppSettings for simpler injection
            services.AddSingleton(sp => sp.GetRequiredService<IOptions<AppSettings>>().Value);

            // Register services
            services.AddTransient<IPhysicianNoteFileReader, PhysicianNoteFileReader>();
            services.AddTransient<IMedicalEquipmentProcessor, MedicalEquipmentProcessor>();
            services.AddTransient<IApiClient, MedicalEquipmentApiClient>();
            services.AddTransient<IDMEExtractorService, DMEExtractorService>();

            return services;
        }
    }
}