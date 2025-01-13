using Microsoft.AspNetCore.Mvc;
using System.IO.Compression;
using Xabe.FFmpeg;

namespace ToFood.ZipAPI.Controllers;

[ApiController]
[Route("zip")]
public class ZipController : ControllerBase
{
    private readonly string _uploadPath = "Uploads";
    private readonly string _outputPath = "Output";

    public ZipController()
    {
        // Garante que os diretórios de uploads e output existam
        Directory.CreateDirectory(_uploadPath);
        Directory.CreateDirectory(_outputPath);
    }

    /// <summary>
    /// Recebe um vídeo, converte-o em imagens e retorna um arquivo ZIP com as imagens extraídas.
    /// </summary>
    /// <param name="file">Arquivo de vídeo enviado pelo usuário</param>
    /// <returns>Arquivo ZIP com as imagens extraídas do vídeo</returns>
    [HttpPost("convert-video-to-images")]
    public async Task<IActionResult> ConvertVideoToImageZip([FromForm] IFormFile file)
    {
        // Verifica se um arquivo foi enviado
        if (file == null || file.Length == 0)
            return BadRequest("Nenhum arquivo enviado.");

        // Verifica se o arquivo tem uma extensão válida
        if (!file.FileName.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) &&
            !file.FileName.EndsWith(".avi", StringComparison.OrdinalIgnoreCase) &&
            !file.FileName.EndsWith(".mov", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("Por favor, envie um arquivo de vídeo válido (.mp4, .avi, .mov).");
        }

        // Define um identificador único para o arquivo
        var uniqueId = Guid.NewGuid().ToString();
        var videoPath = Path.Combine(_uploadPath, $"{uniqueId}_{file.FileName}");

        // Salva o vídeo no servidor
        using (var stream = new FileStream(videoPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Diretório de saída para as imagens extraídas
        var outputFolder = Path.Combine(_outputPath, $"{Path.GetFileNameWithoutExtension(file.FileName)}_{uniqueId}");
        Directory.CreateDirectory(outputFolder);

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
                return BadRequest($"Erro ao configurar os executáveis do FFmpeg: {ex.Message}");
            }
        }

        // Configura o FFmpeg
        FFmpeg.SetExecutablesPath(defaultFFmpegPath);

        try
        {
            // Extrai o número total de segundos do vídeo para gerar múltiplas imagens
            var mediaInfo = await FFmpeg.GetMediaInfo(videoPath);
            var videoDuration = mediaInfo.VideoStreams.First().Duration.TotalSeconds;

            // Lista para armazenar todas as tarefas de conversão
            var snapshotTasks = new List<Task>();

            // Cria capturas de tela a cada segundo
            for (int i = 0; i < videoDuration; i++)
            {
                var outputImagePath = Path.Combine(outputFolder, $"frame_{i:D3}.png");

                // Obtém a conversão (sem iniciar)
                var conversion = await FFmpeg.Conversions.FromSnippet.Snapshot(videoPath, outputImagePath, TimeSpan.FromSeconds(i));

                // Adiciona a tarefa de conversão à lista
                snapshotTasks.Add(conversion.Start());
            }

            // Aguarda todas as conversões terminarem
            await Task.WhenAll(snapshotTasks);
        }
        catch (Exception ex)
        {
            return BadRequest($"Erro durante a conversão: {ex.Message}");
        }


        // Compacta as imagens em um arquivo ZIP
        var zipFilePath = Path.Combine(_outputPath, $"{Path.GetFileNameWithoutExtension(file.FileName)}_{uniqueId}.zip");
        ZipFile.CreateFromDirectory(outputFolder, zipFilePath);

        // Lê o conteúdo do ZIP para retornar ao cliente
        var fileBytes = await System.IO.File.ReadAllBytesAsync(zipFilePath);

        // Verifica se o ZIP foi criado corretamente
        if (fileBytes == null || fileBytes.Length == 0)
        {
            return BadRequest("Erro ao gerar o arquivo ZIP.");
        }

        // Limpa arquivos temporários
        Directory.Delete(outputFolder, true); // Remove imagens geradas
        System.IO.File.Delete(videoPath); // Remove o vídeo original

        // Retorna o arquivo ZIP para o cliente
        return new FileContentResult(fileBytes, "application/zip")
        {
            FileDownloadName = $"{Path.GetFileNameWithoutExtension(file.FileName)+".zip"}"
        };
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
        //File.Delete(tempZipPath);
    }

}
