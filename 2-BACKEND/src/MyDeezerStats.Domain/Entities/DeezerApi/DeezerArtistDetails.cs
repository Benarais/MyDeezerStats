using MyDeezerStats.Domain.Entities.DeezerApi;
using System.Text.Json.Serialization;

public class DeezerArtistDetails
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("link")]
    public string Link { get; set; } = string.Empty;

    [JsonPropertyName("share")]
    public string Share { get; set; } = string.Empty;

    [JsonPropertyName("picture")]
    public string Picture { get; set; } = string.Empty;

    [JsonPropertyName("picture_small")]
    public string PictureSmall { get; set; } = string.Empty;

    [JsonPropertyName("picture_medium")]
    public string PictureMedium { get; set; } = string.Empty;

    [JsonPropertyName("picture_big")]
    public string PictureBig { get; set; } = string.Empty;

    [JsonPropertyName("picture_xl")]
    public string PictureXl { get; set; } = string.Empty;

    [JsonPropertyName("nb_album")]
    public int NbAlbum { get; set; }

    [JsonPropertyName("nb_fan")]
    public int NbFan { get; set; }

    [JsonPropertyName("radio")]
    public bool Radio { get; set; }

    [JsonPropertyName("tracklist")]
    public string Tracklist { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("tracks")]
    public DeezerTrackList Tracks { get; set; } = new();
}