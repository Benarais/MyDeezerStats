using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MyDeezerStats.Application.Dtos
{
    public class DeezerArtistInfos
    {
        public string Artist { get; set; } = string.Empty;
        public string CoverUrl { get; set; } = string.Empty;
        public int Count { get; set; }
        public int TotalListening { get; set; }
    }

    public class DeezerArtist
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("link")]
        public string Link { get; set; } = string.Empty;

        [JsonPropertyName("picture")]
        public string Picture { get; set; } = string.Empty;

        public string? Biography { get; set; }
    }


    public class DeezerPicture
    {
        public string Url { get; set; } = string.Empty; 
        public int Width { get; set; }
        public int Height { get; set; } 
    }
}
