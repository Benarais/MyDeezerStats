using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
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

        [Authorize]
        [HttpGet("top-albums")]
        public async Task<IActionResult> GetTopAlbums([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            _logger.LogInformation("GET /top-albums called with from={From} to={To}", from, to);
            var result = await _service.GetTopAlbumsAsync(from, to);
            _logger.LogInformation($"Found {result}");
            return Ok(result);
        }

        [Authorize]
        [HttpGet("album")]
        public async Task<IActionResult> GetAlbum([FromQuery] string? identifier)
        {
            _logger.LogInformation("GET /album identifier = {identifier}", identifier);

            if (string.IsNullOrWhiteSpace(identifier))
            {
                _logger.LogWarning("Album identifier is null or empty");
                return BadRequest("Identifier is required");
            }

            try
            {
                var result = await _service.GetAlbumAsync(identifier);
                _logger.LogInformation("Successfully retrieved album {Identifier}", identifier);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving album {Identifier}", identifier);
                return StatusCode(500, "An error occurred while processing your request");
            }
        }

        [Authorize]
        [HttpGet("top-artists")]
        public async Task<IActionResult> GetTopArtists([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            _logger.LogInformation("GET /top-artists called with from={From} to={To}", from, to);
            var result = await _service.GetTopArtistsAsync(from, to);
            _logger.LogInformation($"Artistes trouvés: {result.Count}");
            return Ok(result);
        }

        [Authorize]
        [HttpGet("artist")]
        public async Task<IActionResult> GetArtist([FromQuery] string? identifier)
        {
            _logger.LogInformation("GET /artist identifier = {identifier}", identifier);

            if (string.IsNullOrWhiteSpace(identifier))
            {
                _logger.LogWarning("Artist identifier is null or empty");
                return BadRequest("Identifier is required");
            }

            try
            {
                var result = await _service.GetArtistAsync(identifier);
                _logger.LogInformation("Successfully retrieved artist {Identifier}", identifier);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving artist {Identifier}", identifier);
                return StatusCode(500, "An error occurred while processing your request");
            }
        }

        [Authorize]
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
