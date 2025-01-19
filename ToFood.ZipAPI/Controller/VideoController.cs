using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ToFood.Domain.Services;
using ToFood.Domain.DTOs.Request;
using ToFood.Domain.DTOs.Response;

namespace ToFood.ZipAPI.Controller;

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
    /// <param name="request">O modelo contendo a URL do vídeo do YouTube.</param>
    /// <returns>O vídeo MP4.</returns>
    [HttpPost("download/youtube")]
    [AllowAnonymous]
    public async Task<IActionResult> DownloadYoutubeVideo([FromBody] DownloadYoutubeVideoRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.YoutubeUrl) || !request.YoutubeUrl.Contains("tube"))
            return BadRequest(new { Error = "A URL do YouTube está inválida." });

        var videoFilePath = await _youtubeService.DownloadYoutubeVideo(request.YoutubeUrl);

        try
        {
            // Lê o conteúdo do arquivo MP4
            var fileBytes = await System.IO.File.ReadAllBytesAsync(videoFilePath);

            // Retorna o arquivo MP4
            return File(fileBytes, "video/mp4", Path.GetFileName(videoFilePath));
        }
        catch (Exception ex)
        {
            return BadRequest(new DownloadYoutubeVideoResponse
            {
                FileName = null,
                Message = $"Erro ao processar o vídeo: {ex.Message}"
            });
        }
        finally
        {

            // Extrai o nome do arquivo (sem extensão) da variável videoFilePath
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(videoFilePath);

            if (System.IO.File.Exists(videoFilePath))
            {
                try
                {
                    System.IO.File.Delete(videoFilePath);
                    Console.WriteLine($"Arquivo deletado: {fileNameWithoutExtension}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao deletar o arquivo: {ex.Message}");
                }
            }

        }
    }
}
