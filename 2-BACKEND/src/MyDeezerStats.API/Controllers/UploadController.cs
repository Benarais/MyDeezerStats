using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyDeezerStats.Application.Interfaces;

namespace MyDeezerStats.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UploadController : ControllerBase
    {
        private readonly IExcelService _excelService;

        public UploadController(IExcelService excelService)
        {
            _excelService = excelService;
        }


        [HttpPost("import-excel")]
        public async Task<IActionResult> ImportExcel([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Aucun fichier sélectionné.");
            }

            try
            {
                // Traitement du fichier
                using var stream = file.OpenReadStream();
                await _excelService.ProcessExcelFileAsync(stream);
                return Ok("Données importées avec succès.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Erreur lors de l'importation des données : {ex.Message}");
            }
        }
    }
}
