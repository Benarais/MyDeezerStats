using MyDeezerStats.Application.Dtos.LastStream;
using MyDeezerStats.Application.Dtos.TopStream;

namespace MyDeezerStats.Application.Interfaces
{
    public interface IListeningService
    {
        Task<List<ShortAlbumInfos>> GetTopAlbumsAsync(DateTime? from, DateTime? to);
        Task<FullAlbumInfos> GetAlbumAsync(string identifier);
        Task<List<ShortArtistInfos>> GetTopArtistsAsync(DateTime? from, DateTime? to);
        Task<FullArtistInfos> GetArtistAsync(string identifier);
        Task<List<ApiTrackInfos>> GetTopTracksAsync(DateTime? from, DateTime? to);
        Task<IEnumerable<ListeningDto>> GetLatestListeningsAsync(int limit = 100);

    }
}
