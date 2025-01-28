using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ToFood.Domain.DB.Relational;
using ToFood.Domain.DTOs.Response;
using ToFood.Domain.Entities.Relational;
using ToFood.Domain.Enums;
using ToFood.Domain.Helpers;

namespace ToFood.Domain.Services;

public class VideoService
{
    
    private readonly ToFoodRelationalContext _dbRelationalContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<VideoService> _logger;
    private readonly NotificationService _notificationService;

    public VideoService(
        ToFoodRelationalContext dbContext,
        IHttpContextAccessor httpContextAccessor,
        ILogger<VideoService> logger,
        NotificationService notificationService
        )
    {
        _dbRelationalContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        _notificationService = notificationService;
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

    /// <summary>
    /// Processa um único vídeo: salva no banco e gera imagens.
    /// </summary>
    public async Task<Video> ProcessVideo(IFormFile file, string outputPath)
    {
        _logger.LogInformation("Iniciando processamento do vídeo {FileName}.", file.FileName);

        // Valida o arquivo
        if (file == null || file.Length == 0)
        {
            _logger.LogWarning("Arquivo inválido: {FileName}.", file?.FileName ?? "Desconhecido");
            throw new ArgumentException("Arquivo inválido.");
        }

        if (!file.FileName.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) &&
            !file.FileName.EndsWith(".avi", StringComparison.OrdinalIgnoreCase) &&
            !file.FileName.EndsWith(".mov", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Formato de vídeo inválido: {FileName}.", file.FileName);
            throw new ArgumentException("Formato de vídeo inválido. Aceitamos apenas .mp4, .avi ou .mov.");
        }

        // Recupera ID do usuário do JWT de autenticação
        var userId = JWTHelper.GetAuthenticatedUserId(_httpContextAccessor);
        _logger.LogInformation("Usuário autenticado: {UserId}.", userId);

        byte[] videoData;
        using (var memoryStream = new MemoryStream())
        {
            await file.CopyToAsync(memoryStream);
            videoData = memoryStream.ToArray();
        }

        // Cria o registro do vídeo no banco
        var video = new Video
        {
            Id = Guid.NewGuid(),
            FileName = file.FileName,
            FilePath = "",
            FileData = videoData,
            Status = VideoStatus.Completed,
            UserId = userId
        };

        _dbRelationalContext.Videos.Add(video);
        await _dbRelationalContext.SaveChangesAsync();

        var videoPath = Path.Combine(outputPath, $"{video.Id}_{file.FileName}");
        video.FilePath = videoPath;

        try
        {
            using (var stream = new FileStream(videoPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            await _dbRelationalContext.SaveChangesAsync();
            _logger.LogInformation("Vídeo {FileName} salvo em {VideoPath}.", file.FileName, videoPath);
        }
        catch (Exception ex)
        {
            video.Status = VideoStatus.Failed;
            await _dbRelationalContext.SaveChangesAsync();
            await _notificationService.CreateAndSendNotificationAsync(fileId: video.Id);
            _logger.LogError(ex, "Erro ao salvar o vídeo {FileName}.", file.FileName);
            throw;
        }

        return video;
    }


}
