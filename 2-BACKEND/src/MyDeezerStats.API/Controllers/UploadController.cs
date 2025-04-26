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

        //[HttpPost("import-excel")]
        //[RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        //public IActionResult ImportExcel()
        //{
        //    // Log du Content-Type reçu
        //    Console.WriteLine($"Content-Type reçu : {Request.ContentType}");

        //    // Vérification des en-têtes de la requête
        //    foreach (var header in Request.Headers)
        //    {
        //        Console.WriteLine($"Header : {header.Key} = {header.Value}");
        //    }
        //    foreach (var f in Request.Form.Files)
        //    {
        //        Console.WriteLine($"Fichier détecté : {f.Name} ({f.FileName})");
        //    }
        //    // Vérification du formulaire
        //    var form = Request.Form;
        //    Console.WriteLine($"Form Keys: {string.Join(", ", form.Keys)}");

        //    // Vérification de la présence du fichier
        //    var file = form.Files["file"];
        //    if (file == null)
        //    {
        //        Console.WriteLine("Aucun fichier trouvé dans la requête.");
        //        return BadRequest("Fichier non reçu.");
        //    }

        //    // Log du fichier reçu
        //    Console.WriteLine($"Fichier reçu : {file.FileName}, Taille : {file.Length} octets");

        //    return Ok($"Reçu fichier : {file.FileName}");
        //}


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
