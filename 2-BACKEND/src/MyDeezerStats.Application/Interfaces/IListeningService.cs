using MyDeezerStats.Application.Dtos;

namespace MyDeezerStats.Application.Interfaces
{
    public interface IListeningService
    {
        Task<List<DeezerAlbumInfos>> GetTopAlbumsAsync(DateTime? from, DateTime? to);
        Task<List<DeezerArtistInfos>> GetTopArtistsAsync(DateTime? from, DateTime? to);

        Task<List<DeezerTrackInfos>> GetTopTracksAsync(DateTime? from, DateTime? to);
        Task<IEnumerable<ListeningDto>> GetLatestListeningsAsync(int limit = 100);
    }
}
