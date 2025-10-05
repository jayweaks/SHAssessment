namespace DMEExtractor.Interfaces;

/// <summary>
/// Interface for reading physician notes from various sources.
/// </summary>
public interface IPhysicianNoteFileReader
{
    /// <summary>
    /// Reads a physician note from the specified file.
    /// </summary>
    /// <param name="fileName">The name of the file to read from the data/input directory</param>
    /// <returns>The content of the physician note</returns>
    string ReadPhysicianNote(string fileName);
}