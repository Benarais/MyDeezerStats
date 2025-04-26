using MyDeezerStats.Application.Dtos;

namespace MyDeezerStats.Application.Interfaces
{
    public interface IListeningService
    {
        Task<IEnumerable<AggregatedListeningDto>> GetTopAlbumsAsync(DateTime? from, DateTime? to);
        Task<IEnumerable<AggregatedListeningDto>> GetTopArtistsAsync(DateTime? from, DateTime? to);
        Task<IEnumerable<AggregatedListeningDto>> GetTopTracksAsync(DateTime? from, DateTime? to);
        Task<IEnumerable<ListeningDto>> GetLatestListeningsAsync(int limit = 100);
    }
}
