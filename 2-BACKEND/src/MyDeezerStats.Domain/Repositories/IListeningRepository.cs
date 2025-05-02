

using MongoDB.Bson;
using MyDeezerStats.Domain.Entities;
using MyDeezerStats.Domain.Entities.ListeningInfos;


namespace MyDeezerStats.Domain.Repositories
{
    public interface IListeningRepository
    {
        Task<List<AlbumListening>> GetTopAlbumsWithTracksAsync(DateTime? from = null, DateTime? to = null, int limit = 10);
        Task<List<ArtistListening>> GetTopArtistsWithTracksAsync(DateTime? from = null, DateTime? to = null, int limit = 10);
        Task<List<TrackListening>> GetTopTracksWithAsync(DateTime? from, DateTime? to, int limit = 10);
        Task<List<ListeningEntry>> GetLatestListeningsAsync(int limit);
        Task InsertListeningsAsync(List<ListeningEntry> listenings);
    }
}
