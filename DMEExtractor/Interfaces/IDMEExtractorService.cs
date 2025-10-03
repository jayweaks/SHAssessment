using System.Threading.Tasks;

namespace DMEExtractor.Interfaces
{
    /// <summary>
    /// Interface for the main DMEExtractor application service.
    /// </summary>
    public interface IDMEExtractorService
    {
        /// <summary>
        /// Runs the medical equipment extraction process.
        /// </summary>
        /// <param name="fileName">The name of the physician note file to process</param>
        /// <returns>Task representing the async operation</returns>
        Task<int> RunAsync(string fileName);
    }
}