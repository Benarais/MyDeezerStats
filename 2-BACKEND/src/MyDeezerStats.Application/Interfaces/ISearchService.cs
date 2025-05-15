using MyDeezerStats.Application.Dtos.Search;
using MyDeezerStats.Application.Dtos.TopStream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDeezerStats.Application.Interfaces
{
    public interface ISearchService
    {
        Task<List<SearchSuggestion>> SearchAsync(string query);
    }
}
