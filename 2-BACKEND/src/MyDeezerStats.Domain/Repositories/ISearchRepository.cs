using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDeezerStats.Domain.Repositories
{
    public interface ISearchRepository
    {
        Task<Dictionary<string,string>> GetListAlbum(string query);
        Task<List<string>> GetListArtist(string query);
    }
}
