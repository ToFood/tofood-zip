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
        Directory.CreateDirectory(_uploadPath);
        Directory.CreateDirectory(_outputPath);
    }

    /// <summary>
    /// Recebe um vídeo, converte-o em imagens e retorna um arquivo ZIP com as imagens extraídas
    /// </summary>
    /// <param name="file">Arquivo de vídeo enviado pelo usuário</param>
    /// <returns>Arquivo ZIP com as imagens extraídas do vídeo</returns>
    [HttpPost("convert-video-to-images")]
    public async Task<IActionResult> ConvertVideoToImageZip([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Nenhum arquivo enviado.");

        if (!file.FileName.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) &&
            !file.FileName.EndsWith(".avi", StringComparison.OrdinalIgnoreCase) &&
            !file.FileName.EndsWith(".mov", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("Por favor, envie um arquivo de vídeo válido (.mp4, .avi, .mov).");
        }

        var uniqueId = Guid.NewGuid().ToString();
        var videoPath = Path.Combine(_uploadPath, $"{uniqueId}_{file.FileName}");

        // Salva o vídeo no servidor
        using (var stream = new FileStream(videoPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Converte o vídeo em imagens
        var outputFolder = Path.Combine(_outputPath, $"{Path.GetFileNameWithoutExtension(file.FileName)}_{uniqueId}");
        Directory.CreateDirectory(outputFolder);

        FFmpeg.SetExecutablesPath("path_to_ffmpeg"); // Atualize o caminho do executável do FFmpeg
        var conversion = await FFmpeg.Conversions.FromSnippet.Snapshot(videoPath, Path.Combine(outputFolder, "frame%03d.png"), TimeSpan.FromSeconds(1));
        await conversion.Start();

        // Compacta as imagens em um arquivo ZIP
        var zipFilePath = Path.Combine(_outputPath, $"{Path.GetFileNameWithoutExtension(file.FileName)}_{uniqueId}.zip");
        ZipFile.CreateFromDirectory(outputFolder, zipFilePath);

        // Retorna o arquivo ZIP para download
        var fileBytes = await System.IO.File.ReadAllBytesAsync(zipFilePath);

        // Limpa arquivos temporários
        Directory.Delete(outputFolder, true); // Deleta as imagens geradas
        System.IO.File.Delete(videoPath); // Deleta o vídeo original

        return File(fileBytes, "application/zip", $"{Path.GetFileNameWithoutExtension(file.FileName)}.zip");
    }
}
