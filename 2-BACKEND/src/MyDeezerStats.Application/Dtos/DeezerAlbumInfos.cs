using System.Text.Json.Serialization;

namespace MyDeezerStats.Application.Dtos
{
    public class DeezerAlbumInfos
    {
        public string Title { get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;
        public string CoverUrl { get; set; } = string.Empty;
        public int Count { get; set; }
        public int TotalListening { get; set; }
        public int TotalDuration { get; set; } 
    }

    public class DeezerAlbumResponse
    {
        public string cover_medium { get; set; } = string.Empty;
        public int duration { get; set; }

        [JsonPropertyName("tracks")]
        public TrackWrapper? TracksData { get; set; }

        public class TrackWrapper
        {
            public List<DeezerTrack> data { get; set; } = new();
        }
    }

    public class DeezerSearchResponse<T>
    {
        public List<T> data { get; set; } = new List<T>();
    }

    public class DeezerAlbum
    {
        public int id { get; set; }
        public string title { get; set; } = string.Empty;
        public string link { get; set; } = string.Empty;
        public string cover_medium { get; set; } = string.Empty;
    }
}
