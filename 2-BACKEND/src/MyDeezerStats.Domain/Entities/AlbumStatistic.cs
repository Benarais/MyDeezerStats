using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDeezerStats.Domain.Entities
{
    public class AlbumStatistic
    {
        public string Album { get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;
        public string AlbumUrl { get; set; } = string.Empty;
        public int AlbumDuration { get; set; }
        public int AlbumTotalListening { get; set; }

        public int AlbumNumberListening { get; set; }
        public List<ListeningInfo> Listening { get; set; }  = new List<ListeningInfo> { };
    }
}
