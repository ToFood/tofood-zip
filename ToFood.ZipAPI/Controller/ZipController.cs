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
                await DownloadFFmpegExecutablesAsync(defaultFFmpegPath);
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
            // Converte o vídeo em imagens
            var conversion = await FFmpeg.Conversions.FromSnippet.Snapshot(
                videoPath,
                Path.Combine(outputFolder, "frame%03d.png"),
                TimeSpan.FromSeconds(1) // Extrai uma imagem a cada 1 segundo
            );
            await conversion.Start();
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
            FileDownloadName = $"{Path.GetFileNameWithoutExtension(file.FileName)}.zip"
        };
    }

    /// <summary>
    /// Faz o download e extrai os executáveis do FFmpeg se não estiverem disponíveis localmente.
    /// </summary>
    /// <param name="destinationPath">Caminho onde os executáveis serão extraídos</param>
    private async Task DownloadFFmpegExecutablesAsync(string destinationPath)
    {
        // URL oficial para baixar o FFmpeg (versão essencial)
        var ffmpegZipUrl = "https://www.gyan.dev/ffmpeg/builds/ffmpeg-release-essentials.zip";
        var tempZipPath = Path.Combine(Path.GetTempPath(), "ffmpeg.zip");

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

        // Extrai os executáveis para o destino especificado
        ZipFile.ExtractToDirectory(tempZipPath, destinationPath, overwriteFiles: true);

        // Remove o arquivo ZIP temporário
        System.IO.File.Delete(tempZipPath);
    }
}
