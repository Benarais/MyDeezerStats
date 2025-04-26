using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDeezerStats.Domain.Entities
{
    public class Listening
    {
        public string Id { get; set; } = string.Empty;
        public string SongName { get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;
        public string Album { get; set; } = string.Empty;
        public DateTime ListeningDate { get; set; }
    }
}
