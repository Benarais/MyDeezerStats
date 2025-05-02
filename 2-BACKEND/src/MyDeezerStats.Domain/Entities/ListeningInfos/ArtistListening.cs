using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDeezerStats.Domain.Entities.ListeningInfos
{
    public class ArtistListening
    {
        public string Name { get; set; } = string.Empty;
        public int StreamCount { get; set; }
        public Dictionary<string, int> StreamCountByTrack { get; set; } = new Dictionary<string, int>();
    }
}
