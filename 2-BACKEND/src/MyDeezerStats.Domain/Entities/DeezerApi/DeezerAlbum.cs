using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MyDeezerStats.Domain.Entities.DeezerApi
{
    public class DeezerAlbum
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("artist")]
        public DeezerArtist Artist { get; set; } = new();

        [JsonPropertyName("cover_xl")]
        public string CoverXl { get; set; } = string.Empty;

        [JsonPropertyName("cover_big")]
        public string CoverBig { get; set; } = string.Empty;
    }
}
