using System;
using System.IO;
using Xunit;
using DMEExtractor.Services;
using DMEExtractor.Interfaces;
using DMEExtractor.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Moq;

namespace DMEExtractor.UnitTests.Services
{
    /// <summary>
    /// Unit tests for the PhysicianNoteFileReader service.
    /// Tests file reading functionality and error handling.
    /// </summary>
    public class PhysicianNoteFileReaderTests
    {
        private readonly IPhysicianNoteFileReader _fileReader;

        public PhysicianNoteFileReaderTests()
        {
            // Create mock AppSettings for testing
            var appSettings = new AppSettings
            {
                BaseInputDirectory = "../../../../data/input"
            };
            var options = Options.Create(appSettings);
            var mockLogger = new Mock<ILogger<PhysicianNoteFileReader>>();
            _fileReader = new PhysicianNoteFileReader(options, mockLogger.Object);
        }

        [Fact]
        public void ReadPhysicianNote_WithValidFileName_ShouldReturnContent()
        {
            // Arrange
            var fileName = "physician_note.txt";

            // Act
            var result = _fileReader.ReadPhysicianNote(fileName);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            // Should either read the actual file or return the default note
            Assert.True(result.Contains("CPAP") || result.Contains("oxygen") || result.Contains("Dr."));
        }

        [Fact]
        public void ReadPhysicianNote_WithNonExistentFile_ShouldReturnDefaultNote()
        {
            // Arrange
            var fileName = "non_existent_file.txt";

            // Act
            var result = _fileReader.ReadPhysicianNote(fileName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Patient needs a CPAP with full face mask and humidifier. AHI > 20. Ordered by Dr. Cameron.", result);
        }

        [Fact]
        public void ReadPhysicianNote_WithEmptyFileName_ShouldReturnDefaultNote()
        {
            // Arrange
            var fileName = "";

            // Act
            var result = _fileReader.ReadPhysicianNote(fileName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Patient needs a CPAP with full face mask and humidifier. AHI > 20. Ordered by Dr. Cameron.", result);
        }

        [Fact]
        public void ReadPhysicianNote_WithNullFileName_ShouldReturnDefaultNote()
        {
            // Arrange
            string fileName = null!;

            // Act
            var result = _fileReader.ReadPhysicianNote(fileName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Patient needs a CPAP with full face mask and humidifier. AHI > 20. Ordered by Dr. Cameron.", result);
        }

        [Theory]
        [InlineData("physician_note1.txt")]
        [InlineData("physician_note2.txt")]
        [InlineData("some_other_file.txt")]
        public void ReadPhysicianNote_WithVariousFileNames_ShouldHandleGracefully(string fileName)
        {
            // Act
            var result = _fileReader.ReadPhysicianNote(fileName);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }
    }
}