

namespace MyDeezerStats.Application.Dtos
{

    public class ShortAlbumInfos
    {
        public string Title { get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;
        public string CoverUrl { get; set; } = string.Empty;
        public int Count { get; set; }
    }


    public class FullAlbumInfos
    {
        public string Title { get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;
        public int PlayCount { get; set; }
        public int TotalDuration { get; set; }
        public int TotalListening { get; set; }
        public string ReleaseDate { get; set; } = string.Empty;
        public List<ApiTrackInfos> TrackInfos { get; set; } = [];
        public string CoverUrl { get; set; } = string.Empty;
    }
}
