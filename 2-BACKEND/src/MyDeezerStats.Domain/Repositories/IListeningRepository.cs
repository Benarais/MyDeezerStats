

using MongoDB.Bson;
using MyDeezerStats.Domain.Entities;


namespace MyDeezerStats.Domain.Repositories
{
    public interface IListeningRepository
    {
        Task<List<BsonDocument>> AggregateRawAsync(string groupByField, DateTime? from, DateTime? to, int limit);
        Task<List<ListeningEntry>> GetLatestListeningsAsync(int limit);
        Task InsertListeningsAsync(List<ListeningEntry> listenings);
    }
}
