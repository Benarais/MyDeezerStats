using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDeezerStats.Domain.Entities.DeezerApi
{
    public class DeezerTrackList
    {
        public List<DeezerTrack> Data { get; set; } = new();
    }
}
