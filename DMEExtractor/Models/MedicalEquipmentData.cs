using System.Text.Json.Serialization;

namespace DMEExtractor.Models
{
    public class MedicalEquipmentData
    {
        [JsonPropertyName("device")]
        public string? DeviceType { get; set; }

        [JsonPropertyName("liters")]
        public string? OxygenLiters { get; set; }

        [JsonPropertyName("usage")]
        public string? OxygenUsage { get; set; }

        [JsonPropertyName("diagnosis")]
        public string? Diagnosis { get; set; }

        [JsonPropertyName("ordering_provider")]
        public string? OrderingProvider { get; set; }

        [JsonPropertyName("patient_name")]
        public string? PatientName { get; set; }

        [JsonPropertyName("dob")]
        public string? DateOfBirth { get; set; }
    }
}