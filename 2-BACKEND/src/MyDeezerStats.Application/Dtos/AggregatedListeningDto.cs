using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDeezerStats.Application.Dtos
{
    public class AggregatedListeningDto
    {
        public string Key { get; set; } = string.Empty;
        public int Count { get; set; }
        public string? Artist { get; set; }
        public string? Album { get; set; }

    }
}
