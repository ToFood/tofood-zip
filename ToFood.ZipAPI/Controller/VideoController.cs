using Microsoft.AspNetCore.Mvc;
using ToFood.Domain.Services;

namespace ToFood.ZipAPI.Controllers;

[ApiController]
[Route("videos")]
public class VideoController : ControllerBase
{
    private readonly YoutubeService _youtubeService;

    public VideoController(YoutubeService youtubeService)
    {
        _youtubeService = youtubeService;
    }

    /// <summary>
    /// Recebe uma URL do YouTube, faz o download do vídeo e retorna o arquivo MP4.
    /// </summary>
    /// <param name="youtubeUrl">A URL do vídeo do YouTube.</param>
    /// <returns>O vídeo MP4.</returns>
    [HttpPost("download")]
    public async Task<IActionResult> DownloadYoutubeVideo([FromBody] string youtubeUrl)
    {
        // Faz o download do vídeo
        var videoFilePath = await _youtubeService.DownloadYoutubeVideo(youtubeUrl);

        try
        {

            // Lê o conteúdo do arquivo MP4
            var fileBytes = await System.IO.File.ReadAllBytesAsync(videoFilePath);

            // Retorna o arquivo MP4
            return File(fileBytes, "video/mp4", Path.GetFileName(videoFilePath));
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
        finally
        {
            // Opcional: limpa o arquivo baixado após retornar ao cliente
            if (System.IO.File.Exists(videoFilePath))
            {
                System.IO.File.Delete(videoFilePath);
            }
        }
    }
}
