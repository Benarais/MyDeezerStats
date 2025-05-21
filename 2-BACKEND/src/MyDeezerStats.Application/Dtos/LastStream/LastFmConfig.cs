using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDeezerStats.Application.Dtos.LastStream
{
    public class LastFmOptions
    {
        public string ApiKey { get; set; } = null!;
        public string Username { get; set; } = null!;
    }
}
