using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDeezerStats.Domain.Entities
{
    public class ArtistStatistic
    {
        public string Artist { get; set; } = string.Empty;
        public string ArtistUrl { get; set; } = string.Empty;
        public int ArtistListeningDuration { get; set; }
        public int ArtistListeningCount { get; set; }
        public List<ListeningInfo> Listening { get; set; } = new List<ListeningInfo> { };
    }
}
