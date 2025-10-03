using System;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using DMEExtractor.Configuration;
using DMEExtractor.Interfaces;
using DMEExtractor.Services;
using DMEExtractor.Infrastructure;

namespace DMEExtractor.UnitTests.Configuration
{
    /// <summary>
    /// Unit tests for the ServiceConfiguration.
    /// Tests dependency injection container setup and service resolution.
    /// </summary>
    public class ServiceConfigurationTests
    {
        [Fact]
        public void ConfigureServices_ShouldRegisterAllRequiredServices()
        {
            // Arrange
            var services = new ServiceCollection();
            
            // Act
            services.ConfigureServices();
            using var serviceProvider = services.BuildServiceProvider();
            
            // Assert - All services should be resolvable
            var fileReader = serviceProvider.GetService<IPhysicianNoteFileReader>();
            var processor = serviceProvider.GetService<IMedicalEquipmentProcessor>();
            var apiClient = serviceProvider.GetService<IApiClient>();
            var mainService = serviceProvider.GetService<IDMEExtractorService>();
            
            Assert.NotNull(fileReader);
            Assert.NotNull(processor);
            Assert.NotNull(apiClient);
            Assert.NotNull(mainService);
        }

        [Fact]
        public void ConfigureServices_ShouldRegisterCorrectImplementations()
        {
            // Arrange
            var services = new ServiceCollection();
            
            // Act
            services.ConfigureServices();
            using var serviceProvider = services.BuildServiceProvider();
            
            // Assert - Verify correct implementations
            var fileReader = serviceProvider.GetService<IPhysicianNoteFileReader>();
            var processor = serviceProvider.GetService<IMedicalEquipmentProcessor>();
            var apiClient = serviceProvider.GetService<IApiClient>();
            var mainService = serviceProvider.GetService<IDMEExtractorService>();
            
            Assert.IsType<PhysicianNoteFileReader>(fileReader);
            Assert.IsType<MedicalEquipmentProcessor>(processor);
            Assert.IsType<MedicalEquipmentApiClient>(apiClient);
            Assert.IsType<DMEExtractorService>(mainService);
        }

        [Fact]
        public void ConfigureServices_ShouldRegisterServicesAsTransient()
        {
            // Arrange
            var services = new ServiceCollection();
            services.ConfigureServices();
            using var serviceProvider = services.BuildServiceProvider();
            
            // Act - Get multiple instances
            var fileReader1 = serviceProvider.GetService<IPhysicianNoteFileReader>();
            var fileReader2 = serviceProvider.GetService<IPhysicianNoteFileReader>();
            
            var processor1 = serviceProvider.GetService<IMedicalEquipmentProcessor>();
            var processor2 = serviceProvider.GetService<IMedicalEquipmentProcessor>();
            
            // Assert - Different instances should be returned (Transient lifestyle)
            Assert.NotSame(fileReader1, fileReader2);
            Assert.NotSame(processor1, processor2);
        }

        [Fact]
        public void ConfigureServices_WithEmptyServiceCollection_ShouldReturnConfiguredCollection()
        {
            // Arrange
            var services = new ServiceCollection();
            
            // Act
            var result = services.ConfigureServices();
            
            // Assert
            Assert.Same(services, result); // Should return the same collection for fluent interface
            Assert.True(services.Count > 0); // Should have services registered
        }

        [Fact]
        public void ConfigureServices_ShouldHandleDependencyChain()
        {
            // Arrange
            var services = new ServiceCollection();
            services.ConfigureServices();
            using var serviceProvider = services.BuildServiceProvider();
            
            // Act - Get the main service which depends on others
            var mainService = serviceProvider.GetService<IDMEExtractorService>();
            
            // Assert - Should successfully resolve with all dependencies
            Assert.NotNull(mainService);
            Assert.IsType<DMEExtractorService>(mainService);
        }

        [Fact]
        public void ConfigureServices_ShouldAllowMultipleRegistrations()
        {
            // Arrange
            var services = new ServiceCollection();
            
            // Act - Configure multiple times
            services.ConfigureServices();
            services.ConfigureServices();
            
            // Assert - Should not throw and should still work
            using var serviceProvider = services.BuildServiceProvider();
            var mainService = serviceProvider.GetService<IDMEExtractorService>();
            Assert.NotNull(mainService);
        }
    }
}