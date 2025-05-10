using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyDeezerStats.Application.Interfaces;

namespace MyDeezerStats.API.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class ListeningController : Controller
    {
        private readonly IListeningService _service;
        private readonly ILogger<ListeningController> _logger;

        public ListeningController(IListeningService service, ILogger<ListeningController> logger)
        {
            _service = service;
            _logger = logger;
        }


        [HttpGet("top-albums")]
        public async Task<IActionResult> GetTopAlbums([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            _logger.LogInformation("GET /top-albums called with from={From} to={To}", from, to);
            var result = await _service.GetTopAlbumsAsync(from, to);
            _logger.LogInformation($"Found {result}");
            return Ok(result);
        }

        //[Authorize]
        [HttpGet("top-artists")]
        public async Task<IActionResult> GetTopArtists([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            _logger.LogInformation("GET /top-artists called with from={From} to={To}", from, to);
            var result = await _service.GetTopArtistsAsync(from, to);
            _logger.LogInformation($"Found {result}");
            return Ok(result);
        }

        //[Authorize]
        [HttpGet("top-tracks")]
        public async Task<IActionResult> GetTopTracks([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            _logger.LogInformation("GET /top-tracks called with from={From} to={To}", from, to);
            var result = await _service.GetTopTracksAsync(from, to);
            _logger.LogInformation($"Found {result}");
            return Ok(result);
        }

        [Authorize]
        [HttpGet("recent")]
        public async Task<IActionResult> GetRecentTracks()
        {
            _logger.LogInformation("GET /recent tracks");
            var result = await _service.GetLatestListeningsAsync();
            return Ok(result);
        }
    }
}
