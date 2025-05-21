using MyDeezerStats.Application.Dtos.LastStream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDeezerStats.Application.Interfaces
{
    public interface ILastFmService
    {
        public Task<List<ListeningDto>> GetListeningHistorySince(DateTime sinceDate);
    }
}
