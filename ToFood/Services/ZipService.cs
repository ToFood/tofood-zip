using System.IO.Compression;
using Microsoft.AspNetCore.Http;
using Xabe.FFmpeg;

namespace ToFood.Domain.Services;

public class ZipService
{
    private readonly string _uploadPath = "Uploads"; // Diretório para armazenar os vídeos enviados
    private readonly string _outputPath = Path.Combine("Output", "Zips"); // Diretório para armazenar os arquivos ZIP gerados

    public ZipService()
    {
        // Garante que os diretórios de upload e saída existam
        Directory.CreateDirectory(_uploadPath);
        Directory.CreateDirectory(_outputPath);
    }

    /// <summary>
    /// Recebe uma lista de vídeos, converte cada um em imagens e retorna o caminho do arquivo ZIP consolidado.
    /// </summary>
    /// <param name="files">Lista de arquivos de vídeo enviados pelo usuário.</param>
    /// <returns>O caminho do arquivo ZIP consolidado.</returns>
    public async Task<string> ConvertVideosToImageZipToPath(List<IFormFile> files)
    {
        if (files == null || files.Count == 0)
            throw new Exception("Nenhum arquivo foi enviado.");

        var tasks = new List<Task<string>>(); // Lista de tarefas para processar cada vídeo
        var processedZips = new List<string>(); // Lista para armazenar os caminhos dos ZIPs gerados

        // Verifica e configura os executáveis do FFmpeg
        await EnsureFFmpegIsConfigured();

        foreach (var file in files)
        {
            tasks.Add(ProcessarVideoAsync(file));
        }

        // Processa todos os vídeos de forma assíncrona
        processedZips = (await Task.WhenAll(tasks)).ToList();

        // Gera um identificador único para o ZIP consolidado
        Guid zipId = Guid.NewGuid();
        var zipDate = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

        // Cria o caminho do arquivo ZIP consolidado
        var consolidatedZipPath = Path.Combine(_outputPath, $"Zip_{zipId}_{zipDate}.zip");

        // Cria o arquivo ZIP consolidado contendo todos os ZIPs individuais
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

        // Remove os arquivos ZIP temporários gerados
        foreach (var zipPath in processedZips)
        {
            File.Delete(zipPath);
        }

        return consolidatedZipPath; // Retorna o caminho do ZIP consolidado
    }

    /// <summary>
    /// Garante que o FFmpeg esteja configurado e os executáveis estejam disponíveis.
    /// </summary>
    private async Task EnsureFFmpegIsConfigured()
    {
        var defaultFFmpegPath = Path.Combine(Directory.GetCurrentDirectory(), "ffmpeg_executables");
        if (!Directory.Exists(defaultFFmpegPath) || !File.Exists(Path.Combine(defaultFFmpegPath, "ffmpeg.exe")) ||
            !File.Exists(Path.Combine(defaultFFmpegPath, "ffprobe.exe")))
        {
            await DownloadFFmpegExecutables();
        }
        FFmpeg.SetExecutablesPath(defaultFFmpegPath);
    }

    /// <summary>
    /// Processa um único vídeo: converte em imagens e gera um arquivo ZIP.
    /// </summary>
    private async Task<string> ProcessarVideoAsync(IFormFile file)
    {
        // Valida o arquivo
        if (file == null || file.Length == 0)
            throw new ArgumentException("Arquivo inválido.");

        if (!file.FileName.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) &&
            !file.FileName.EndsWith(".avi", StringComparison.OrdinalIgnoreCase) &&
            !file.FileName.EndsWith(".mov", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Formato de vídeo inválido. Aceitamos apenas .mp4, .avi ou .mov.");
        }

        // Define um nome único para o vídeo
        var uniqueId = Guid.NewGuid().ToString();
        var videoPath = Path.Combine(_uploadPath, $"{uniqueId}{file.FileName}");

        // Salva o vídeo no servidor
        using (var stream = new FileStream(videoPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Define o diretório de saída para as imagens
        var outputFolder = Path.Combine(_outputPath, $"{Path.GetFileNameWithoutExtension(file.FileName)}_{uniqueId}");
        Directory.CreateDirectory(outputFolder);

        // Extrai imagens do vídeo
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

        // Cria o arquivo ZIP para as imagens
        var zipFilePath = Path.Combine(_outputPath, $"{Path.GetFileNameWithoutExtension(file.FileName)}_{uniqueId}.zip");
        ZipFile.CreateFromDirectory(outputFolder, zipFilePath);

        // Remove arquivos temporários
        Directory.Delete(outputFolder, true);
        File.Delete(videoPath);

        return zipFilePath;
    }

    /// <summary>
    /// Método para download dos executáveis do FFmpeg (mantém lógica existente)
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private async Task DownloadFFmpegExecutables()
    {
        // Caminho de destino na raiz do projeto
        var destinationPath = Path.Combine(Directory.GetCurrentDirectory(), "ffmpeg_executables");

        // URL oficial para baixar o FFmpeg (versão essencial)
        var ffmpegZipUrl = "https://www.gyan.dev/ffmpeg/builds/ffmpeg-release-essentials.zip";
        var tempZipPath = Path.Combine(Path.GetTempPath(), "ffmpeg.zip");
        var tempExtractPath = Path.Combine(Path.GetTempPath(), "ffmpeg_temp");

        // Garante que o diretório de destino está vazio
        if (Directory.Exists(destinationPath))
        {
            Directory.Delete(destinationPath, true);
        }
        Directory.CreateDirectory(destinationPath);

        // Faz o download do arquivo ZIP contendo os executáveis
        using (var httpClient = new HttpClient())
        using (var response = await httpClient.GetAsync(ffmpegZipUrl))
        {
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Não foi possível baixar os executáveis do FFmpeg.");
            }

            await using (var fs = new FileStream(tempZipPath, FileMode.Create, FileAccess.Write))
            {
                await response.Content.CopyToAsync(fs);
            }
        }

        // Extrai o conteúdo do ZIP para um diretório temporário
        ZipFile.ExtractToDirectory(tempZipPath, tempExtractPath, overwriteFiles: true);

        // Obtém o caminho da primeira subpasta dentro do diretório de extração
        var firstSubFolder = Directory.GetDirectories(tempExtractPath).FirstOrDefault();

        if (string.IsNullOrEmpty(firstSubFolder))
        {
            throw new Exception("Nenhuma subpasta encontrada no diretório extraído.");
        }

        // Caminho da pasta "bin" dentro da primeira subpasta
        var binPath = Path.Combine(firstSubFolder, "bin");

        if (!Directory.Exists(binPath))
        {
            throw new Exception("A pasta 'bin' não foi encontrada na subpasta extraída.");
        }

        // Copia apenas os executáveis da pasta "bin" para o destino final usando Streams
        foreach (var file in Directory.EnumerateFiles(binPath))
        {
            var fileName = Path.GetFileName(file);
            var destFile = Path.Combine(destinationPath, fileName);

            // Abre o arquivo de origem para leitura
            await using (var sourceStream = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                // Abre o arquivo de destino para escrita
                await using (var destinationStream = new FileStream(destFile, FileMode.Create, FileAccess.Write))
                {
                    await sourceStream.CopyToAsync(destinationStream);
                }
            }
        }

        // Remove os diretórios e arquivos temporários
        Directory.Delete(tempExtractPath, true);
        File.Delete(tempZipPath);
    }
}
