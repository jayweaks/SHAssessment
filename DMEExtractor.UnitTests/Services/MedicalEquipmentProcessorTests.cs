using System;
using System.Linq;
using Xunit;
//using DMEExtractor.Models;
using DMEExtractor.Services;
using DMEExtractor.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;

namespace DMEExtractor.UnitTests.Services;

/// <summary>
/// Unit tests for the MedicalEquipmentProcessor service.
/// Tests the core business logic for extracting medical equipment data from physician notes.
/// </summary>
public class MedicalEquipmentProcessorTests
{
    private readonly IMedicalEquipmentProcessor _processor;
    private readonly Mock<ILogger<MedicalEquipmentProcessor>> _mockLogger;

    public MedicalEquipmentProcessorTests()
    {
        _mockLogger = new Mock<ILogger<MedicalEquipmentProcessor>>();
        _processor = new MedicalEquipmentProcessor(_mockLogger.Object);
    }

    [Fact]
    public void ExtractEquipmentData_WithCPAPNote_ShouldExtractCorrectData()
    {
        // Arrange
        var physicianNote = "Patient: John Doe\nDOB: 01/15/1980\nDiagnosis: Sleep apnea\nPatient needs a CPAP with full face mask and humidifier. AHI > 20. Ordered by Dr. Cameron.";

        // Act
        var result = _processor.ExtractEquipmentData(physicianNote);

        // Assert
        Assert.Equal("CPAP", result.DeviceType);
        Assert.Equal("Dr. Cameron", result.OrderingProvider);
        Assert.Equal("John Doe", result.PatientName);
        Assert.Equal("01/15/1980", result.DateOfBirth);
        Assert.Equal("Sleep apnea", result.Diagnosis);
    }

    [Fact]
    public void ExtractEquipmentData_WithOxygenNote_ShouldExtractCorrectData()
    {
        // Arrange
        var physicianNote = "Patient: Jane Smith\nDOB: 05/20/1975\nDiagnosis: COPD\nPatient requires 2.5 L oxygen for sleep and exertion. Ordered by Dr. House.";

        // Act
        var result = _processor.ExtractEquipmentData(physicianNote);

        // Assert
        Assert.Equal("Oxygen Tank", result.DeviceType);
        Assert.Equal("2.5 L", result.OxygenLiters);
        Assert.Equal("sleep and exertion", result.OxygenUsage);
        Assert.Equal("Dr. House", result.OrderingProvider);
        Assert.Equal("Jane Smith", result.PatientName);
        Assert.Equal("05/20/1975", result.DateOfBirth);
        Assert.Equal("COPD", result.Diagnosis);
    }

    [Fact]
    public void ExtractEquipmentData_WithWheelchairNote_ShouldExtractCorrectData()
    {
        // Arrange
        var physicianNote = "Patient: Bob Johnson\nDOB: 12/10/1965\nDiagnosis: Mobility impairment\nPatient requires manual wheelchair for mobility. Ordered by Dr. Smith.";

        // Act
        var result = _processor.ExtractEquipmentData(physicianNote);

        // Assert
        Assert.Equal("Wheelchair", result.DeviceType);
        Assert.Equal("Dr. Smith", result.OrderingProvider);
        Assert.Equal("Bob Johnson", result.PatientName);
        Assert.Equal("12/10/1965", result.DateOfBirth);
        Assert.Equal("Mobility impairment", result.Diagnosis);
    }

    [Fact]
    public void ExtractEquipmentData_WithUnknownDevice_ShouldDefaultToUnknown()
    {
        // Arrange
        var physicianNote = "Patient: Unknown Patient\nDOB: 01/01/2000\nDiagnosis: General\nPatient needs some medical equipment. Ordered by Dr. Unknown.";

        // Act
        var result = _processor.ExtractEquipmentData(physicianNote);

        // Assert
        Assert.Equal("Unknown", result.DeviceType);
        Assert.Equal("Dr. Unknown", result.OrderingProvider);
        Assert.Equal("Unknown Patient", result.PatientName);
        Assert.Equal("01/01/2000", result.DateOfBirth);
        Assert.Equal("General", result.Diagnosis);
    }

    [Fact]
    public void ExtractEquipmentData_WithEmptyNote_ShouldThrowArgumentException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() => _processor.ExtractEquipmentData(""));
        Assert.Throws<ArgumentException>(() => _processor.ExtractEquipmentData(null!));
        Assert.Throws<ArgumentException>(() => _processor.ExtractEquipmentData("   "));
    }

    [Fact]
    public void ExtractEquipmentData_WithoutDoctor_ShouldDefaultToUnknown()
    {
        // Arrange
        var physicianNote = "Patient: Test Patient\nDOB: 06/15/1990\nDiagnosis: Sleep disorder\nPatient needs a CPAP with full face mask.";

        // Act
        var result = _processor.ExtractEquipmentData(physicianNote);

        // Assert
        Assert.Equal("CPAP", result.DeviceType);
        Assert.Equal("Unknown", result.OrderingProvider);
        Assert.Equal("Test Patient", result.PatientName);
        Assert.Equal("06/15/1990", result.DateOfBirth);
        Assert.Equal("Sleep disorder", result.Diagnosis);
    }

    [Fact]
    public void ExtractEquipmentData_WithMultipleOxygenUsages_ShouldExtractBoth()
    {
        // Arrange
        var physicianNote = "Patient: Mary Wilson\nDOB: 08/30/1970\nDiagnosis: Respiratory failure\nPatient needs 3 L oxygen for sleep and exertion activities. Ordered by Dr. Wilson.";

        // Act
        var result = _processor.ExtractEquipmentData(physicianNote);

        // Assert
        Assert.Equal("Oxygen Tank", result.DeviceType);
        Assert.Equal("3 L", result.OxygenLiters);
        Assert.Equal("sleep and exertion", result.OxygenUsage);
        Assert.Equal("Dr. Wilson", result.OrderingProvider);
        Assert.Equal("Mary Wilson", result.PatientName);
        Assert.Equal("08/30/1970", result.DateOfBirth);
        Assert.Equal("Respiratory failure", result.Diagnosis);
    }

    [Fact]
    public void ExtractEquipmentData_WithOnlySleepUsage_ShouldExtractSleepOnly()
    {
        // Arrange
        var physicianNote = "Patient: Stephen Strange\nDOB: 11/18/1963\nDiagnosis: Sleep apnea\nPatient needs 1.5 L oxygen for sleep apnea. Ordered by Dr. Strange.";

        // Act
        var result = _processor.ExtractEquipmentData(physicianNote);

        // Assert
        Assert.Equal("Oxygen Tank", result.DeviceType);
        Assert.Equal("1.5 L", result.OxygenLiters);
        Assert.Equal("sleep", result.OxygenUsage);
        Assert.Equal("Dr. Strange", result.OrderingProvider);
        Assert.Equal("Stephen Strange", result.PatientName);
        Assert.Equal("11/18/1963", result.DateOfBirth);
        Assert.Equal("Sleep apnea", result.Diagnosis);
    }
}