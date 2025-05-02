using MyDeezerStats.Domain.Entities;
using MyDeezerStats.Domain.Entities.DeezerInfos;

namespace MyDeezerStats.Domain.Repositories
{
    public interface IInformationRepository
    {
        Task<AlbumInfos?> GetAlbumInfosAsync(string album, string artist);
        Task<ArtistInfos?> GetArtistInfosAsync(string artist);
        Task<TrackInfos?> GetTrackInfosAsync(string track, string artist);
        Task InsertAlbumInfosAsync(AlbumInfos albumInfos);
        Task InsertArtistInfosAsync(ArtistInfos artistInfos);
        Task InsertTrackInfosAsync(TrackInfos trackInfo);
    }
}
