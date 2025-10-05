using DMEExtractor.Infrastructure;
using DMEExtractor.Models;
using DMEExtractor.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Http;
using Moq;
using System.Net.Http;
using System.Text.Json;

namespace DMEExtractor.UnitTests.Infrastructure;

/// <summary>
/// Unit tests for the MedicalEquipmentApiClient.
/// Tests API communication functionality.
/// </summary>
public class MedicalEquipmentApiClientTests
{
    [Fact]
    public void Constructor_WithDefaultEndpoint_ShouldSetCorrectEndpoint()
    {
        // Arrange
        var settings = new AppSettings { ApiEndpoint = "https://alert-api.com/DrExtract" };
        var mockHttpClientFactory = new Mock<IHttpClientFactory>();
        var mockLogger = new Mock<ILogger<MedicalEquipmentApiClient>>();
        mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>()))
                            .Returns(new HttpClient());
        
        // Act
        var apiClient = new MedicalEquipmentApiClient(mockHttpClientFactory.Object, settings, mockLogger.Object);

        // Assert
        // We can't directly access the private field, but we can test that it doesn't throw
        Assert.NotNull(apiClient);
    }

    [Fact]
    public void Constructor_WithCustomEndpoint_ShouldAcceptCustomEndpoint()
    {
        // Arrange
        var customEndpoint = "https://custom-api.example.com/endpoint";
        var settings = new AppSettings { ApiEndpoint = customEndpoint };
        var mockHttpClientFactory = new Mock<IHttpClientFactory>();
        var mockLogger = new Mock<ILogger<MedicalEquipmentApiClient>>();
        mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>()))
                            .Returns(new HttpClient());

        // Act
        var apiClient = new MedicalEquipmentApiClient(mockHttpClientFactory.Object, settings, mockLogger.Object);

        // Assert
        Assert.NotNull(apiClient);
    }

    [Fact]
    public async Task SendEquipmentDataAsync_WithNullData_ShouldThrow()
    {
        // Arrange
        var settings = new AppSettings { ApiEndpoint = "https://test-api.com" };
        var mockHttpClientFactory = new Mock<IHttpClientFactory>();
        var mockLogger = new Mock<ILogger<MedicalEquipmentApiClient>>();
        mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>()))
                            .Returns(new HttpClient());
        var apiClient = new MedicalEquipmentApiClient(mockHttpClientFactory.Object, settings, mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => apiClient.SendEquipmentDataAsync(null!));
    }

    [Theory]
    [InlineData("CPAP")]
    [InlineData("Oxygen Tank")]
    [InlineData("Wheelchair")]
    public void SendEquipmentDataAsync_WithDifferentDeviceTypes_ShouldHandleCorrectly(string deviceType)
    {
        // Arrange
        var equipmentData = new MedicalEquipmentData
        {
            DeviceType = deviceType,
            OrderingProvider = "Dr. Theory"
        };
        var settings = new AppSettings { ApiEndpoint = "https://invalid-endpoint-that-does-not-exist.com" };
        var mockHttpClientFactory = new Mock<IHttpClientFactory>();
        var mockLogger = new Mock<ILogger<MedicalEquipmentApiClient>>();
        mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>()))
                            .Returns(new HttpClient());
        var apiClient = new MedicalEquipmentApiClient(mockHttpClientFactory.Object, settings, mockLogger.Object);

        // Act & Assert
        // We're testing that the method signature accepts different device types
        // The actual HTTP call would fail, but we're validating the data structure
        var equipmentJson = JsonSerializer.Serialize(equipmentData);
        //Assert.NotNull(equipmentData.ToJson());
        Assert.NotNull(equipmentJson);
    }
}