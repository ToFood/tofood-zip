using Microsoft.AspNetCore.Mvc;
using ToFood.Domain.Services;

namespace ToFood.ZipAPI.Controllers;

[ApiController]
[Route("zip")]
public class ZipController : ControllerBase
{
    private readonly ZipService _zipService;

    public ZipController(ZipService zipService)
    {
        _zipService = zipService;
    }

    /// <summary>
    /// Recebe uma lista de vídeos, converte cada um em imagens e retorna um arquivo ZIP consolidado.
    /// </summary>
    /// <param name="files">Lista de arquivos de vídeo enviados pelo usuário.</param>
    /// <returns>Um arquivo ZIP contendo as imagens extraídas de todos os vídeos.</returns>
    [HttpPost("convert-videos-to-images")]
    public async Task<IActionResult> ConvertVideosToImageZip([FromForm] List<IFormFile> files)
    {
        string zipFilePath = string.Empty;

        try
        {
            if (files == null || files.Count == 0)
                return BadRequest("Nenhum arquivo foi enviado.");

            // Gera o arquivo ZIP consolidado
            zipFilePath = await _zipService.ConvertVideosToImageZipToPath(files);

            // Lê os bytes do ZIP
            var zipBytes = await System.IO.File.ReadAllBytesAsync(zipFilePath);

            return File(zipBytes, "application/zip", Path.GetFileName(zipFilePath));
        }
        finally
        {
            // Remove o ZIP consolidado após a resposta
            if (!string.IsNullOrEmpty(zipFilePath) && System.IO.File.Exists(zipFilePath))
            {
                System.IO.File.Delete(zipFilePath);
            }
        }
    }
}
