using MyDeezerStats.Application.Interfaces;
using MyDeezerStats.Domain.Entities.DeezerInfos;
using System.Net.Http.Json;
using System.Text.Json;
using System.Web;
using Microsoft.Extensions.Logging;
using System.Net;

namespace MyDeezerStats.Application.DeezerServices
{
    public class DeezerService : IDeezerService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<DeezerService> _logger;
        private const string DeezerApiBaseUrl = "https://api.deezer.com";

        public DeezerService(HttpClient httpClient, ILogger<DeezerService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task EnrichAlbumWithDeezerData(AlbumInfos? album)
        {
            if (album is null)
            {
                _logger.LogWarning("Album is null, skipping enrichment.");
                return;
            }

            var query = HttpUtility.UrlEncode($"{album.Artist} {album.Title}");
            var searchUrl = $"{DeezerApiBaseUrl}/search/album?q={query}";

            _logger.LogInformation("Searching album: {Query}", query);

            var searchData = await GetJsonArrayFromUrl(searchUrl);
            if (searchData is null || searchData.Value.ValueKind != JsonValueKind.Array || !searchData.Value.EnumerateArray().Any())
            {
                _logger.LogWarning("No search data found for album: {Query}", query);
                return;
            }

            foreach (var item in searchData.Value.EnumerateArray())
            {
                if (item.TryGetProperty("title", out var albumTitleProp) &&
                    item.TryGetProperty("artist", out var artistProp) &&
                    artistProp.TryGetProperty("name", out var artistNameProp))
                {
                    var albumTitle = albumTitleProp.GetString();
                    var artistName = artistNameProp.GetString();

                    _logger.LogDebug("Found album: {AlbumTitle} by {ArtistName}", albumTitle, artistName);

                    if (string.Equals(albumTitle, album.Title, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(artistName, album.Artist, StringComparison.OrdinalIgnoreCase))
                    {
                        if (item.TryGetProperty("id", out var albumIdProp))
                        {
                            var albumId = albumIdProp.GetInt32();
                            var albumDetailsUrl = $"{DeezerApiBaseUrl}/album/{albumId}";

                            _logger.LogInformation("Match found, retrieving album details from: {Url}", albumDetailsUrl);

                            var albumDetails = await GetJsonFromUrl(albumDetailsUrl);
                            if (albumDetails.HasValue)
                            {
                                // Protection contre les clés manquantes dans les réponses
                                if (albumDetails.Value.TryGetProperty("cover_big", out var coverProp))
                                {
                                    album.AlbumUrl = coverProp.GetString() ?? string.Empty;
                                }

                                if (albumDetails.Value.TryGetProperty("duration", out var durationProp))
                                {
                                    album.Duration = durationProp.GetInt32();
                                }

                                _logger.LogInformation("Album enriched with cover and duration.");
                            }
                            else
                            {
                                _logger.LogWarning("No album details found for album ID: {AlbumId}", albumId);
                            }
                        }
                        else
                        {
                            _logger.LogWarning("No album ID found for album: {AlbumTitle} by {ArtistName}", albumTitle, artistName);
                        }
                    }
                }
                else
                {
                    _logger.LogWarning("Missing expected properties in search result for album: {AlbumTitle} by {ArtistName}", album.Title, album.Artist);
                }
            }
        }


    


      /**  public async Task EnrichAlbumWithDeezerData(AlbumInfos? album)
        {
            if (album is null)
                return;

            var query = HttpUtility.UrlEncode($"{album.Artist} {album.Title}");
            var searchUrl = $"{DeezerApiBaseUrl}/search/album?q={query}";

            _logger.LogInformation("Searching album: {Query}", query);

            var searchData = await GetJsonArrayFromUrl(searchUrl);
            if (searchData is null)
            {
                _logger.LogWarning("No search data found for album: {Query}", query);
                return;
            }

            foreach (var item in searchData.Value.EnumerateArray())
            {
                var albumTitle = item.GetProperty("title").GetString();
                var artistName = item.GetProperty("artist").GetProperty("name").GetString();

                _logger.LogDebug("Found album: {AlbumTitle} by {ArtistName}", albumTitle, artistName);

                if (string.Equals(albumTitle, album.Title, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(artistName, album.Artist, StringComparison.OrdinalIgnoreCase))
                {
                    var albumId = item.GetProperty("id").GetInt32();
                    var albumDetailsUrl = $"{DeezerApiBaseUrl}/album/{albumId}";

                    _logger.LogInformation("Match found, retrieving album details from: {Url}", albumDetailsUrl);

                    var albumDetails = await GetJsonFromUrl(albumDetailsUrl);
                    if (albumDetails.HasValue)
                    {
                        album.AlbumUrl = albumDetails.Value.GetProperty("cover_big").GetString() ?? string.Empty;
                        album.Duration = albumDetails.Value.GetProperty("duration").GetInt32();

                        _logger.LogInformation("Album enriched with cover and duration.");
                    }

                    break;
                }
            }
        }**/

        /**public async Task EnrichAlbumWithDeezerData(AlbumInfos? album)
        {
            if (album is null)
                return;

            var query = HttpUtility.UrlEncode($"{album.Artist} {album.Title}");
            var searchUrl = $"{DeezerApiBaseUrl}/search/album?q={query}";

            _logger.LogInformation("Searching album: {Query}", query);

            var searchData = await GetJsonArrayFromUrl(searchUrl);
            if (searchData is null)
            {
                _logger.LogWarning("No search data found for album: {Query}", query);
                return;
            }

            foreach (var item in searchData.Value.EnumerateArray())
            {
                if (item.TryGetProperty("title", out var albumTitleProp) && albumTitleProp.ValueKind == JsonValueKind.String)
                {
                    var albumTitle = albumTitleProp.GetString();
                    if (item.TryGetProperty("artist", out var artistProp) && artistProp.TryGetProperty("name", out var artistNameProp))
                    {
                        var artistName = artistNameProp.GetString();

                        _logger.LogDebug("Found album: {AlbumTitle} by {ArtistName}", albumTitle, artistName);

                        if (string.Equals(albumTitle, album.Title, StringComparison.OrdinalIgnoreCase) &&
                            string.Equals(artistName, album.Artist, StringComparison.OrdinalIgnoreCase))
                        {
                            if (item.TryGetProperty("id", out var albumIdProp) && albumIdProp.ValueKind == JsonValueKind.Number)
                            {
                                var albumId = albumIdProp.GetInt32();
                                var albumDetailsUrl = $"{DeezerApiBaseUrl}/album/{albumId}";

                                _logger.LogInformation("Match found, retrieving album details from: {Url}", albumDetailsUrl);

                                var albumDetails = await GetJsonFromUrl(albumDetailsUrl);
                                if (albumDetails.HasValue)
                                {
                                    if (albumDetails.Value.TryGetProperty("cover_big", out var coverUrlProp))
                                    {
                                        album.AlbumUrl = coverUrlProp.GetString() ?? string.Empty;
                                    }
                                    if (albumDetails.Value.TryGetProperty("duration", out var durationProp) && durationProp.ValueKind == JsonValueKind.Number)
                                    {
                                        album.Duration = durationProp.GetInt32();
                                    }

                                    _logger.LogInformation("Album enriched with cover and duration.");
                                }
                            }
                        }
                    }
                }
            }
        }**/


        public async Task EnrichTrackWithDeezerData(TrackInfos? track)
        {
            if (track == null || string.IsNullOrWhiteSpace(track.Title) || string.IsNullOrWhiteSpace(track.Artist))
            {
                _logger.LogWarning("Track information incomplete, cannot enrich");
                return;
            }

            // Si les données sont déjà complètes, on ne fait rien
            if (track.Duration > 0 && !string.IsNullOrWhiteSpace(track.TrackUrl))
            {
                _logger.LogDebug("Track already enriched: {Title}", track.Title);
                return;
            }

            const int maxRetries = 3;
            int attempt = 0;
            bool success = false;

            while (attempt < maxRetries && !success)
            {
                attempt++;
                _logger.LogInformation("Enriching track {Title} (attempt {Attempt})", track.Title, attempt);

                try
                {
                    // 1. Recherche exacte avec l'API Deezer
                    var query = HttpUtility.UrlEncode($"artist:\"{track.Artist}\" track:\"{track.Title}\"");
                    var searchUrl = $"{DeezerApiBaseUrl}/search?q={query}&limit=5";

                    using var response = await _httpClient.GetAsync(searchUrl);

                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        _logger.LogWarning("Track not found: {Title}", track.Title);
                        continue;
                    }

                    response.EnsureSuccessStatusCode();

                    var json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;

                    if (!root.TryGetProperty("data", out var data) || data.GetArrayLength() == 0)
                    {
                        _logger.LogWarning("No results found for: {Title}", track.Title);
                        continue;
                    }

                    // 2. Trouver la meilleure correspondance
                    foreach (var item in data.EnumerateArray())
                    {
                        var apiTitle = item.GetProperty("title").GetString() ?? string.Empty;
                        var apiArtist = item.GetProperty("artist").GetProperty("name").GetString() ?? string.Empty;
                        var apiAlbum = item.GetProperty("album").GetProperty("title").GetString() ?? string.Empty;

                        if (IsMatchingTrack(track, apiTitle, apiArtist, apiAlbum))
                        {
                            // 3. Récupérer les données nécessaires
                            track.Duration = item.GetProperty("duration").GetInt32();

                            if (item.TryGetProperty("album", out var albumProp) &&
                                albumProp.TryGetProperty("cover_big", out var coverProp))
                            {
                                track.TrackUrl = coverProp.GetString() ?? string.Empty;
                            }

                            _logger.LogInformation("Successfully enriched track: {Title}", track.Title);
                            success = true;
                            break;
                        }
                    }
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError(ex, "HTTP error while enriching track (attempt {Attempt})", attempt);
                    //if (attempt >= maxRetries) ApplyFallbackValues(track);
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "JSON parsing error (attempt {Attempt})", attempt);
                    //if (attempt >= maxRetries) ApplyFallbackValues(track);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error (attempt {Attempt})", attempt);
                    //if (attempt >= maxRetries) ApplyFallbackValues(track);
                }

                if (!success && attempt < maxRetries)
                {
                    await Task.Delay(1000 * attempt); // Backoff exponentiel
                }
            }

            if (!success)
            {
                //ApplyFallbackValues(track);
                _logger.LogWarning("Using fallback values for track: {Title}", track.Title);
            }
        }

        // Helper method pour vérifier la correspondance
        private bool IsMatchingTrack(TrackInfos track, string apiTitle, string apiArtist, string apiAlbum)
        {
            // Comparaison insensible à la casse et aux caractères spéciaux
            var cleanTrackTitle = CleanString(track.Title);
            var cleanApiTitle = CleanString(apiTitle);

            var cleanTrackArtist = CleanString(track.Artist);
            var cleanApiArtist = CleanString(apiArtist);

            var titleMatch = cleanTrackTitle.Equals(cleanApiTitle, StringComparison.OrdinalIgnoreCase);
            var artistMatch = cleanTrackArtist.Equals(cleanApiArtist, StringComparison.OrdinalIgnoreCase);

            // L'album est optionnel pour la correspondance
            var albumMatch = string.IsNullOrEmpty(track.Album) ||
                            CleanString(track.Album).Equals(CleanString(apiAlbum), StringComparison.OrdinalIgnoreCase);

            return titleMatch && artistMatch && albumMatch;
        }

        // Helper method pour nettoyer les strings
        private string CleanString(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;

            return input.Trim()
                       .ToLowerInvariant()
                       .Replace(" ", "")
                       .Replace("-", "")
                       .Replace("'", "")
                       .Replace("&", "and");
        }

        public async Task EnrichArtistWithDeezerData(ArtistInfos? artist)
        {
            if (artist is null || string.IsNullOrWhiteSpace(artist.Name))
                return;

            var query = HttpUtility.UrlEncode(artist.Name);
            var searchUrl = $"{DeezerApiBaseUrl}/search/artist?q={query}";

            _logger.LogInformation("Searching artist: {Query}", query);

            var searchData = await GetJsonArrayFromUrl(searchUrl);
            if (searchData is null)
            {
                _logger.LogWarning("No search data found for artist: {Query}", query);
                return;
            }

            foreach (var item in searchData.Value.EnumerateArray())
            {
                var artistName = item.GetProperty("name").GetString();
                _logger.LogDebug("Found artist: {ArtistName}", artistName);

                if (string.Equals(artistName, artist.Name, StringComparison.OrdinalIgnoreCase))
                {
                    artist.ArtistUrl = item.GetProperty("picture_big").GetString() ?? string.Empty;
                    _logger.LogInformation("Artist enriched: {Artist}", artist.Name);
                    return;
                }
            }
        }

        private async Task<JsonElement?> GetJsonFromUrl(string url)
        {
            try
            {
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Request failed: {StatusCode} for URL {Url}", response.StatusCode, url);
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("Raw JSON from {Url}:\n{Json}", url, json);

                using var doc = JsonDocument.Parse(json);
                return doc.RootElement.Clone(); // Cloner pour éviter la perte de scope du JsonDocument
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching or parsing JSON from URL: {Url}", url);
                return null;
            }
        }

        private async Task<JsonElement?> GetJsonArrayFromUrl(string url)
        {
            var root = await GetJsonFromUrl(url);

            if (root is null)
            {
                _logger.LogWarning("No JSON returned from URL: {Url}", url);
                return null;
            }

            if (!root.Value.TryGetProperty("data", out var dataArray))
            {
                _logger.LogWarning("Missing 'data' property in JSON from URL: {Url}", url);
                _logger.LogDebug("Full JSON received: {Json}", root.ToString());
                return null;
            }

            return dataArray;
        }
    }
}
