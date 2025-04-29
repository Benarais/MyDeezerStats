using MyDeezerStats.Application.Dtos;
using MyDeezerStats.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDeezerStats.Application.Interfaces
{
    public interface IDeezerService
    {
        Task EnrichAlbumWithDeezerData(AlbumStatistic albumStatistic);
        Task EnrichArtistWithDeezerData(ArtistStatistic artist);
        Task EnrichTrackWithDeezerData(TrackStatistic track);
    }
}
