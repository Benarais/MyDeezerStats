using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using MyDeezerStats.Application.Dtos;
using MyDeezerStats.Application.Interfaces;
using MyDeezerStats.Domain.Entities.DeezerInfos;
using MyDeezerStats.Domain.Entities.ListeningInfos;
using MyDeezerStats.Domain.Repositories;

namespace MyDeezerStats.Application.MongoDbServices
{
    public class ListeningService : IListeningService
    {
        private readonly IListeningRepository _repository;
        private readonly IInformationRepository _informationRepository;
        private readonly IDeezerService _deezerService;
        private readonly ILogger<ListeningService> _logger;

        public ListeningService(
            IListeningRepository repository,
            IInformationRepository informationRepository,
            IDeezerService deezerService,
            ILogger<ListeningService> logger)
        {
            _repository = repository;
            _informationRepository = informationRepository;
            _deezerService = deezerService;
            _logger = logger;
        }

        public async Task<List<DeezerAlbumInfos>> GetTopAlbumsAsync(DateTime? from, DateTime? to)
        {
            return await ExecuteWithErrorHandling(async () =>
            {
                var topAlbums = await _repository.GetTopAlbumsWithTracksAsync(from, to, 10);
                var albumTasks = topAlbums.Select(ProcessAlbumAsync);
                return (await Task.WhenAll(albumTasks)).ToList();
            }, "GetTopAlbumsAsync");
        }

        public async Task<List<DeezerArtistInfos>> GetTopArtistsAsync(DateTime? from, DateTime? to)
        {
            return await ExecuteWithErrorHandling(async () =>
            {
                var topArtists = await _repository.GetTopArtistsWithTracksAsync(from, to, 10);
                var artistTasks = topArtists.Select(ProcessArtistAsync);
                return (await Task.WhenAll(artistTasks)).ToList();
            }, "GetTopArtistsAsync");
        }

        public async Task<List<DeezerTrackInfos>> GetTopTracksAsync(DateTime? from, DateTime? to)
        {
            return await ExecuteWithErrorHandling(async () =>
            {
                var topTracks = await _repository.GetTopTracksWithAsync(from, to, 10);
                var trackTasks = topTracks.Select(ProcessTrackAsync);
                return (await Task.WhenAll(trackTasks)).ToList();
            }, "GetTopTracksAsync");
        }

        public async Task<IEnumerable<ListeningDto>> GetLatestListeningsAsync(int limit = 100)
        {
            return await ExecuteWithErrorHandling(async () =>
            {
                var listenings = await _repository.GetLatestListeningsAsync(limit);
                return listenings.Select(x => new ListeningDto
                {
                    Track = x.Track,
                    Artist = x.Artist,
                    Album = x.Album,
                    Date = x.Date
                });
            }, "GetLatestListeningsAsync");
        }

        #region Private Methods

        private async Task<T> ExecuteWithErrorHandling<T>(Func<Task<T>> operation, string operationName)
        {
            using var scope = _logger.BeginScope(operationName);
            try
            {
                _logger.LogInformation("Starting {Operation}", operationName);
                var result = await operation();
                _logger.LogInformation("Completed {Operation}", operationName);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in {Operation}", operationName);
                throw;
            }
        }

        private async Task<DeezerAlbumInfos> ProcessAlbumAsync(AlbumListening album)
        {
            var albumInfos = await GetOrCreateAlbumInfosAsync(album);
            var totalListening = await CalculateTotalListeningAsync(
                album.StreamCountByTrack,
                album.Title,
                album.Artist);

            return new DeezerAlbumInfos
            {
                Title = album.Title,
                Artist = album.Artist,
                Count = album.StreamCount,
                TotalDuration = albumInfos.Duration,
                CoverUrl = albumInfos.AlbumUrl,
                TotalListening = totalListening
            };
        }

        private async Task<DeezerArtistInfos> ProcessArtistAsync(ArtistListening artist)
        {
            var artistInfos = await GetOrCreateArtistInfosAsync(artist);
            var totalListening = await CalculateTotalListeningAsync(
                artist.StreamCountByTrack,
                artist.Name);

            return new DeezerArtistInfos
            {
                Artist = artist.Name,
                Count = artist.StreamCount,
                CoverUrl = artistInfos.ArtistUrl,
                TotalListening = totalListening
            };
        }

        private async Task<DeezerTrackInfos> ProcessTrackAsync(TrackListening track)
        {
            var trackInfos = await GetOrCreateTrackInfosAsync(track);
            return new DeezerTrackInfos
            {
                Track = track.Name,
                Album = track.Album,
                Artist = track.Artist,
                TrackUrl = trackInfos.TrackUrl,
                Count = track.StreamCount,
                Duration = trackInfos.Duration,
                TotalListening = track.StreamCount * trackInfos.Duration
            };
        }

        private async Task<AlbumInfos> GetOrCreateAlbumInfosAsync(AlbumListening album)
        {
            var cacheKey = $"album:{album.Title}:{album.Artist}";
            //if (_albumCache.TryGetValue(cacheKey, out var cached)) return cached;

            var infos = await _informationRepository.GetAlbumInfosAsync(album.Title, album.Artist)
                       ?? new AlbumInfos { Title = album.Title, Artist = album.Artist };

            if (!IsAlbumInfoComplete(infos))
            {
                await _deezerService.EnrichAlbumWithDeezerData(infos);
                await _informationRepository.InsertAlbumInfosAsync(infos);
            }

            //_albumCache.TryAdd(cacheKey, infos);
            return infos;
        }

        private async Task<ArtistInfos> GetOrCreateArtistInfosAsync(ArtistListening artist)
        {
            var cacheKey = $"artist:{artist.Name}";
            //if (_artistCache.TryGetValue(cacheKey, out var cached)) return cached;

            var infos = await _informationRepository.GetArtistInfosAsync(artist.Name)
                       ?? new ArtistInfos { Name = artist.Name };

            if (!IsArtistInfoComplete(infos))
            {
                await _deezerService.EnrichArtistWithDeezerData(infos);
                await _informationRepository.InsertArtistInfosAsync(infos);
            }

            //_artistCache.TryAdd(cacheKey, infos);
            return infos;
        }

        private async Task<TrackInfos> GetOrCreateTrackInfosAsync(TrackListening track)
        {
            //var cacheKey = $"track:{track.Name}:{track.Artist}:{track.Album}";
            //if (_trackCache.TryGetValue(cacheKey, out var cached)) return cached;

            var infos = await _informationRepository.GetTrackInfosAsync(track.Name, track.Artist)
                       ?? new TrackInfos { Title = track.Name, Artist = track.Artist, Album = track.Album };

            if (!IsTrackInfoComplete(infos))
            {
                await _deezerService.EnrichTrackWithDeezerData(infos);
                await _informationRepository.InsertTrackInfosAsync(infos);
            }
            else
            {
                _logger.LogInformation("Track info found in database and is complete for track: {TrackName} by {Artist}", track.Name, track.Artist);
            }

            //_trackCache.TryAdd(cacheKey, infos);
            return infos;
        }

        private async Task<int> CalculateTotalListeningAsync(
            Dictionary<string, int> streamCountByTrack,
            string artist,
            string? album = null)
        {
            var results = new ConcurrentBag<int>();
            var tasks = streamCountByTrack.Select(async track =>
            {
                _logger.LogInformation("Track: {TrackKey}, Streams: {Streams}", track.Key, track.Value);  // Log des tracks
                var duration = await GetTrackDurationAsync(track.Key, artist, album);
                results.Add(track.Value * duration);
            });

            await Task.WhenAll(tasks);
            return results.Sum();
        }

        private async Task<int> GetTrackDurationAsync(string trackTitle, string artist, string? album = null)
        {
            var trackInfos = await GetOrCreateTrackInfosAsync(new TrackListening
            {
                Name = trackTitle,
                Artist = artist,
                Album = album
            });

            _logger.LogInformation("Track: {TrackTitle}, Duration: {Duration}", trackTitle, trackInfos.Duration);  // Ajoutez ce log
            return trackInfos.Duration;
        }

        private bool IsAlbumInfoComplete(AlbumInfos infos) =>
            !string.IsNullOrEmpty(infos.AlbumUrl) && infos.Duration > 0 && infos.TrackNumber > 0;

        private bool IsArtistInfoComplete(ArtistInfos infos) =>
            !string.IsNullOrEmpty(infos.ArtistUrl);

        private bool IsTrackInfoComplete(TrackInfos infos) =>
            infos.Duration > 0 && !string.IsNullOrEmpty(infos.TrackUrl);

        #endregion
    }
}