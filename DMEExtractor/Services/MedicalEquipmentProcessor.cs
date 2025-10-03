using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DMEExtractor.Interfaces;
using System.Text.Json;
using System.Text.Json.Serialization;
using DMEExtractor.Models;
using Microsoft.Extensions.Logging;

namespace DMEExtractor.Services
{
    /// <summary>
    /// Processes physician notes to extract structured medical equipment data.
    /// </summary>
    public class MedicalEquipmentProcessor : IMedicalEquipmentProcessor
    {
        // Regex patterns for extracting information from physician notes
        private static class RegexPatterns
        {
            public const string OrderingPhysician = @"Ordering Physician:\s*([^\r\n]+)";
            public const string PatientName = @"Patient Name:\s*([^\r\n]+)";
            public const string PatientAlternate = @"Patient:\s*([^\r\n]+)";
            public const string PatientNameJson = @"Patient Name:\s*([^\\]+?)\\n";
            public const string DateOfBirth = @"DOB:\s*(\d{2}/\d{2}/\d{4})";
            public const string Diagnosis = @"Diagnosis:\s*([^\r\n]+)";
            public const string DiagnosisJson = @"Diagnosis:\s*([^\\]+?)\\n";
            public const string OxygenLiters = @"(\d+(?:\.\d+)?)\s?L";
        }
        
        private readonly ILogger<MedicalEquipmentProcessor> _logger;
        
        // this should probably be an enum or config-driven in a real app
        private readonly Dictionary<string, string> _deviceKeywords = new(StringComparer.OrdinalIgnoreCase)
        {
            { "CPAP", "CPAP" },
            { "oxygen", "Oxygen Tank" },
            { "wheelchair", "Wheelchair" }
        };
        
        public MedicalEquipmentProcessor(ILogger<MedicalEquipmentProcessor> logger)
        {
            _logger = logger;
        }
        
        /// <summary>
        /// Extracts structured medical equipment data from a physician note.
        /// </summary>
        /// <param name="physicianNote">The raw physician note text</param>
        /// <returns>Structured medical equipment data</returns>
        public MedicalEquipmentData ExtractEquipmentData(string physicianNote)
        {
            if (string.IsNullOrWhiteSpace(physicianNote))
                throw new ArgumentException("Physician note cannot be null or empty", nameof(physicianNote));
                
            _logger.LogInformation("Extracting medical equipment data from physician note");
            
            var equipmentData = new MedicalEquipmentData
            {
                DeviceType = ExtractDeviceType(physicianNote),
                OrderingProvider = ExtractOrderingProvider(physicianNote),
                PatientName = ExtractPatientName(physicianNote),
                DateOfBirth = ExtractDateOfBirth(physicianNote),
                Diagnosis = ExtractDiagnosis(physicianNote)
            };
            
            // Extract device-specific information for oxygen tanks
            if (equipmentData.DeviceType == "Oxygen Tank")
            {
                equipmentData.OxygenLiters = ExtractOxygenLiters(physicianNote);
                equipmentData.OxygenUsage = ExtractOxygenUsage(physicianNote);
            }
            
            // Output the extracted data for verification
            _logger.LogInformation("Successfully extracted equipment data");
            var jsonOptions = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                WriteIndented = true
            };
            var equipmentJson = System.Text.Json.JsonSerializer.Serialize(
                equipmentData,
                jsonOptions
            );
            _logger.LogDebug("Extracted data: {EquipmentData}", equipmentJson);
            
            // Also output to console for immediate visibility
            Console.WriteLine("Extracted equipment data:");
            Console.WriteLine(equipmentJson);
            
            return equipmentData;
        }
        
        private string ExtractDeviceType(string note)
        {
            foreach (var (keyword, deviceType) in _deviceKeywords)
            {
                if (note.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation("Detected device type: {DeviceType}", deviceType);
                    return deviceType;
                }
            }
            
            _logger.LogWarning("No recognized device type found, defaulting to 'Unknown'");
            return "Unknown";
        }
        
        private string ExtractOrderingProvider(string note)
        {
            // Check for "Ordering Physician: <doctorname>" first
            var physicianMatch = Regex.Match(note, RegexPatterns.OrderingPhysician, RegexOptions.IgnoreCase);
            if (physicianMatch.Success)
            {
                var provider = physicianMatch.Groups[1].Value.Trim();
                _logger.LogInformation("Detected ordering provider: {Provider}", provider);
                return provider;
            }

            // Fallback: look for "Dr."
            var doctorIndex = note.IndexOf("Dr.", StringComparison.OrdinalIgnoreCase);
            if (doctorIndex >= 0)
            {
                var provider = note.Substring(doctorIndex)
                    .Replace("Ordered by ", "", StringComparison.OrdinalIgnoreCase)
                    .Trim('.', '\n', '\r');
                _logger.LogInformation("Detected ordering provider: {Provider}", provider);
                return provider;
            }
            
            _logger.LogWarning("No ordering provider found, defaulting to 'Unknown'");
            return "Unknown";
        }
        
        private string? ExtractOxygenLiters(string note)
        {
            var literMatch = Regex.Match(note, RegexPatterns.OxygenLiters, RegexOptions.IgnoreCase);
            if (literMatch.Success)
            {
                var liters = literMatch.Groups[1].Value + " L";
                _logger.LogInformation("Detected oxygen liters: {OxygenLiters}", liters);
                return liters;
            }
            return null;
        }
        
        private string? ExtractOxygenUsage(string note)
        {
            var hasSleep = note.Contains("sleep", StringComparison.OrdinalIgnoreCase);
            var hasExertion = note.Contains("exertion", StringComparison.OrdinalIgnoreCase);
            
            if (hasSleep && hasExertion)
            {
                _logger.LogInformation("Detected oxygen usage: sleep and exertion");
                return "sleep and exertion";
            }
            else if (hasSleep)
            {
                _logger.LogInformation("Detected oxygen usage: sleep");
                return "sleep";
            }
            else if (hasExertion)
            {
                _logger.LogInformation("Detected oxygen usage: exertion");
                return "exertion";
            }
            
            return null;
        }
        
        private string? ExtractPatientName(string note)
        {
            // Try "Patient Name: <name>" format first
            var nameMatch = Regex.Match(note, RegexPatterns.PatientName, RegexOptions.IgnoreCase);
            if (nameMatch.Success)
            {
                var patientName = nameMatch.Groups[1].Value.Trim();
                _logger.LogInformation("Detected patient name: {PatientName}", patientName);
                return patientName;
            }
            
            // Try "Patient: <name>" format (used in tests)
            nameMatch = Regex.Match(note, RegexPatterns.PatientAlternate, RegexOptions.IgnoreCase);
            if (nameMatch.Success)
            {
                var patientName = nameMatch.Groups[1].Value.Trim();
                _logger.LogInformation("Detected patient name: {PatientName}", patientName);
                return patientName;
            }
            
            // Also check for JSON format (like physician_note2.txt) - look for name before \n
            var jsonNameMatch = Regex.Match(note, RegexPatterns.PatientNameJson, RegexOptions.IgnoreCase);
            if (jsonNameMatch.Success)
            {
                var patientName = jsonNameMatch.Groups[1].Value.Trim();
                _logger.LogInformation("Detected patient name: {PatientName}", patientName);
                return patientName;
            }
            
            return null;
        }
        
        private string? ExtractDateOfBirth(string note)
        {
            // Standard format: "DOB: MM/DD/YYYY"
            var dobMatch = Regex.Match(note, RegexPatterns.DateOfBirth, RegexOptions.IgnoreCase);
            if (dobMatch.Success)
            {
                var dob = dobMatch.Groups[1].Value;
                _logger.LogInformation("Detected date of birth: {DateOfBirth}", dob);
                return dob;
            }
            
            return null;
        }
        
        private string? ExtractDiagnosis(string note)
        {
            // Standard format: "Diagnosis: <diagnosis>"
            var diagnosisMatch = Regex.Match(note, RegexPatterns.Diagnosis, RegexOptions.IgnoreCase);
            if (diagnosisMatch.Success)
            {
                var diagnosis = diagnosisMatch.Groups[1].Value.Trim();
                _logger.LogInformation("Detected diagnosis: {Diagnosis}", diagnosis);
                return diagnosis;
            }
            
            // Also check for JSON format - look for diagnosis before \n
            var jsonDiagnosisMatch = Regex.Match(note, RegexPatterns.DiagnosisJson, RegexOptions.IgnoreCase);
            if (jsonDiagnosisMatch.Success)
            {
                var diagnosis = jsonDiagnosisMatch.Groups[1].Value.Trim();
                _logger.LogInformation("Detected diagnosis: {Diagnosis}", diagnosis);
                return diagnosis;
            }
            
            return null;
        }
    }
}