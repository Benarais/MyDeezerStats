using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MyDeezerStats.Application.Dtos.TopStream
{
    public class ApiTrackInfos : ApiEntity
    {
        public string Track { get; set; } = string.Empty;
        public string Album {  get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;
        public string TrackUrl { get; set; } = string.Empty;
        public int Count { get; set; }
        public int Duration { get; set; }
        public int TotalListening { get; set; }
        public string LastListen { get; set; } = string.Empty;
    }
}
