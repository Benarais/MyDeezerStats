using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDeezerStats.Application.Dtos.Search
{
    public class SearchSuggestion
    {
        public EntityType Type { get; set; }
        public string Artist { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
    }
}
