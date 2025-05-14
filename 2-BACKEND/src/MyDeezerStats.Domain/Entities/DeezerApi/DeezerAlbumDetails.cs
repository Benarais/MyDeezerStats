using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MyDeezerStats.Domain.Entities.DeezerApi
{
    public class DeezerAlbumDetails
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("release_date")]
        public string ReleaseDate { get; set; } = string.Empty;

        [JsonPropertyName("duration")]
        public int Duration { get; set; }

        [JsonPropertyName("cover_xl")]
        public string CoverXl { get; set; } = string.Empty;

        [JsonPropertyName("tracks")]
        public DeezerTrackList Tracks { get; set; } = new();
    }
}
