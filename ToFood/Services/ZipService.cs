using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.IO.Compression;
using ToFood.Domain.DB.Relational;
using ToFood.Domain.Entities.Relational;
using ToFood.Domain.Enums;
using ToFood.Domain.Utils;
using ToFood.Domain.Helpers;
using Xabe.FFmpeg;
using Microsoft.EntityFrameworkCore;
using ToFood.Domain.DTOs.Response;

namespace ToFood.Domain.Services;

public class ZipService
{
    private readonly string _uploadPath = "Uploads"; // Diretório para armazenar os vídeos enviados
    private readonly string _outputPath = Path.Combine("Output", "Zips"); // Diretório para armazenar os arquivos ZIP gerados
    private readonly ToFoodRelationalContext _dbRelationalContext; // Contexto do banco de dados
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<ZipService> _logger; // Logger para o serviço
    private readonly NotificationService _notificationService;
    private readonly VideoService _videoService;

    public ZipService(
        ToFoodRelationalContext dbContext,
        IHttpContextAccessor httpContextAccessor,
        ILogger<ZipService> logger,
        NotificationService notificationService,
        VideoService videoService)
    {
        _dbRelationalContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        _notificationService = notificationService;
        _videoService = videoService;


        // Garante que os diretórios de upload e saída existam
        Directory.CreateDirectory(_uploadPath);
        Directory.CreateDirectory(_outputPath);
    }

    /// <summary>
    /// Recebe uma lista de vídeos, converte cada um em imagens e retorna o caminho do arquivo ZIP consolidado.
    /// </summary>
    public async Task<string> ConvertVideosToImageZipToPath(List<IFormFile> files)
    {
        if (files == null || files.Count == 0)
        {
            _logger.LogWarning("Nenhum arquivo foi enviado para processamento.");
            throw new Exception("Nenhum arquivo foi enviado.");
        }

        _logger.LogInformation("Iniciando processamento de {FileCount} vídeos.", files.Count);

        var tasks = new List<Task<string>>(); // Lista de tarefas para processar cada vídeo
        var processedZips = new List<string>(); // Lista para armazenar os caminhos dos ZIPs gerados

        // Verifica e configura os executáveis do FFmpeg
        await FFmpegUtils.EnsureFFmpegIsConfigured();
        _logger.LogInformation("FFmpeg configurado com sucesso.");

        foreach (var file in files)
        {
            // Adiciona as tarefas para processar cada vídeo individualmente
            tasks.Add(Task.Run(async () =>
            {
                var video = await _videoService.ProcessVideo(file, _outputPath);
                return await GenerateZipFromVideo(video);
            }));
        }

        try
        {
            // Processa todos os vídeos de forma assíncrona
            processedZips = (await Task.WhenAll(tasks)).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar vídeos.");
            throw;
        }

        // Gera um identificador único para o ZIP consolidado
        Guid zipId = Guid.NewGuid();
        var zipDate = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        var consolidatedZipPath = Path.Combine(_outputPath, $"Zip_{zipId}_{zipDate}.zip");

        // Cria o arquivo ZIP consolidado contendo todos os ZIPs individuais
        try
        {
            using (var zipStream = new FileStream(consolidatedZipPath, FileMode.Create))
            {
                using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create))
                {
                    foreach (var zipPath in processedZips)
                    {
                        var entryName = Path.GetFileName(zipPath);
                        archive.CreateEntryFromFile(zipPath, entryName);
                    }
                }
            }

            _logger.LogInformation("ZIP consolidado criado com sucesso: {ConsolidatedZipPath}", consolidatedZipPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar o ZIP consolidado.");
            throw;
        }

        // Remove os arquivos ZIP temporários gerados
        foreach (var zipPath in processedZips)
        {
            File.Delete(zipPath);
        }

        _logger.LogInformation("Processamento concluído. ZIP consolidado disponível em {ConsolidatedZipPath}.", consolidatedZipPath);

        return consolidatedZipPath; // Retorna o caminho do ZIP consolidado
    }

    /// <summary>
    /// Busca um arquivo ZIP armazenado no banco.
    /// </summary>
    /// <param name="zipId">Identificador do ZIP.</param>
    /// <returns>Bytes do arquivo ZIP e o nome do arquivo.</returns>
    public async Task<(byte[]?, string)> GetZipFile(Guid zipId)
    {
        var zipFile = await _dbRelationalContext.ZipFiles
        .AsNoTracking()
        .Where(z => z.Id == zipId)
        .Select(z => new
        {
            VideoFileName = z.Video != null ? Path.GetFileNameWithoutExtension(z.Video.FileName) : null, // Remove a extensão
            z.FileData, // Dados do arquivo ZIP
            z.FilePath  // Caminho do arquivo ZIP (se necessário)
        })
        .FirstOrDefaultAsync();

        if (zipFile == null)
            return (null, string.Empty);

        if (zipFile.FileData != null)
        {
            // Retorna os bytes do banco
            return (zipFile.FileData, $"{zipFile?.VideoFileName}.zip");
        }
        else if (!string.IsNullOrEmpty(zipFile.FilePath) && File.Exists(zipFile.FilePath))
        {
            // Lê os bytes do sistema de arquivos
            var fileBytes = await File.ReadAllBytesAsync(zipFile.FilePath);
            return (fileBytes, Path.GetFileName(zipFile.FilePath));
        }

        return (null, string.Empty);
    }

    /// <summary>
    /// Lista os Zips vinculados a um usuário.
    /// </summary>
    /// <returns>Lista de Zips vinculados ao usuário.</returns>
    public async Task<List<ZipResponse>> ListZipsByUser()
    {
        // Recupera o ID do usuário logado do JWT
        var userId = JWTHelper.GetAuthenticatedUserId(_httpContextAccessor);

        if (userId == Guid.Empty)
        {
            return new List<ZipResponse>();
        }

        return await _dbRelationalContext.ZipFiles
            .AsNoTracking()
            .Where(z => z.UserId == userId)
            .Select(z => new ZipResponse
            {
                Id = z.Id,
                FileName = z.Video != null ? Path.GetFileNameWithoutExtension(z.Video.FileName) : null, // Remove a extensão,
                CreatedAt = z.CreatedAt,
                Status = z.Status.ToEnumDescription(),
            })
            .ToListAsync();
    }

    /// <summary>
    /// Gera imagens do vídeo e cria um arquivo ZIP.
    /// </summary>
    private async Task<string> GenerateZipFromVideo(Video video)
    {
        var outputFolder = Path.Combine(_outputPath, $"{Path.GetFileNameWithoutExtension(video.FileName)}_{video.Id}");
        Directory.CreateDirectory(outputFolder);

        var zipFilePath = "";
        try
        {
            var mediaInfo = await FFmpeg.GetMediaInfo(video.FilePath);
            var videoDuration = mediaInfo.VideoStreams.First().Duration.TotalSeconds;

            var snapshotTasks = new List<Task>();
            for (int i = 0; i < videoDuration; i++)
            {
                var outputImagePath = Path.Combine(outputFolder, $"frame_{i:D3}.png");
                var conversion = await FFmpeg.Conversions.FromSnippet.Snapshot(video.FilePath, outputImagePath, TimeSpan.FromSeconds(i));
                snapshotTasks.Add(conversion.Start());
            }

            await Task.WhenAll(snapshotTasks);

            zipFilePath = Path.Combine(_outputPath, $"{Path.GetFileNameWithoutExtension(video.FileName)}_{video.Id}.zip");
            System.IO.Compression.ZipFile.CreateFromDirectory(outputFolder, zipFilePath);

            // Lê os bytes do ZIP
            byte[] zipData;
            using (var fileStream = new FileStream(zipFilePath, FileMode.Open, FileAccess.Read))
            {
                using (var memoryStream = new MemoryStream())
                {
                    await fileStream.CopyToAsync(memoryStream);
                    zipData = memoryStream.ToArray();
                }
            }

            var zipFile = new Entities.Relational.ZipFile
            {
                Id = Guid.NewGuid(),
                FilePath = zipFilePath,
                Status = ZipStatus.Completed,
                VideoId = video.Id,
                UserId = video.UserId,
                FileData = zipData,
            };

            _dbRelationalContext.ZipFiles.Add(zipFile);
            await _dbRelationalContext.SaveChangesAsync();

            _logger.LogInformation("ZIP gerado com sucesso: {Response}", SanitizerHelper.Sanitize(zipFile));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar o vídeo {FileName}.", video.FileName);
            throw;
        }
        finally
        {
            Directory.Delete(outputFolder, true);
            File.Delete(video.FilePath);
        }

        return zipFilePath;
    }

}
