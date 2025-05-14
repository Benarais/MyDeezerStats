using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MyDeezerStats.Application.Dtos
{
    public class ShortArtistInfos
    {
        public string Artist { get; set; } = string.Empty;
        public string CoverUrl { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class FullArtistInfos
    {
        public string Artist { get; set; } = string.Empty;
        public string CoverUrl { get; set; } = string.Empty;
        public int PlayCount { get; set; }
        public int TotalListening { get; set; }
        public List<ApiTrackInfos> TrackInfos { get; set; } = [];
        public int NbFans { get; set; }
    }
}
