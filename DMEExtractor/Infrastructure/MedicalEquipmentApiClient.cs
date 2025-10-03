using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DMEExtractor.Models;
using DMEExtractor.Interfaces;
using DMEExtractor.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DMEExtractor.Infrastructure
{
    /// <summary>
    /// Handles communication with the external medical equipment API.
    /// </summary>
    public class MedicalEquipmentApiClient : IApiClient
    {
        private readonly string _apiEndpoint;
        private readonly HttpClient _httpClient;
        private readonly ILogger<MedicalEquipmentApiClient> _logger;
        
        public MedicalEquipmentApiClient(IHttpClientFactory httpClientFactory, AppSettings settings, ILogger<MedicalEquipmentApiClient> logger)
        {
            _apiEndpoint = settings?.ApiEndpoint ?? "https://alert-api.com/DrExtract";
            _httpClient = httpClientFactory.CreateClient();
            _logger = logger;
        }
        
        /// <summary>
        /// Sends extracted medical equipment data to the external API.
        /// </summary>
        /// <param name="equipmentData">The medical equipment data to send</param>
        public async Task SendEquipmentDataAsync(MedicalEquipmentData equipmentData)
        {
            try
            {
                if (equipmentData == null)
                {
                    throw new ArgumentNullException(nameof(equipmentData), "Equipment data cannot be null");
                }
                
                var jsonOptions = new JsonSerializerOptions
                {
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    WriteIndented = true
                };
                var json = JsonSerializer.Serialize(equipmentData, jsonOptions);
                
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                _logger.LogInformation("Sending equipment data to API: {ApiEndpoint}", _apiEndpoint);
                _logger.LogDebug("Payload: {JsonPayload}", json);
                
                var response = await _httpClient.PostAsync(_apiEndpoint, content);
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Equipment data sent successfully to API");
                }
                else
                {
                    _logger.LogError("API call failed with status: {StatusCode}", response.StatusCode);
                    throw new HttpRequestException($"API request failed: {response.StatusCode}");
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error sending data to API: {ApiEndpoint}", _apiEndpoint);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error sending data to API: {ApiEndpoint}", _apiEndpoint);
                throw;
            }
        }
    }
}