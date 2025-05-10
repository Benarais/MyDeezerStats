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
            var topAlbums = await _repository.GetTopAlbumsWithAsync(from, to, 10);

            var albumTasks = topAlbums.Select(async album =>
            {
                var currentAlbum = new DeezerAlbumInfos
                {
                    Title = album.Title,
                    Artist = album.Artist,
                    Count = album.StreamCount
                };

                var albumInfoTask = _informationRepository.GetAlbumInfosAsync(album.Artist, album.Title);
                var trackInfoTasks = album.StreamCountByTrack.Select(trackNameCount =>
                    _informationRepository.GetTrackInfosAsync(trackNameCount.Key, album.Artist)).ToList();

                var albumInfo = await albumInfoTask ?? new AlbumInfos { Title = album.Title, Artist = album.Artist };

                if (!IsAlbumInfoComplete(albumInfo))
                {
                    await _deezerService.EnrichAlbumWithDeezerData(albumInfo);
                    await _informationRepository.InsertAlbumInfosAsync(albumInfo);
                }

                currentAlbum.TotalDuration = albumInfo.Duration;
                currentAlbum.CoverUrl = albumInfo.AlbumUrl;

                var trackInfos = await Task.WhenAll(trackInfoTasks);

                var enrichmentTasks = trackInfos.Select(async (trackInfo, index) =>
                {
                    var trackNameCount = album.StreamCountByTrack.ElementAt(index);
                    trackInfo ??= new TrackInfos { Title = trackNameCount.Key, Album = album.Title, Artist = album.Artist };

                    if (!IsTrackInfoComplete(trackInfo))
                    {
                        await _deezerService.EnrichTrackWithDeezerData(trackInfo);
                        await _informationRepository.InsertTrackInfosAsync(trackInfo);
                    }

                    return trackNameCount.Value * trackInfo.Duration;
                });

                var listeningDurations = await Task.WhenAll(enrichmentTasks);
                currentAlbum.TotalListening = listeningDurations.Sum();

                return currentAlbum;
            });

            var result = await Task.WhenAll(albumTasks);
            return result.ToList();
        }

        public async Task<List<DeezerArtistInfos>> GetTopArtistsAsync(DateTime? from, DateTime? to)
        {
            var topArtist = await _repository.GetTopArtistsWithAsync(from, to, 10);
            var artistTasks = topArtist.Select(async artist =>
            {
                var currentArtist = new DeezerArtistInfos
                {
                    Artist = artist.Name,
                    Count = artist.StreamCount
                };

                var artistInfoTask = _informationRepository.GetArtistInfosAsync(artist.Name);
                var trackInfoTasks = artist.StreamCountByTrack.Select(trackNameCount =>
                    _informationRepository.GetTrackInfosAsync(trackNameCount.Key, artist.Name)).ToList();

                var artistInfo = await artistInfoTask ?? new ArtistInfos { Name = artist.Name  };

                if (!IsArtistInfoComplete(artistInfo))
                {
                    await _deezerService.EnrichArtistWithDeezerData(artistInfo);
                    await _informationRepository.InsertArtistInfosAsync(artistInfo);
                }

                currentArtist.CoverUrl = artistInfo.ArtistUrl;
                var trackInfos = await Task.WhenAll(trackInfoTasks);

                var enrichmentTasks = trackInfos.Select(async (trackInfo, index) =>
                {
                    var trackNameCount = artist.StreamCountByTrack.ElementAt(index);
                    trackInfo ??= new TrackInfos { Title = trackNameCount.Key, Artist = artist.Name };

                    if (!IsTrackInfoComplete(trackInfo))
                    {
                        await _deezerService.EnrichTrackWithDeezerData(trackInfo);
                        await _informationRepository.InsertTrackInfosAsync(trackInfo);
                    }

                    return trackNameCount.Value * trackInfo.Duration;
                });

                var listeningDurations = await Task.WhenAll(enrichmentTasks);
                currentArtist.TotalListening = listeningDurations.Sum();

                return currentArtist;
            });

            var result = await Task.WhenAll(artistTasks);
            return result.ToList();
        }
       
        public async Task<List<DeezerTrackInfos>> GetTopTracksAsync(DateTime? from, DateTime? to)
        {
            var topTracks = await _repository.GetTopTracksWithAsync(from, to, 10);
            var trackTasks = topTracks.Select(async track =>
            {
                var currentTrack = new DeezerTrackInfos
                {
                    Track = track.Name,
                    Album = track.Album,
                    Artist = track.Artist,
                    Count = track.StreamCount,
                };

                var trackInfoTask = _informationRepository.GetTrackInfosAsync(track.Name, track.Artist);
                var trackInfo = await trackInfoTask ?? new TrackInfos {Title = track.Name, Album = track.Album, Artist = track.Artist};

                if (!IsTrackInfoComplete(trackInfo))
                {
                    await _deezerService.EnrichTrackWithDeezerData(trackInfo);
                    await _informationRepository.InsertTrackInfosAsync(trackInfo);
                }

                currentTrack.TrackUrl = trackInfo.TrackUrl;
                currentTrack.Duration = trackInfo.Duration;
                currentTrack.TotalListening = track.StreamCount * trackInfo.Duration;
                return currentTrack;
            });

            var result = await Task.WhenAll(trackTasks);
            return result.ToList();
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
              
        private bool IsAlbumInfoComplete(AlbumInfos infos) =>
            !string.IsNullOrEmpty(infos.AlbumUrl) && infos.Duration > 0;

        private bool IsArtistInfoComplete(ArtistInfos infos) =>
            !string.IsNullOrEmpty(infos.ArtistUrl);

        private bool IsTrackInfoComplete(TrackInfos infos) =>
            infos.Duration > 0 && !string.IsNullOrEmpty(infos.TrackUrl);

        

        #endregion
    }
}