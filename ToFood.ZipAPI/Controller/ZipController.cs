using Microsoft.AspNetCore.Mvc;
using ToFood.Domain.Services;

namespace ToFood.ZipAPI.Controllers;

[ApiController]
[Route("zip")]
public class ZipController : ControllerBase
{
    private readonly ZipService _zipService;

    public ZipController(ZipService zipServices)
    {
        _zipService = zipServices; // Instancia o serviço de processamento de ZIPs
    }

    /// <summary>
    /// Recebe uma lista de vídeos, converte cada um em imagens e retorna um arquivo ZIP consolidado.
    /// </summary>
    /// <param name="files">Lista de arquivos de vídeo enviados pelo usuário.</param>
    /// <returns>Um arquivo ZIP contendo as imagens extraídas de todos os vídeos.</returns>
    [HttpPost("convert-videos-to-images")]
    public async Task<IActionResult> ConvertVideosToImageZip([FromForm] List<IFormFile> files)
    {
        try
        {
            // Verifica se os arquivos foram enviados
            if (files == null || files.Count == 0)
                return BadRequest("Nenhum arquivo foi enviado.");

            // Chama o serviço para processar os vídeos e gerar o ZIP consolidado
            var zipBytes = await _zipService.ConvertVideosToImageZip(files);

            // Retorna o arquivo ZIP gerado
            return File(zipBytes, "application/zip", "VideosProcessados.zip");
        }
        catch (Exception ex)
        {
            // Retorna erro detalhado em caso de falha
            return BadRequest(new { Error = ex.Message });
        }
    }
}
