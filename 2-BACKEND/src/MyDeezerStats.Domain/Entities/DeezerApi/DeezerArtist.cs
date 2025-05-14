using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDeezerStats.Domain.Entities.DeezerApi
{
    public class DeezerArtist
    {
        // Identité
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Link { get; set; } = string.Empty;

        // Images (plusieurs tailles disponibles)
        public string Picture { get; set; } = string.Empty;       // 56x56
        public string PictureSmall { get; set; } = string.Empty;   // 250x250
        public string PictureMedium { get; set; } = string.Empty; // 500x500
        public string PictureBig { get; set; } = string.Empty;    // 1000x1000
        public string PictureXl { get; set; } = string.Empty;     // 1500x1500

        // Statistiques
        public int NbAlbum { get; set; }
        public int NbFan { get; set; }

        // Métadonnées
        public string Tracklist { get; set; } = string.Empty; // URL API des tracks
        public string Type { get; set; } = "artist";

    }
}
