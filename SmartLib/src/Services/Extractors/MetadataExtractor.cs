using System;
using System.IO;
using System.Threading.Tasks;
using UglyToad.PdfPig;

namespace SmartLib.src.Services.BookInfoExtractor
{
    public class MetadataExtractor
    {
        /// <summary>
        /// Extracts metadata from a PDF file.
        /// </summary>
        public static async Task<BookInfo> ExtractBookInfo(string filepath)
        {
            try
            {
                using (var document = PdfDocument.Open(filepath))
                {
                    var info = document.Information;
                    string title = info.Title ?? null;
                    string author = info.Author ?? null;
                    return new BookInfo { Title = title, Author = author };
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to extract metadata from {filepath}: {e.Message}");
                return new BookInfo { Title = null, Author = null };
            }
        }
    }
}