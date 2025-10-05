using System.Threading.Tasks;
using DMEExtractor.Models;

namespace DMEExtractor.Interfaces;

/// <summary>
/// Interface for communicating with external medical equipment APIs.
/// </summary>
public interface IApiClient
{
    /// <summary>
    /// Sends extracted medical equipment data to the external API.
    /// </summary>
    /// <param name="equipmentData">The medical equipment data to send</param>
    Task SendEquipmentDataAsync(MedicalEquipmentData equipmentData);
}