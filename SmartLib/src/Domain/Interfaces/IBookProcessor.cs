namespace SmartLib.src.Domain.Interfaces
{
    public interface IBookProcessor
    {
        string FolderPath { get; set; }

        /// <summary>
        /// Processes all digital books in the specified folder and adds their information to the database.
        /// </summary>
        Task ProcessAllBooksAsync();

        /// <summary>
        /// Recursively collects all file paths of digital books within a folder.
        /// </summary>
        /// <param name="folderPath">The path to the folder.</param>
        /// <returns>A list of all file paths of digital books within the folder.</returns>
        List<string> GetAllBookFiles(string folderPath);

        /// <summary>
        /// Checks if a file is a digital book based on its file extension and MIME type.
        /// </summary>
        /// <param name="filepath">The path to the file.</param>
        /// <returns>True if the file is a digital book, False otherwise.</returns>
        bool IsDigitalBook(string filepath);

        /// <summary>
        /// Extracts book information from a digital book file and adds it to the database.
        /// </summary>
        /// <param name="filepath">The path to the digital book file to be processed.</param>
        Task ProcessBookAsync(string filepath);
    }
}
