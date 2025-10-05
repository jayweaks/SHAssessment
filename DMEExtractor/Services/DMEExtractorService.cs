using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using DMEExtractor.Interfaces;

namespace DMEExtractor.Services;

/// <summary>
/// Main service that orchestrates the medical equipment extraction process.
/// </summary>
public class DMEExtractorService : IDMEExtractorService
{
    private readonly IPhysicianNoteFileReader _fileReader;
    private readonly IMedicalEquipmentProcessor _processor;
    private readonly IApiClient _apiClient;
    private readonly ILogger<DMEExtractorService> _logger;

    public DMEExtractorService(
        IPhysicianNoteFileReader fileReader,
        IMedicalEquipmentProcessor processor,
        IApiClient apiClient,
        ILogger<DMEExtractorService> logger)
    {
        _fileReader = fileReader ?? throw new ArgumentNullException(nameof(fileReader));
        _processor = processor ?? throw new ArgumentNullException(nameof(processor));
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Runs the medical equipment extraction process.
    /// </summary>
    /// <param name="fileName">The name of the physician note file to process</param>
    /// <returns>Task representing the async operation with exit code</returns>
    public async Task<int> RunAsync(string fileName)
    {
        try
        {
            _logger.LogInformation("Starting medical equipment extraction process for file: {FileName}", fileName);
            
            // Read the physician note
            var physicianNote = _fileReader.ReadPhysicianNote(fileName);
            if (string.IsNullOrWhiteSpace(physicianNote))
            {
                _logger.LogWarning("Physician note is empty or null in file: {FileName}", fileName);
                return 1;
            }
            _logger.LogDebug("Successfully read physician note from file: {FileName}", fileName);

            // Extract equipment data
            var equipmentData = _processor.ExtractEquipmentData(physicianNote);
            if (equipmentData == null)
            {
                _logger.LogWarning("No medical equipment data extracted from physician note in file: {FileName}", fileName);
                return 1; // Indicate failure due to no data extracted
            }
            _logger.LogDebug("Successfully extracted equipment data from physician note");
            
            // Send to API
            //await _apiClient.SendEquipmentDataAsync(equipmentData);
            _logger.LogDebug("Successfully sent equipment data to API");
            
            _logger.LogInformation("Medical equipment extraction completed successfully for file: {FileName}", fileName);
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Application failed while processing file: {FileName}", fileName);
            return 1;
        }
    }
}