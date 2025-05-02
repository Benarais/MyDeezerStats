using MyDeezerStats.Domain.Entities;
using MyDeezerStats.Domain.Entities.DeezerInfos;

namespace MyDeezerStats.Application.Interfaces
{
    public interface IDeezerService
    {
        Task EnrichAlbumWithDeezerData(AlbumInfos? albumStatistic);
        Task EnrichArtistWithDeezerData(ArtistInfos artist);
        Task EnrichTrackWithDeezerData(TrackInfos? track);
    }
}
