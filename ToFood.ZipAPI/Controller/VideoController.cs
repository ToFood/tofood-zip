using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ToFood.Domain.Services;
using ToFood.Domain.DTOs.Request;
using ToFood.Domain.DTOs.Response;
using ToFood.Domain.Helpers;

namespace ToFood.ZipAPI.Controller;

[ApiController]
[Route("videos")]
public class VideoController : ControllerBase
{
    private readonly YoutubeService _youtubeService;
    private readonly VideoService _videoService;
    private readonly IHttpContextAccessor _contextAccessor;

    public VideoController(YoutubeService youtubeService, VideoService videoService, IHttpContextAccessor contextAccessor)
    {
        _youtubeService = youtubeService;
        _videoService = videoService;
        _contextAccessor = contextAccessor;
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

    /// <summary>
    /// Faz o Download do Video
    /// </summary>
    /// <param name="videoId"></param>
    /// <returns></returns>
    [HttpGet("download/{videoId}")]
    public async Task<IActionResult> DownloadVideo(Guid videoId)
    {
        try
        {
            var (videoBytes, fileName) = await _videoService.DownloadVideo(videoId);

            if (videoBytes == null)
                return NotFound("Vídeo não encontrado.");

            return File(videoBytes, "video/mp4", fileName);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro ao baixar o vídeo: {ex.Message}");
        }
    }

    /// <summary>
    /// Lista os vídeos vinculados ao usuário logado.
    /// </summary>
    /// <returns>Uma lista com os vídeos do usuário.</returns>
    [HttpGet("list")]
    public async Task<IActionResult> ListVideos()
    {
        try
        {
            // Recupera o ID do usuário logado do JWT
            var userId = JWTHelper.GetAuthenticatedUserId(_contextAccessor);

            if (userId == Guid.Empty || userId == null)
                return Unauthorized("Usuário não autenticado ou ID inválido.");

            // Obtém os vídeos do serviço
            var videos = await _videoService.ListVideosByUser(userId);

            // Retorna a lista de vídeos
            return Ok(videos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro ao listar vídeos: {ex.Message}");
        }
    }

}
