using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using DMEExtractor.Interfaces;
using DMEExtractor.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace DMEExtractor.Services
{
    /// <summary>
    /// Handles reading physician notes from files with proper error handling and logging.
    /// </summary>
    public class PhysicianNoteFileReader : IPhysicianNoteFileReader
    {
        private const string DefaultPhysicianNote = "Patient needs a CPAP with full face mask and humidifier. AHI > 20. Ordered by Dr. Cameron.";
        private const string JsonDataField = "data";
        
        private readonly AppSettings _appSettings;
        private readonly ILogger<PhysicianNoteFileReader> _logger;
        
        public PhysicianNoteFileReader(IOptions<AppSettings> appSettings, ILogger<PhysicianNoteFileReader> logger)
        {
            _appSettings = appSettings.Value;
            _logger = logger;
        }
        
        /// <summary>
        /// Reads a physician note from the specified file.
        /// </summary>
        /// <param name="fileName">The name of the file to read from the data/input directory</param>
        /// <returns>The content of the physician note</returns>
        public string ReadPhysicianNote(string fileName)
        {
            try
            {
                var filePath = GetDataFilePath(fileName);
                _logger.LogInformation("Reading physician note from: {FilePath}", filePath);
                
                if (!File.Exists(filePath))
                {
                    _logger.LogWarning("File not found: {FilePath}. Using default note", filePath);
                    return DefaultPhysicianNote;
                }
                
                var content = File.ReadAllText(filePath);
                _logger.LogInformation("Successfully read {ContentLength} characters from file: {FileName}", content.Length, fileName);

                // Check if the content is JSON and extract the data field if it exists               
                content = ExtractDataFromJsonIfNeeded(content);
                
                return content;
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogWarning(ex, "File not found: {FileName}. Using default note", fileName);
                return DefaultPhysicianNote;
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Access denied reading file: {FileName}. Using default note", fileName);
                return DefaultPhysicianNote;
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "IO error reading file: {FileName}. Using default note", fileName);
                return DefaultPhysicianNote;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error reading physician note from {FileName}. Using default note", fileName);
                return DefaultPhysicianNote;
            }
        }
        
        private string GetDataFilePath(string fileName)
        {
            // Check if fileName is already an absolute path
            if (Path.IsPathRooted(fileName))
            {
                return fileName;
            }
            
            // For relative paths, always resolve from current working directory
            return Path.GetFullPath(fileName);
        }
        
        /// <summary>
        /// Extracts the data field from JSON content if the content is in JSON format.
        /// </summary>
        /// <param name="content">The raw file content</param>
        /// <returns>The extracted data or original content if not JSON</returns>
        private string ExtractDataFromJsonIfNeeded(string content)
        {
            try
            {
                // Try to parse as JSON
                var json = JObject.Parse(content);
                
                // If it has a "data" field, extract and return it
                if (json[JsonDataField] != null)
                {
                    var dataContent = json[JsonDataField]?.ToString();
                    if (!string.IsNullOrEmpty(dataContent))
                    {
                        _logger.LogInformation("Extracted data from JSON format file");
                        return dataContent;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Content is not valid JSON, using as plain text");
            }
            
            return content;
        }
    }
}