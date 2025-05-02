using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDeezerStats.Domain.Entities.DeezerInfos
{
    public class TrackInfos
    {
        public string Title { get; set; } = string.Empty;
        public string Album {  get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;
        public int Duration { get; set; } = 0;
        public string TrackUrl {  get; set; } = string.Empty;
    }
}
