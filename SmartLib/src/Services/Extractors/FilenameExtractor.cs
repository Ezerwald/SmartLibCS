using System.Text.RegularExpressions;

namespace SmartLib.src.Services.Extractors
{
    public class FilenameExtractor
    {
        /// <summary>
        /// Extracts the book title and author from a given filename.
        /// </summary>
        public static BookInfo ExtractBookInfo(string filename)
        {
            // Remove file extension
            filename = Regex.Replace(filename, @"\.[a-zA-Z0-9]+$", "");

            // Common patterns
            string[] patterns =
            {
            @"^(.*?) - (.*?)$",           // e.g., "BookTitle - Author"
            @"^(.*?) - (.*?) -",          // e.g., "BookTitle - Author - ..."
            @"^(.*?) \((.*?)\)$",         // e.g., "BookTitle (Author)"
            @"^(.*?) - (.*?) -",          // e.g., "Author - BookTitle"
            @"^(.*?) by (.*?)$",          // e.g., "BookTitle by Author"
            @"^(.*?) -- (.*?)$",          // e.g., "BookTitle -- Author"
        };

            foreach (var pattern in patterns)
            {
                var match = Regex.Match(filename, pattern);
                if (match.Success)
                {
                    // Return the first two matching groups
                    return new BookInfo { Title = match.Groups[1].Value.Trim(), Author = match.Groups[2].Value.Trim() };
                }
            }

            // If no pattern matches, return the filename and null
            return new BookInfo { Title = filename.Trim(), Author = null };
        }
    }
}