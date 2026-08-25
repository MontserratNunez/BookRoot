using Application.Dtos.Book;
using Application.Interfaces;
using System.Net.Http.Json;
using System.Security.Cryptography;

namespace Infraestructure
{
    public class OpenLibraryService : IOpenLibraryService
    {
        private readonly HttpClient _http;

        public OpenLibraryService(HttpClient http)
        {
            _http = http;
        }

        public async Task<BookInfoDto?> GetByWork(string workId)
        {
            if (string.IsNullOrWhiteSpace(workId))
            {
                return null;
            }

            var url = $"https://openlibrary.org/works/{Uri.EscapeDataString(workId)}.json";
            OpenLibraryWorkResponse? response = null;

            try
            {
                response = await _http.GetFromJsonAsync<OpenLibraryWorkResponse>(url);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
            catch (Exception)
            {
                return null;
            }

            if (response == null)
                return null;

            string? authorName = null;

            var firstAuthor = response.Authors?.FirstOrDefault()?.Author;
            if (firstAuthor != null && !string.IsNullOrEmpty(firstAuthor.Key))
            {
                var authorUrl = $"https://openlibrary.org{firstAuthor.Key}.json";
                try
                {
                    var authorResponse = await _http.GetFromJsonAsync<OpenLibraryAuthorResponse>(authorUrl);
                    authorName = authorResponse?.PersonalName ?? authorResponse?.Name;
                }
                catch (Exception)
                {
                    authorName = "Autor Desconocido";
                }
            }

            return new BookInfoDto
            {
                Title = response.Title,
                Author = authorName,
                FirstPublishYear = response.FirstPublishYear,
                BookWorkKey = workId,
                CoverEditionKey = response.Covers?.FirstOrDefault().ToString()
            };
        }

        public async Task<List<BookInfoDto>> GetBooks(string search)
        {
            var url = $"https://openlibrary.org/search.json?q={Uri.EscapeDataString(search)}";

            var response = await _http.GetFromJsonAsync<OpenLibraryResponse>(url);

            if (response?.Docs == null)
                return new List<BookInfoDto>();

            return response.Docs.Take(10)
                .Select(doc => new BookInfoDto
                {
                    Title = doc.title,
                    Author = doc.author_name?.FirstOrDefault(),
                    FirstPublishYear = doc.first_publish_year,
                    BookWorkKey = doc.key?.Split('/').LastOrDefault() ?? doc.key,
                    CoverEditionKey = doc.cover_edition_key ?? doc.cover_i?.ToString()
                })
                .ToList();
        }

        public Task<string?> GetCover(string? id, string size)
        {
            if (string.IsNullOrEmpty(id)) return Task.FromResult<string?>(null);

            var lastChar = id[id.Length - 1];

            if (char.IsLetter(lastChar))
            {
                return Task.FromResult<string?>($"https://covers.openlibrary.org/b/olid/{id}-{size}.jpg");
            }

            else if (char.IsDigit(lastChar))
            {
                return Task.FromResult<string?>($"https://covers.openlibrary.org/b/id/{id}-{size}.jpg");
            }

            return Task.FromResult<string?>(null);
        }
    }

    // --- Modelos de mapeo de la API ---

    public class OpenLibraryResponse
    {
        public List<OpenLibraryDoc> Docs { get; set; } = new();
    }

    public class OpenLibraryDoc
    {
        public string title { get; set; } = string.Empty;
        public List<string>? author_name { get; set; }
        public int first_publish_year { get; set; }
        public string? cover_edition_key { get; set; }
        public string key { get; set; } = string.Empty;
        public int? cover_i { get; set; }
    }

    // Nuevas clases para mapear el endpoint /works/{id}.json
    public class OpenLibraryWorkResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [System.Text.Json.Serialization.JsonPropertyName("authors")]
        public List<OpenLibraryWorkAuthorContainer>? Authors { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("first_publish_year")]
        public int FirstPublishYear { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("key")]
        public string Key { get; set; } = string.Empty;

        [System.Text.Json.Serialization.JsonPropertyName("covers")]
        public List<int>? Covers { get; set; }
    }

    public class OpenLibraryWorkAuthorContainer
    {
        [System.Text.Json.Serialization.JsonPropertyName("author")]
        public OpenLibraryWorkAuthor? Author { get; set; }
    }

    public class OpenLibraryWorkAuthor
    {
        [System.Text.Json.Serialization.JsonPropertyName("key")]
        public string Key { get; set; } = string.Empty; // Devuelve algo como "/authors/OL26320A"
    }

    // Nueva clase para mapear el endpoint /authors/{id}.json
    public class OpenLibraryAuthorResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("name")]
        public string? Name { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("personal_name")]
        public string? PersonalName { get; set; }
    }
}