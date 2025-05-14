using MyDeezerStats.Application.Dtos;
using MyDeezerStats.Domain.Entities;
using MyDeezerStats.Domain.Entities.DeezerInfos;
using MyDeezerStats.Domain.Entities.ListeningInfos;

namespace MyDeezerStats.Application.Interfaces
{
    public interface IDeezerService
    {
        Task<FullAlbumInfos> EnrichFullAlbumWithDeezerData(AlbumListening album);
        Task<ShortAlbumInfos> EnrichShortAlbumWithDeezerData(AlbumListening album);
        Task<FullArtistInfos> EnrichFullArtistWithDeezerData(ArtistListening artist);
        Task<ShortArtistInfos> EnrichShortArtistWithDeezerData(ArtistListening artist);
        Task<ApiTrackInfos> EnrichTrackWithDeezerData(TrackListening track);
    }
}
