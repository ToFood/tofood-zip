using Microsoft.AspNetCore.Mvc;
using System.IO.Compression;
using Xabe.FFmpeg;

namespace ToFood.ZipAPI.Controllers;

[ApiController]
[Route("zip")]
public class ZipController : ControllerBase
{
    private readonly string _uploadPath = "Uploads"; // Diretório para armazenar os vídeos enviados
    private readonly string _outputPath = "Output"; // Diretório para armazenar as saídas geradas

    public ZipController()
    {
        // Garantir que os diretórios de upload e saída existam
        Directory.CreateDirectory(_uploadPath);
        Directory.CreateDirectory(_outputPath);
    }

    /// <summary>
    /// Recebe uma lista de vídeos, converte cada um em imagens e retorna um arquivo ZIP consolidado.
    /// </summary>
    /// <param name="files">Lista de arquivos de vídeo enviados pelo usuário.</param>
    /// <returns>Um arquivo ZIP contendo as imagens extraídas de todos os vídeos.</returns>
    [HttpPost("convert-videos-to-images")]
    public async Task<IActionResult> ConvertVideosToImageZip([FromForm] List<IFormFile> files)
    {
        // Verifica se há arquivos enviados
        if (files == null || files.Count == 0)
            return BadRequest("Nenhum arquivo foi enviado.");

        var tasks = new List<Task<string>>(); // Lista de tarefas para processar cada vídeo
        var processedZips = new List<string>(); // Lista para armazenar os caminhos dos ZIPs gerados

        // Diretório padrão para o FFmpeg
        var defaultFFmpegPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg_executables");

        // Garante que os executáveis do FFmpeg estão configurados
        if (!Directory.Exists(defaultFFmpegPath) || !System.IO.File.Exists(Path.Combine(defaultFFmpegPath, "ffmpeg.exe")) ||
            !System.IO.File.Exists(Path.Combine(defaultFFmpegPath, "ffprobe.exe")))
        {
            try
            {
                // Cria o diretório e baixa os executáveis, se necessário
                Directory.CreateDirectory(defaultFFmpegPath);
                await DownloadFFmpegExecutables(defaultFFmpegPath);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao configurar os executáveis do FFmpeg: {ex.Message}");
            }
        }

        // Configura o FFmpeg para uso
        FFmpeg.SetExecutablesPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg_executables"));

        foreach (var file in files)
        {
            tasks.Add(ProcessarVideoAsync(file)); // Adiciona o processamento de cada vídeo como uma tarefa
        }

        try
        {
            // Processa todos os vídeos de forma assíncrona
            processedZips = (await Task.WhenAll(tasks)).ToList();
        }
        catch (Exception ex)
        {
            return BadRequest($"Erro ao processar os vídeos: {ex.Message}");
        }

        // Cria um ZIP consolidado contendo todos os ZIPs individuais
        var consolidatedZipPath = Path.Combine(_outputPath, $"Consolidado{Guid.NewGuid()}.zip");
        using (var zipStream = new FileStream(consolidatedZipPath, FileMode.Create))
        {
            using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create))
            {
                foreach (var zipPath in processedZips)
                {
                    var entryName = Path.GetFileName(zipPath); // Nome do arquivo no ZIP
                    archive.CreateEntryFromFile(zipPath, entryName); // Adiciona o ZIP individual ao ZIP consolidado
                }
            }
        }

        // Lê o arquivo consolidado para retornar ao cliente
        var fileBytes = await System.IO.File.ReadAllBytesAsync(consolidatedZipPath);

        // Remove os arquivos ZIP temporários
        foreach (var zipPath in processedZips)
        {
            System.IO.File.Delete(zipPath);
        }

        return new FileContentResult(fileBytes, "application/zip")
        {
            FileDownloadName = "VideosProcessados.zip"
        };
    }

    /// <summary>
    /// Processa um único vídeo: converte em imagens e gera um arquivo ZIP.
    /// </summary>
    /// <param name="file">Arquivo de vídeo enviado.</param>
    /// <returns>O caminho do arquivo ZIP gerado.</returns>
    private async Task<string> ProcessarVideoAsync(IFormFile file)
    {
        // Valida o arquivo recebido
        if (file == null || file.Length == 0)
            throw new ArgumentException("Arquivo inválido.");

        if (!file.FileName.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) &&
            !file.FileName.EndsWith(".avi", StringComparison.OrdinalIgnoreCase) &&
            !file.FileName.EndsWith(".mov", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Formato de vídeo inválido. Aceitamos apenas .mp4, .avi ou .mov.");
        }

        // Define um nome único para o arquivo
        var uniqueId = Guid.NewGuid().ToString();
        var videoPath = Path.Combine(_outputPath, $"{uniqueId}{file.FileName}");

        // Salva o vídeo no servidor
        using (var stream = new FileStream(videoPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Define o diretório de saída para as imagens extraídas
        var outputFolder = Path.Combine(_outputPath, $"{Path.GetFileNameWithoutExtension(file.FileName)}{uniqueId}");
        Directory.CreateDirectory(outputFolder);



        // Extrai imagens do vídeo
        var mediaInfo = await FFmpeg.GetMediaInfo(videoPath);
        var videoDuration = mediaInfo.VideoStreams.First().Duration.TotalSeconds; // Duração total do vídeo
        var snapshotTasks = new List<Task>(); // Tarefas de captura de frames

        for (int i = 0; i < videoDuration; i++)
        {
            var outputImagePath = Path.Combine(outputFolder, $"frame_{i:D3}.png");
            var conversion = await FFmpeg.Conversions.FromSnippet.Snapshot(videoPath, outputImagePath, TimeSpan.FromSeconds(i));
            snapshotTasks.Add(conversion.Start());
        }

        // Aguarda todas as capturas de frames serem concluídas
        await Task.WhenAll(snapshotTasks);

        // Cria um arquivo ZIP para as imagens extraídas
        var zipFilePath = Path.Combine(_outputPath, $"{Path.GetFileNameWithoutExtension(file.FileName)}{uniqueId}.zip");
        ZipFile.CreateFromDirectory(outputFolder, zipFilePath);

        // Remove arquivos temporários
        Directory.Delete(outputFolder, true); // Remove o diretório com as imagens
        System.IO.File.Delete(videoPath); // Remove o vídeo original

        return zipFilePath; // Retorna o caminho do ZIP gerado
    }

    /// <summary>
    /// Faz o download e extrai os executáveis do FFmpeg se não estiverem disponíveis localmente.
    /// </summary>
    /// <param name="destinationPath">Caminho onde os executáveis serão extraídos</param>
    private async Task DownloadFFmpegExecutables(string destinationPath)
    {
        // URL oficial para baixar o FFmpeg (versão essencial)
        var ffmpegZipUrl = "https://www.gyan.dev/ffmpeg/builds/ffmpeg-release-essentials.zip";
        var tempZipPath = Path.Combine(Path.GetTempPath(), "ffmpeg.zip");
        var tempExtractPath = Path.Combine(Path.GetTempPath(), "ffmpeg_temp");

        // Garante que o diretório de destino está vazio
        //if (Directory.Exists(destinationPath))
        //{
        //    Directory.Delete(destinationPath, true);
        //}
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
        //File.Delete(tempZipPath);
    }
}