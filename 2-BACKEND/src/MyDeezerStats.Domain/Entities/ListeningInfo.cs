using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDeezerStats.Domain.Entities
{
    public class ListeningInfo
    {
        public string Name { get; set; } = string.Empty;
        public int Count { get; set; }
        public int TotalDuration { get; set; }
        public int TotalListening { get; set; }
    }
}
