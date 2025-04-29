using Microsoft.Extensions.Logging;
using MyDeezerStats.Application.Dtos;
using MyDeezerStats.Application.Interfaces;
using MyDeezerStats.Domain.Repositories;

namespace MyDeezerStats.Application.MongoDbServices
{
    public class ListeningService : IListeningService
    {
        private readonly IListeningRepository _repository;
        private readonly IDeezerService _deezerService;
        private readonly ILogger<ListeningService> _logger;

        public ListeningService(IListeningRepository repository, ILogger<ListeningService> logger, IDeezerService deezerService)
        {
            _repository = repository;
            _logger = logger;
            _deezerService = deezerService;
        }

        public async Task<List<DeezerAlbumInfos>> GetTopAlbumsAsync(DateTime? from, DateTime? to)
        {
            _logger.LogInformation("Début de récupération des top albums. Période : {From} à {To}",
                from?.ToString("yyyy-MM-dd") ?? "Début",
                to?.ToString("yyyy-MM-dd") ?? "Maintenant");

            try
            {
                // 1. Récupération depuis la base de données
                var results = await _repository.GetTopAlbumsWithTracksAsync(from, to, 10);
                _logger.LogInformation("Nombre d'albums récupérés depuis MongoDB : {Count}", results.Count);

                var apiResult = new List<DeezerAlbumInfos>();

                // 2. Traitement de chaque album
                foreach (var album in results)
                {
                    _logger.LogDebug("Traitement de l'album : {Album} - {Artist}", album.Album, album.Artist);
                    try
                    {
                        // 3. Enrichissement via Deezer
                        _logger.LogDebug("Appel à EnrichAlbumWithDeezerData");
                        await _deezerService.EnrichAlbumWithDeezerData(album);

                        _logger.LogDebug("Données enrichies - CoverUrl: {Url}, Durée: {Duration}s",
                            album.AlbumUrl, album.AlbumDuration);

                        // 4. Construction du résultat
                        var deezerAlbum = new DeezerAlbumInfos
                        {
                            Title = album.Album,
                            Artist = album.Artist,
                            CoverUrl = album.AlbumUrl,
                            Count = album.AlbumNumberListening,
                            TotalDuration = album.AlbumDuration,
                            TotalListening = album.AlbumTotalListening
                        };

                        apiResult.Add(deezerAlbum);
                        _logger.LogTrace("Album transformé : {@Album}", deezerAlbum);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Erreur lors du traitement de l'album {Album}", album.Album);
                        apiResult.Add(new DeezerAlbumInfos
                        {
                            Title = album.Album,
                            Artist = album.Artist,
                            Count = album.AlbumNumberListening
                        });
                    }
                }
                _logger.LogInformation("Résultat final contenant {Count} albums", apiResult.Count);
                return apiResult;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Échec critique dans GetTopAlbumsAsync");
                throw;
            }
        }



        public async Task<List<DeezerArtistInfos>> GetTopArtistsAsync(DateTime? from, DateTime? to)
        {
            _logger.LogInformation("Début de récupération des top artistes. Période : {From} à {To}",
                from?.ToString("yyyy-MM-dd") ?? "Début",
                to?.ToString("yyyy-MM-dd") ?? "Maintenant");
            try
            {
                // 1. Récupération depuis la base de données
                var results = await _repository.GetTopArtistsWithTracksAsync(from, to, 10);
                _logger.LogInformation("Nombre d'artistes récupérés depuis MongoDB : {Count}", results.Count);

                var apiResult = new List<DeezerArtistInfos>();

                // 2. Traitement de chaque artiste
                foreach (var artist in results)
                {
                    _logger.LogDebug("Traitement de l'artiste: {Artist}", artist.Artist);
                    try
                    {
                        // 3. Enrichissement via Deezer
                        _logger.LogDebug("Appel à EnrichAlbumWithDeezerData");
                        await _deezerService.EnrichArtistWithDeezerData(artist);
                        // 4. Construction du résultat
                        var deezerArtist = new DeezerArtistInfos
                        {
                            Artist = artist.Artist,
                            CoverUrl = artist.ArtistUrl,
                            Count = artist.ArtistListeningCount,
                            TotalListening = artist.ArtistListeningDuration
                        };

                        apiResult.Add(deezerArtist);
                        _logger.LogTrace("Artiste transformé : {@Artist}", deezerArtist);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Erreur lors du traitement de l'artiste {Artiste}", artist.Artist);
                        apiResult.Add(new DeezerArtistInfos
                        {
                            Artist = artist.Artist,
                            Count = artist.ArtistListeningCount
                        });
                    }
                }
                _logger.LogInformation("Résultat final contenant {Count} artistes", apiResult.Count);
                return apiResult;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Échec critique dans GetTopArtistsAsync");
                throw;
            }
        }

         public async Task<List<DeezerTrackInfos>> GetTopTracksAsync(DateTime? from, DateTime? to)
         {
            _logger.LogInformation("Début de récupération des top tracks. Période : {From} à {To}",
               from?.ToString("yyyy-MM-dd") ?? "Début",
               to?.ToString("yyyy-MM-dd") ?? "Maintenant");
            try
            {
                // Récupération depuis la base de données
                var results = await _repository.GetTopTracksWithAsync(from, to, 10);
                _logger.LogInformation("Nombre de tracks récupérés depuis MongoDB : {Count}", results.Count);

                var apiResult = new List<DeezerTrackInfos>();

                // Traitement de chaque track
                foreach (var track in results)
                {
                    _logger.LogDebug("Traitement de la track: {Track}", track.Track);
                    try
                    {
                        // Enrichissement via Deezer
                        _logger.LogDebug("Appel à EnrichTrackWithDeezerData");
                        await _deezerService.EnrichTrackWithDeezerData(track);
                        // Construction du résultat
                        var deezerTrack = new DeezerTrackInfos
                        {
                            Track = track.Track,
                            Album = track.Album,
                            Artist = track.Artist,
                            TrackUrl = track.TrackUrl,  
                            Count = track.TrackNumberListening,
                            TotalListening = track.TrackTotalListening,
                            Duration = track.TrackDuration                        };

                        apiResult.Add(deezerTrack);
                        _logger.LogTrace("Album transformé : {@Track}", deezerTrack);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Erreur lors du traitement de la track {Track}", track.Track);
                        apiResult.Add(new DeezerTrackInfos
                        {
                            Track = track.Track,
                            Artist = track.Artist,
                            Album= track.Album,
                            Count = track.TrackNumberListening
                        });
                    }
                }
                _logger.LogInformation("Résultat final contenant {Count} tracks", apiResult.Count);
                return apiResult;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Échec critique dans GetTopTracksAsync");
                throw;
            }
         }

        public async Task<IEnumerable<ListeningDto>> GetLatestListeningsAsync(int limit = 100)
        {
            var listenings = await _repository.GetLatestListeningsAsync(limit);
            return listenings.Select(x => new ListeningDto
            {
                Track = x.Track,
                Artist = x.Artist,
                Album = x.Album,
                Date = x.Date
            });
        }

    }
}
