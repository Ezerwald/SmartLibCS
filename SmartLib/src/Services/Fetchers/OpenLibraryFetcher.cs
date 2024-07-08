using System.Net.Http;
using System.Text.Json;

namespace SmartLib.src.Services.Fetchers
{
    public class OpenLibraryFetcher
    {
        private static readonly HttpClient HttpClient = new HttpClient();

        /// <summary>
        /// Fetches book info from OpenLibrary API.
        /// </summary>
        public static async Task<BookInfo> FetchBookInfo(string query)
        {
            var apiUrl = $"https://openlibrary.org/search.json?q={query}";

            try
            {
                var response = await HttpClient.GetAsync(apiUrl);
                response.EnsureSuccessStatusCode();
                var responseData = await response.Content.ReadAsStringAsync();
                var data = JsonDocument.Parse(responseData).RootElement;

                if (data.TryGetProperty("docs", out var docs) && docs.GetArrayLength() > 0)
                {
                    var doc = docs[0];
                    var title = doc.GetProperty("title").GetString();
                    var authors = doc.GetProperty("author_name").EnumerateArray();
                    var authorList = string.Join(", ", authors);

                    return new BookInfo { Title = title, Author = authorList };
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"OpenLibrary API request failed: {e.Message}");
            }
            return null;
        }
    }
}