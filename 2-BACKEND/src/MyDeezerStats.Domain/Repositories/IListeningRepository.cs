

using MongoDB.Bson;
using MyDeezerStats.Domain.Entities;


namespace MyDeezerStats.Domain.Repositories
{
    public interface IListeningRepository
    {
        Task<List<AlbumStatistic>> GetTopAlbumsWithTracksAsync(DateTime? from = null, DateTime? to = null, int limit = 10);
        Task<List<ArtistStatistic>> GetTopArtistsWithTracksAsync(DateTime? from = null, DateTime? to = null, int limit = 10);
        Task<List<TrackStatistic>> GetTopTracksWithAsync(DateTime? from, DateTime? to, int limit = 10);
        Task<List<ListeningEntry>> GetLatestListeningsAsync(int limit);
        Task InsertListeningsAsync(List<ListeningEntry> listenings);
    }
}
