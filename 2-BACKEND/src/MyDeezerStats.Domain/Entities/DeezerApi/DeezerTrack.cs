using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDeezerStats.Domain.Entities.DeezerApi
{
    public class DeezerTrack
    {
        public string Title { get; set; } = string.Empty;
        public int Duration { get; set; }
        public string Preview { get; set; } = string.Empty;
        public string CoverUrl { get; set; } = string.Empty;
        public DeezerArtist Artist { get; set; } = new();
        public DeezerAlbum Album { get; set; } = new();
    }
}
