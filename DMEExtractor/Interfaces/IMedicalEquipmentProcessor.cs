using DMEExtractor.Models;

namespace DMEExtractor.Interfaces
{
    /// <summary>
    /// Interface for processing physician notes to extract medical equipment data.
    /// </summary>
    public interface IMedicalEquipmentProcessor
    {
        /// <summary>
        /// Extracts structured medical equipment data from a physician note.
        /// </summary>
        /// <param name="physicianNote">The raw physician note text</param>
        /// <returns>Structured medical equipment data</returns>
        MedicalEquipmentData ExtractEquipmentData(string physicianNote);
    }
}