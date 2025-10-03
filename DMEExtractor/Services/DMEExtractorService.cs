using System;
using System.Threading.Tasks;
using DMEExtractor.Interfaces;

namespace DMEExtractor.Services
{
    /// <summary>
    /// Main service that orchestrates the medical equipment extraction process.
    /// </summary>
    public class DMEExtractorService : IDMEExtractorService
{
    private readonly IPhysicianNoteFileReader _fileReader;
    private readonly IMedicalEquipmentProcessor _processor;
    private readonly IApiClient _apiClient;

    public DMEExtractorService(
        IPhysicianNoteFileReader fileReader,
        IMedicalEquipmentProcessor processor,
        IApiClient apiClient)
    {
        _fileReader = fileReader ?? throw new ArgumentNullException(nameof(fileReader));
        _processor = processor ?? throw new ArgumentNullException(nameof(processor));
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
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
            Console.WriteLine("Starting medical equipment extraction process...");
            
            // Read the physician note
            var physicianNote = _fileReader.ReadPhysicianNote(fileName);
            
            // Extract equipment data
            var equipmentData = _processor.ExtractEquipmentData(physicianNote);
            
            // Send to API
            //await _apiClient.SendEquipmentDataAsync(equipmentData);
            
            Console.WriteLine("Medical equipment extraction completed successfully.");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Application failed: {ex.Message}");
            return 1;
        }
    }
}
}