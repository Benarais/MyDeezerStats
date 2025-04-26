using Microsoft.Extensions.Logging;
using MyDeezerStats.Application.Dtos;
using MyDeezerStats.Application.Interfaces;
using MyDeezerStats.Domain.Repositories;

namespace MyDeezerStats.Application.MongoDbServices
{
    public class ListeningService : IListeningService
    {
        private readonly IListeningRepository _repository;
        private readonly ILogger<ListeningService> _logger;

        public ListeningService(IListeningRepository repository, ILogger<ListeningService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<IEnumerable<AggregatedListeningDto>> GetTopAlbumsAsync(DateTime? from, DateTime? to)
        {
            _logger.LogInformation("Calcul des top albums entre {From} et {To}", from, to);
            var results = await GetTopAsync("Album", from, to, 20);
            return results.Select(x => new AggregatedListeningDto { Key = x.Key, Artist = x.Artist, Count = x.Count });
        }

        public async Task<IEnumerable<AggregatedListeningDto>> GetTopArtistsAsync(DateTime? from, DateTime? to)
        {
            var results = await GetTopAsync("Artist", from, to, 20);
            return results.Select(x => new AggregatedListeningDto { Key = x.Key, Count = x.Count });
        }

        public async Task<IEnumerable<AggregatedListeningDto>> GetTopTracksAsync(DateTime? from, DateTime? to)
        {
            var results = await GetTopAsync("Track", from, to, 20);
            return results.Select(x => new AggregatedListeningDto { Key = x.Key, Album = x.Album, Artist = x.Artist, Count = x.Count });
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

        public async Task<List<AggregatedListeningDto>> GetTopAsync(string groupByField, DateTime? from, DateTime? to, int limit)
        {
            var rawResult = await _repository.AggregateRawAsync(groupByField, from, to, limit);

            if (rawResult == null || !rawResult.Any())
            {
                _logger.LogInformation("L'agrégation n'a retourné aucun résultat.");
            }
            else
            {
                _logger.LogInformation($"L'agrégation a retourné {rawResult.Count()} résultats.");
                foreach (var doc in rawResult)
                {
                    // Afficher chaque document sous forme de chaîne JSON pour voir son contenu
                    Console.WriteLine(doc.ToString());
                }
            }

            return rawResult!.Select(doc => new AggregatedListeningDto
            {
                Key = doc["_id"]?.BsonType == MongoDB.Bson.BsonType.Null ? "(inconnu)" : doc["_id"]?.ToString() ?? "(inconnu)",
                Count = doc["count"].AsInt32,
                Artist = doc.Contains("artist") ? doc["artist"].AsString : "",
                Album = doc.Contains("album") ? doc["album"].AsString : ""
            }).ToList();
        }
    }
}
