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

namespace ToFood.Domain.Services;

public class ZipService
{
    private readonly string _uploadPath = "Uploads"; // Diretório para armazenar os vídeos enviados
    private readonly string _outputPath = Path.Combine("Output", "Zips"); // Diretório para armazenar os arquivos ZIP gerados
    private readonly ToFoodRelationalContext _dbRelationalContext; // Contexto do banco de dados
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<ZipService> _logger; // Logger para o serviço

    public ZipService(ToFoodRelationalContext dbContext, IHttpContextAccessor httpContextAccessor, ILogger<ZipService> logger)
    {
        _dbRelationalContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;

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
            tasks.Add(ProcessarVideoAsync(file));
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
    /// Processa um único vídeo: converte em imagens, gera um arquivo ZIP e salva os dados no banco.
    /// </summary>
    private async Task<string> ProcessarVideoAsync(IFormFile file)
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

        byte[] VideoData;
        using (var memoryStream = new MemoryStream())
        {
            await file.CopyToAsync(memoryStream);
            VideoData = memoryStream.ToArray();
        }

        // Cria o registro do vídeo no banco
        var video = new Video
        {
            Id = Guid.NewGuid(),
            FileName = file.FileName,
            FilePath = "", // Opcional se o caminho não for necessário
            FileData = VideoData, // Armazena os dados binários
            Status = VideoStatus.Completed,
            UserId = userId
        };

        _dbRelationalContext.Videos.Add(video);
        await _dbRelationalContext.SaveChangesAsync();

        var uniqueId = Guid.NewGuid().ToString();
        var videoPath = Path.Combine(_uploadPath, $"{uniqueId}_{file.FileName}");

        try
        {
            using (var stream = new FileStream(videoPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            video.FilePath = videoPath;
            await _dbRelationalContext.SaveChangesAsync();

            _logger.LogInformation("Vídeo {FileName} salvo em {VideoPath}.", file.FileName, videoPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao salvar o vídeo {FileName}.", file.FileName);
            throw;
        }

        // Extração de imagens e criação do ZIP
        var outputFolder = Path.Combine(_outputPath, $"{Path.GetFileNameWithoutExtension(file.FileName)}_{uniqueId}");
        Directory.CreateDirectory(outputFolder);

        var zipFilePath = "";
        try
        {
            var mediaInfo = await FFmpeg.GetMediaInfo(videoPath);
            var videoDuration = mediaInfo.VideoStreams.First().Duration.TotalSeconds;

            var snapshotTasks = new List<Task>();
            for (int i = 0; i < videoDuration; i++)
            {
                var outputImagePath = Path.Combine(outputFolder, $"frame_{i:D3}.png");
                var conversion = await FFmpeg.Conversions.FromSnippet.Snapshot(videoPath, outputImagePath, TimeSpan.FromSeconds(i));
                snapshotTasks.Add(conversion.Start());
            }

            await Task.WhenAll(snapshotTasks);

            zipFilePath = Path.Combine(_outputPath, $"{Path.GetFileNameWithoutExtension(file.FileName)}_{uniqueId}.zip");
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
                UserId = userId,
                FileData = zipData, // Salva os bytes no banco

            };

            _dbRelationalContext.ZipFiles.Add(zipFile);
            await _dbRelationalContext.SaveChangesAsync();

            _logger.LogInformation("ZIP gerado com sucesso: {Response}", SanitizerHelper.Sanitize(zipFile));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar o vídeo {FileName}.", file.FileName);
            throw;
        }
        finally
        {
            Directory.Delete(outputFolder, true);
            File.Delete(videoPath);
        }

        return zipFilePath;
    }

    /// <summary>
    /// Busca um arquivo ZIP armazenado no banco.
    /// </summary>
    /// <param name="zipId">Identificador do ZIP.</param>
    /// <returns>Bytes do arquivo ZIP e o nome do arquivo.</returns>
    public async Task<(byte[]?, string)> GetZipFileAsync(Guid zipId)
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

}
