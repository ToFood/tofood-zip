using System.IO.Compression;

namespace ToFood.Domain.Utils;

/// <summary>
/// Classe utilitária para gerenciar o FFmpeg, incluindo download e configuração.
/// </summary>
internal static class FFmpegUtils
{
    /// <summary>
    /// Garante que o FFmpeg esteja configurado e os executáveis estejam disponíveis.
    /// </summary>
    public static async Task EnsureFFmpegIsConfigured()
    {
        var defaultFFmpegPath = Path.Combine(Directory.GetCurrentDirectory(), "ffmpeg_executables");
        if (!Directory.Exists(defaultFFmpegPath) || !File.Exists(Path.Combine(defaultFFmpegPath, "ffmpeg.exe")) ||
            !File.Exists(Path.Combine(defaultFFmpegPath, "ffprobe.exe")))
        {
            await DownloadFFmpegExecutables();
        }
        Xabe.FFmpeg.FFmpeg.SetExecutablesPath(defaultFFmpegPath);
    }

    /// <summary>
    /// Faz o download e instalação dos executáveis do FFmpeg, se necessário.
    /// </summary>
    private static async Task DownloadFFmpegExecutables()
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
