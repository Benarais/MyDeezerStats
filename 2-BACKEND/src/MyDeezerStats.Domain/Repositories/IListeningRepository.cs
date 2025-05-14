

using MongoDB.Bson;
using MyDeezerStats.Domain.Entities;
using MyDeezerStats.Domain.Entities.ListeningInfos;


namespace MyDeezerStats.Domain.Repositories
{
    public interface IListeningRepository
    {
        Task<List<AlbumListening>> GetTopAlbumsWithAsync(DateTime? from = null, DateTime? to = null, int limit = 10);
        Task<AlbumListening?> GetAlbumsWithAsync(string title, string artist, DateTime? from = null, DateTime? to = null);


        Task<List<ArtistListening>> GetTopArtistWithAsync(DateTime? from = null, DateTime? to = null, int limit = 10);
        Task<ArtistListening?> GetArtistWithAsync(string artist, DateTime? from = null, DateTime? to = null);

        Task<List<TrackListening>> GetTopTrackWithAsync(DateTime? from, DateTime? to, int limit = 10);


        Task<List<ListeningEntry>> GetLatestListeningsAsync(int limit);

        Task InsertListeningsAsync(List<ListeningEntry> listenings);
        
    }
}
