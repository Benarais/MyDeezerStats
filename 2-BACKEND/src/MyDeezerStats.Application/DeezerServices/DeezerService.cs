using MyDeezerStats.Application.Dtos;
using MyDeezerStats.Application.Interfaces;
using MyDeezerStats.Domain.Entities;
using System.Net.Http.Json;

namespace MyDeezerStats.Application.DeezerServices
{
    public class DeezerService : IDeezerService
    {
        private readonly HttpClient _httpClient;
        private const string DeezerApiBaseUrl = "https://api.deezer.com";

        public DeezerService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task EnrichAlbumWithDeezerData(AlbumStatistic albumStatistic)
        {
            if (albumStatistic == null || albumStatistic.Listening == null || !albumStatistic.Listening.Any())
                return;

            try
            {
                // Recherche de l'album sur Deezer
                var searchUrl = $"{DeezerApiBaseUrl}/search/album?q={Uri.EscapeDataString($"{albumStatistic.Album} {albumStatistic.Artist}")}&limit=1";
                var searchResponse = await _httpClient.GetFromJsonAsync<DeezerSearchResponse<DeezerAlbum>>(searchUrl);
                var deezerAlbum = searchResponse?.data?.FirstOrDefault();
                if (deezerAlbum == null) return;

                // Mise à jour de l'URL de la cover
                albumStatistic.AlbumUrl = deezerAlbum.cover_medium;

                // Récupération des détails de l'album (avec les pistes et leurs durées)
                var albumDetailsUrl = $"{DeezerApiBaseUrl}/album/{deezerAlbum.id}";
                var albumDetailsResponse = await _httpClient.GetFromJsonAsync<DeezerAlbumResponse>(albumDetailsUrl);

                if (albumDetailsResponse?.TracksData?.data != null)
                {
                    // Durée totale de l’album
                    albumStatistic.AlbumDuration = albumDetailsResponse.duration;

                    // Calcul du temps d'écoute total (somme durée piste × nb écoutes)
                    albumStatistic.AlbumTotalListening = 0;

                    foreach (var track in albumStatistic.Listening)
                    {
                        var deezerTrack = albumDetailsResponse.TracksData.data
                            .FirstOrDefault(t => t.Title.Equals(track.Name, StringComparison.OrdinalIgnoreCase));

                        if (deezerTrack != null)
                        {
                            track.TotalDuration = deezerTrack.Duration * track.Count;
                            albumStatistic.AlbumTotalListening += track.TotalDuration;
                        }
                    }
                }

                // Calcul du nombre total d'écoutes
                albumStatistic.AlbumNumberListening = albumStatistic.Listening.Sum(t => t.Count);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur Deezer API: {ex.Message}");
                CalculateFallbackValues(albumStatistic);
            }
        }

        private void CalculateFallbackValues(AlbumStatistic albumStatistic)
        {
            // Calcul approximatif de la durée native si indisponible
            if (albumStatistic.AlbumDuration <= 0 && albumStatistic.Listening.Any(t => t.TotalDuration > 0))
            {
                // Moyenne des durées connues × nombre de pistes
                var avgDuration = albumStatistic.Listening
                    .Where(t => t.TotalDuration > 0)
                    .Average(t => t.TotalDuration / t.Count);

                albumStatistic.AlbumDuration = (int)(avgDuration * albumStatistic.Listening.Count);
            }

            // Temps d'écoute total si indisponible
            if (albumStatistic.AlbumTotalListening <= 0)
            {
                albumStatistic.AlbumTotalListening = albumStatistic.Listening.Sum(t => t.TotalDuration);
            }

            // Nombre total d'écoutes
            albumStatistic.AlbumNumberListening = albumStatistic.Listening.Sum(t => t.Count);
        }

        public async Task EnrichArtistWithDeezerData(ArtistStatistic artistStatistic)
        {
            // Appel à l'API Deezer pour obtenir les détails de l'artiste
            var artistInfo = await GetArtistInfoFromDeezer(artistStatistic.Artist);

            if (artistInfo != null)
            {
                // Enrichissement des données de l'artiste
                artistStatistic.ArtistUrl = artistInfo.CoverUrl;

                // durée de chaque track écouté de l'artiste
                foreach (var listening in artistStatistic.Listening)
                {
                    var trackInfo = await GetTrackInfoFromDeezer(listening.Name);
                    if (trackInfo != null)
                    {
                        listening.TotalDuration = trackInfo.Duration;
                    }
                }

                // Calcul de la durée totale d'écoute et du nombre total d'écoutes
                int totalDuration = 0;
                int totalListeningCount = 0;

                // Parcours de chaque ListeningInfo pour calculer les durées et écoutes
                foreach (var listening in artistStatistic.Listening)
                {
                    totalDuration += listening.TotalDuration;
                    totalListeningCount += listening.Count;
                }

                artistStatistic.ArtistListeningDuration = totalDuration;
                artistStatistic.ArtistListeningCount = totalListeningCount;
            }
        }

        public  async Task EnrichTrackWithDeezerData(TrackStatistic track)
        {
            // Appel à l'API Deezer pour obtenir les détails du morceau
            var trackInfos = await GetTrackInfoFromDeezer(track.Track, track.Album, track.Artist); 
            if (trackInfos != null)
            {
                // Enrichissement des données de l'artiste
                track.TrackDuration = trackInfos.Duration;
                track.TrackUrl = trackInfos.TrackUrl;

                track.TrackTotalListening = track.TrackNumberListening * track.TrackDuration;
            }
        }

        private async Task<DeezerArtistInfos?> GetArtistInfoFromDeezer(string artistName)
        {
            // Effectuer la recherche de l'artiste
            var url = $"https://api.deezer.com/search/artist?q={Uri.EscapeDataString(artistName)}";
            var searchResponse = await _httpClient.GetFromJsonAsync<DeezerSearchResponse<DeezerArtist>>(url);
            var deezerArtist = searchResponse?.data?.FirstOrDefault();

            // Si on trouve l'artiste, effectuer une requête détaillée pour obtenir l'image
            if (deezerArtist == null)
                return null;

            // Appeler l'API Deezer pour obtenir les détails de l'artiste (y compris l'image)
            var artistDetailsUrl = $"https://api.deezer.com/artist/{deezerArtist.Id}";
            var artistDetails = await _httpClient.GetFromJsonAsync<DeezerArtist>(artistDetailsUrl);

            // Retourner les informations de l'artiste, y compris l'image
            return new DeezerArtistInfos
            {
                Artist = deezerArtist.Name,
                CoverUrl = artistDetails?.Picture ?? string.Empty 
            };
        }

        private async Task<DeezerTrackInfos?> GetTrackInfoFromDeezer(string trackName, string? albumName = null, string? artistName = null)
        {
            // Crée une base de la query avec le titre de la piste
            var query = $"track:\"{Uri.EscapeDataString(trackName)}\"";

            // Si l'album est fourni, l'ajouter à la query
            if (!string.IsNullOrEmpty(albumName))
            {
                query += $" album:\"{Uri.EscapeDataString(albumName)}\"";
            }

            // Si l'artiste est fourni, l'ajouter à la query
            if (!string.IsNullOrEmpty(artistName))
            {
                query += $" artist:\"{Uri.EscapeDataString(artistName)}\"";
            }

            // Construire l'URL complet
            var url = $"https://api.deezer.com/search?q={query}";

            // Effectuer la requête à Deezer
            var searchResponse = await _httpClient.GetFromJsonAsync<DeezerSearchResponse<DeezerTrack>>(url);
            var deezerTrack = searchResponse?.data?.FirstOrDefault();

            if (deezerTrack == null)
                return null;

            return new DeezerTrackInfos
            {
                Track = deezerTrack.Title,
                Duration = deezerTrack.Duration,
                TrackUrl = deezerTrack.Album?.cover_medium ?? string.Empty,
            };
        }

    }
}
