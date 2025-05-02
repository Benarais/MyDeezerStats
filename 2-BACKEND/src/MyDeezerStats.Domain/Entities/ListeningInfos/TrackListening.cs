using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDeezerStats.Domain.Entities.ListeningInfos
{
    public class TrackListening
    {
        public string Name { get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;
        public string Album { get; set; } = string.Empty;   
        public int StreamCount { get; set; } 
    }
}
