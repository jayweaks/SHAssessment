using System.ComponentModel.DataAnnotations;

namespace DMEExtractor.Configuration
{
    public class AppSettings
    {
        [Required, Url]
        public string ApiEndpoint { get; set; } = string.Empty;
        [Required]
        public string BaseInputDirectory { get; set; } = "../../../../data/input";
    }
}