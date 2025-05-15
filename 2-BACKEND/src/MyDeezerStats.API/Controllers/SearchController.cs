using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyDeezerStats.Application.Interfaces;


namespace MyDeezerStats.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SearchController : ControllerBase
    {
        private readonly ISearchService _searchService;

        public SearchController(ISearchService searchService)
        {
            _searchService = searchService;
        }

        [HttpGet("suggest")]
        public async Task<IActionResult> Suggest([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest("Query parameter is required.");
            }

            try
            {
                var results = await _searchService.SearchAsync(query);
                return Ok(results);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error during search: {ex.Message}");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
    }
}
