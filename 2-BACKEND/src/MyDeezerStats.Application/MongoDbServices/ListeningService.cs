
using Microsoft.Extensions.Logging;
using MyDeezerStats.Application.Dtos.LastStream;
using MyDeezerStats.Application.Dtos.TopStream;
using MyDeezerStats.Application.Interfaces;
using MyDeezerStats.Domain.Entities;
using MyDeezerStats.Domain.Entities.ListeningInfos;
using MyDeezerStats.Domain.Exceptions;
using MyDeezerStats.Domain.Repositories;

namespace MyDeezerStats.Application.MongoDbServices
{
    public class ListeningService : IListeningService
    {
        private readonly IListeningRepository _repository;
        private readonly IDeezerService _deezerService;
        private readonly ILastFmService _lastFmService;
        private readonly ILogger<ListeningService> _logger;

        public ListeningService(
            IListeningRepository repository,
            IDeezerService deezerService,
            ILastFmService lastFmService,
            ILogger<ListeningService> logger)
        {
            _repository = repository;
            _deezerService = deezerService;
            _lastFmService = lastFmService;
            _logger = logger;
        }

        public async Task<List<ShortAlbumInfos>> GetTopAlbumsAsync(DateTime? from, DateTime? to, int nb = 5)
        {
            var topAlbums = await _repository.GetTopAlbumsWithAsync(nb, from, to);

            var albumTasks = topAlbums.Select(async album =>
            {
                ShortAlbumInfos enrichedAlbum = await _deezerService.EnrichShortAlbumWithDeezerData(album);
                enrichedAlbum.Count = album.StreamCount;
                return enrichedAlbum;
            });

            var result = await Task.WhenAll(albumTasks);
            return result.ToList();
        }

        public async Task<FullAlbumInfos> GetAlbumAsync(string fullId)
        {
            if (string.IsNullOrWhiteSpace(fullId))
            {
                throw new ArgumentException("Album identifier cannot be empty", nameof(fullId));
            }

            var pipeIndex = fullId.IndexOf('|');
            if (pipeIndex < 0 || pipeIndex == fullId.Length - 1)
            {
                throw new ArgumentException("Invalid album identifier format", nameof(fullId));
            }

            var title = Uri.UnescapeDataString(fullId.Substring(0, pipeIndex));
            var artist = Uri.UnescapeDataString(fullId.Substring(pipeIndex + 1));
            var album = await _repository.GetAlbumsWithAsync(title, artist, null, null)
                ?? throw new NotFoundException($"Album {title} by {artist} not found");

            var enrichedAlbum = await _deezerService.EnrichFullAlbumWithDeezerData(album);

            return enrichedAlbum;
        }

        public async Task<List<ShortArtistInfos>> GetTopArtistsAsync(DateTime? from, DateTime? to, int nb = 5)
        {
            var topArtist = await _repository.GetTopArtistWithAsync(nb, from, to);
            _logger.LogInformation($"Nombre d'artistes récupérés : {topArtist.Count}");
            var albumTasks = topArtist.Select(async artist =>
            {
                ShortArtistInfos enrichedArtist = await _deezerService.EnrichShortArtistWithDeezerData(artist);
                enrichedArtist.Count = artist.StreamCount;
                return enrichedArtist;
            });

            var result = await Task.WhenAll(albumTasks);
            return result.ToList();
        }

        public async Task<FullArtistInfos> GetArtistAsync(string fullId)
        {
            if (string.IsNullOrWhiteSpace(fullId))
            {
                throw new ArgumentException("Invalid album identifier format", nameof(fullId));
            }

            ArtistListening artist = await _repository.GetArtistWithAsync(fullId, null, null)
                ?? throw new NotFoundException($"Artist {fullId} not found");

            FullArtistInfos enrichedArtist = await _deezerService.EnrichFullArtistWithDeezerData(artist);
            return enrichedArtist;
        }
     
        public async Task<List<ApiTrackInfos>> GetTopTracksAsync(DateTime? from, DateTime? to, int nb = 5)
        {
            var topTrack = await _repository.GetTopTrackWithAsync(nb, from, to);

            var trackTasks = topTrack.Select(async track =>
            {
                ApiTrackInfos enrichedTrack = await _deezerService.EnrichTrackWithDeezerData(track);
                return enrichedTrack;
            });

            var result = await Task.WhenAll(trackTasks);
            return result.ToList();
        }

        public async Task<IEnumerable<ListeningDto>> GetLatestListeningsAsync(int limit = 100)
        {
            var listenings = await _repository.GetLatestListeningsAsync(limit);
            var result = listenings.Select(x => new ListeningDto
            {
                Track = x.Track,
                Artist = x.Artist,
                Album = x.Album,
                Date = x.Date
            }).OrderByDescending(x=>x.Date);
            var lastStreamDate = result.First().Date;
            var lastStreams = _lastFmService.GetListeningHistorySince(lastStreamDate).Result;
            var lastStreamToUpsert = new List<ListeningEntry>();

            foreach(var listeningEntry in lastStreams) 
            { 
                lastStreamToUpsert.Add(new ListeningEntry
                {
                    Album = listeningEntry.Album,
                    Date = listeningEntry.Date, 
                    Track = listeningEntry.Track,   
                    Artist = listeningEntry.Artist
                }); 
            }
            await _repository.InsertListeningsAsync(lastStreamToUpsert);
            listenings = await _repository.GetLatestListeningsAsync(limit);
            result = listenings.Select(x => new ListeningDto
            {
                Track = x.Track,
                Artist = x.Artist,
                Album = x.Album,
                Date = x.Date
            }).OrderByDescending(x => x.Date);
            return result;
        }
    }
}