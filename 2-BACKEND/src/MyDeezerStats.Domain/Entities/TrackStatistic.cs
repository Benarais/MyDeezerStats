using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDeezerStats.Domain.Entities
{
    public class TrackStatistic
    {
        public string Track { get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;
        public string Album { get; set; } = string.Empty;
        public string TrackUrl { get; set; } = string.Empty;
        public int TrackDuration { get; set; }
        public int TrackTotalListening { get; set; }

        public int TrackNumberListening { get; set; }
    }
}
