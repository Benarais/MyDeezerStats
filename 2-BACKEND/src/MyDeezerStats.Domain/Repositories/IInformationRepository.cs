using MyDeezerStats.Domain.Entities;
using MyDeezerStats.Domain.Entities.DeezerInfos;

namespace MyDeezerStats.Domain.Repositories
{
    public interface IInformationRepository
    {
        Task<ArtistInfos?> GetArtistInfosAsync(string artist);
        Task<TrackInfos?> GetTrackInfosAsync(string track, string artist);
        Task InsertArtistInfosAsync(ArtistInfos artistInfos);
        Task InsertTrackInfosAsync(TrackInfos trackInfo);
    }
}
