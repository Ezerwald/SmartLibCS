using System.Net.Http;
using System.Text.Json;

namespace SmartLib.src.Services.Fetchers
{
    public class GoogleBooksFetcher
    {
        private static readonly HttpClient HttpClient = new HttpClient();

        /// <summary>
        /// Fetches book information from Google Books API based on the provided query.
        /// </summary>
        /// <param name="query">The search query to retrieve book information.</param>
        /// <returns>A BookInfo object containing title and authors if found; otherwise, null.</returns>
        public static async Task<BookInfo> FetchBookInfo(string query)
        {
            var apiUrl = $"https://www.googleapis.com/books/v1/volumes?q={query}";

            try
            {
                // Send GET request to Google Books API
                var response = await HttpClient.GetAsync(apiUrl);
                response.EnsureSuccessStatusCode(); // Ensure HTTP 200 OK response

                // Read and parse response data
                var responseData = await response.Content.ReadAsStringAsync();
                var data = JsonDocument.Parse(responseData).RootElement;

                // Check if 'items' array exists and has at least one item
                if (data.TryGetProperty("items", out var items) && items.GetArrayLength() > 0)
                {
                    // Extract book information from the first item
                    var item = items[0].GetProperty("volumeInfo");
                    var title = item.GetProperty("title").GetString();
                    var authors = item.GetProperty("authors").EnumerateArray();
                    var authorList = string.Join(", ", authors);

                    return new BookInfo { Title = title, Author = authorList };
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Google Books API request failed: {e.Message}");
            }
            catch (JsonException e)
            {
                Console.WriteLine($"Error parsing JSON response: {e.Message}");
            }

            // Return null if no valid book information is found or if there's an error
            return null;
        }
    }
}