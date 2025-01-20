using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ToFood.Domain.DB.Relational;
using ToFood.Domain.DTOs.Response;
using ToFood.Domain.Helpers;

namespace ToFood.Domain.Services;

public class VideoService
{
    
    private readonly ToFoodRelationalContext _dbRelationalContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<VideoService> _logger;

    public VideoService(ToFoodRelationalContext dbContext, IHttpContextAccessor httpContextAccessor, ILogger<VideoService> logger)
    {
        _dbRelationalContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }



    /// <summary>
    /// Busca um arquivo Video armazenado no banco.
    /// </summary>
    /// <param name="videoId">Identificador do vídeo.</param>
    /// <returns>Os dados binários do vídeo e o nome do arquivo.</returns>
    /// <exception cref="FileNotFoundException">Lançada se o vídeo não for encontrado.</exception>
    public async Task<(byte[]?, string)> DownloadVideo(Guid videoId)
    {
        _logger.LogInformation("Iniciando a busca do vídeo com ID {VideoId}.", videoId);

        // Busca apenas os campos necessários no banco
        var videoData = await _dbRelationalContext.Videos
            .AsNoTracking()
            .Where(v => v.Id == videoId)
            .Select(v => new
            {
                FileData = v.FileData, // Dados binários do vídeo
                FileName = v.FileName != null ? Path.GetFileNameWithoutExtension(v.FileName) : null // Nome sem extensão
            })
            .FirstOrDefaultAsync();

        if (videoData == null)
        {
            _logger.LogWarning("Vídeo com ID {VideoId} não encontrado no banco de dados.", videoId);
            throw new FileNotFoundException($"Vídeo com ID {videoId} não encontrado.");
        }

        if (videoData.FileData == null)
        {
            _logger.LogWarning("Dados do vídeo com ID {VideoId} não encontrados no banco de dados.", videoId);
            throw new FileNotFoundException($"Dados do vídeo com ID {videoId} não encontrados.");
        }

        _logger.LogInformation("Vídeo com ID {VideoId} encontrado e pronto para download.", videoId);

        // Retorna os dados do vídeo e o nome sem extensão
        return (videoData.FileData, $"{videoData.FileName}.mp4");
    }

    /// <summary>
    /// Lista os vídeos vinculados a um usuário.
    /// </summary>
    /// <param name="userId">ID do usuário.</param>
    /// <returns>Lista de vídeos vinculados ao usuário.</returns>
    public async Task<List<VideoResponse>> ListVideosByUser()
    {
        // Recupera o ID do usuário logado do JWT
        var userId = JWTHelper.GetAuthenticatedUserId(_httpContextAccessor);

        if (userId == Guid.Empty)
        {
            return new List<VideoResponse>();
        };

        return await _dbRelationalContext.Videos
            .AsNoTracking()
            .Where(v => v.UserId == userId)
            .Select(v => new VideoResponse
            {
                Id = v.Id,
                FileName = Path.GetFileNameWithoutExtension(v.FileName),
                CreatedAt = v.CreatedAt,
                Status = v.Status.ToEnumDescription(),
            })
            .ToListAsync();
    }

}
