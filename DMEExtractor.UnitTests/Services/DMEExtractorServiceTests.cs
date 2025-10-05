using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using DMEExtractor.Services;
using DMEExtractor.Interfaces;
using DMEExtractor.Models;
using Microsoft.Extensions.Logging;

namespace DMEExtractor.UnitTests.Services;

/// <summary>
/// Unit tests for the DMEExtractorService.
/// Tests the main orchestration logic and dependency coordination.
/// </summary>
public class DMEExtractorServiceTests
{
    private readonly Mock<IPhysicianNoteFileReader> _mockFileReader;
    private readonly Mock<IMedicalEquipmentProcessor> _mockProcessor;
    private readonly Mock<IApiClient> _mockApiClient;
    private readonly Mock<ILogger<DMEExtractorService>> _mockLogger;
    private readonly DMEExtractorService _service;

    public DMEExtractorServiceTests()
    {
        _mockFileReader = new Mock<IPhysicianNoteFileReader>();
        _mockProcessor = new Mock<IMedicalEquipmentProcessor>();
        _mockApiClient = new Mock<IApiClient>();
        _mockLogger = new Mock<ILogger<DMEExtractorService>>();
        
        _service = new DMEExtractorService(
            _mockFileReader.Object,
            _mockProcessor.Object,
            _mockApiClient.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task RunAsync_WithValidInput_ShouldReturnSuccessCode()
    {
        // Arrange
        var fileName = "test_note.txt";
        var physicianNote = "Patient needs CPAP. Ordered by Dr. Test.";
        var equipmentData = new MedicalEquipmentData { DeviceType = "CPAP" };

        _mockFileReader.Setup(x => x.ReadPhysicianNote(fileName))
                        .Returns(physicianNote);
        
        _mockProcessor.Setup(x => x.ExtractEquipmentData(physicianNote))
                        .Returns(equipmentData);

        // Act
        var result = await _service.RunAsync(fileName);

        // Assert
        Assert.Equal(0, result);
        
        // Verify all services were called
        _mockFileReader.Verify(x => x.ReadPhysicianNote(fileName), Times.Once);
        _mockProcessor.Verify(x => x.ExtractEquipmentData(physicianNote), Times.Once);
    }

    [Fact]
    public async Task RunAsync_WithDefaultFileName_ShouldUseDefaultValue()
    {
        // Arrange
        var defaultFileName = "physician_note2.txt";
        var physicianNote = "Patient needs oxygen. Ordered by Dr. Default.";
        var equipmentData = new MedicalEquipmentData { DeviceType = "Oxygen Tank" };

        _mockFileReader.Setup(x => x.ReadPhysicianNote(defaultFileName))
                        .Returns(physicianNote);
        
        _mockProcessor.Setup(x => x.ExtractEquipmentData(physicianNote))
                        .Returns(equipmentData);

        // Act
        var result = await _service.RunAsync(defaultFileName);

        // Assert
        Assert.Equal(0, result);
        _mockFileReader.Verify(x => x.ReadPhysicianNote(defaultFileName), Times.Once);
    }

    [Fact]
    public async Task RunAsync_WhenFileReaderThrows_ShouldReturnErrorCode()
    {
        // Arrange
        var fileName = "error_file.txt";
        _mockFileReader.Setup(x => x.ReadPhysicianNote(fileName))
                        .Throws(new InvalidOperationException("File reader error"));

        // Act
        var result = await _service.RunAsync(fileName);

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public async Task RunAsync_WhenProcessorThrows_ShouldReturnErrorCode()
    {
        // Arrange
        var fileName = "test_note.txt";
        var physicianNote = "Invalid note content";

        _mockFileReader.Setup(x => x.ReadPhysicianNote(fileName))
                        .Returns(physicianNote);
        
        _mockProcessor.Setup(x => x.ExtractEquipmentData(physicianNote))
                        .Throws(new ArgumentException("Invalid physician note"));

        // Act
        var result = await _service.RunAsync(fileName);

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void Constructor_WithNullFileReader_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new DMEExtractorService(null!, _mockProcessor.Object, _mockApiClient.Object, _mockLogger.Object));
    }

    [Fact]
    public void Constructor_WithNullProcessor_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new DMEExtractorService(_mockFileReader.Object, null!, _mockApiClient.Object, _mockLogger.Object));
    }

    [Fact]
    public void Constructor_WithNullApiClient_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new DMEExtractorService(_mockFileReader.Object, _mockProcessor.Object, null!, _mockLogger.Object));
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new DMEExtractorService(_mockFileReader.Object, _mockProcessor.Object, _mockApiClient.Object, null!));
    }

    [Fact]
    public async Task RunAsync_ShouldLogInformationMessages()
    {
        // Arrange
        var fileName = "test_note.txt";
        var physicianNote = "Patient needs CPAP. Ordered by Dr. Test.";
        var equipmentData = new MedicalEquipmentData { DeviceType = "CPAP", PatientName = "John Doe" };

        _mockFileReader.Setup(x => x.ReadPhysicianNote(fileName))
                        .Returns(physicianNote);
        
        _mockProcessor.Setup(x => x.ExtractEquipmentData(physicianNote))
                        .Returns(equipmentData);

        // Act
        var result = await _service.RunAsync(fileName);

        // Assert
        Assert.Equal(0, result);
        
        // Verify logging calls
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Starting medical equipment extraction process")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task RunAsync_WithEmptyPhysicianNote_ShouldLogWarningAndReturnErrorCode()
    {
        // Arrange
        var fileName = "empty_note.txt";
        var emptyNote = "";

        _mockFileReader.Setup(x => x.ReadPhysicianNote(fileName))
                        .Returns(emptyNote);

        // Act
        var result = await _service.RunAsync(fileName);

        // Assert
        Assert.Equal(1, result);
        
        // Verify warning was logged
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Physician note is empty or null")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}