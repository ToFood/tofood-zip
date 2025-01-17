using Xabe.FFmpeg;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace ToFood.Domain.Services;

public class YoutubeService
{
    private readonly YoutubeClient _youtubeClient;
    private readonly string _videoOutputPath = Path.Combine("Output", "Videos"); // Diretório para armazenar os vídeos baixados

    public YoutubeService()
    {
        _youtubeClient = new YoutubeClient();

        // Configura o caminho para os executáveis do FFmpeg
        var ffmpegPath = Path.Combine(Directory.GetCurrentDirectory(), "ffmpeg_executables");
        if (!Directory.Exists(ffmpegPath) ||
            !File.Exists(Path.Combine(ffmpegPath, "ffmpeg.exe")) ||
            !File.Exists(Path.Combine(ffmpegPath, "ffprobe.exe")))
        {
            throw new Exception("Os executáveis do FFmpeg não estão configurados corretamente no diretório 'ffmpeg_executables'.");
        }

        FFmpeg.SetExecutablesPath(ffmpegPath);

        // Garante que o diretório de saída para vídeos existe
        Directory.CreateDirectory(_videoOutputPath);
    }
    
    /// <summary>
    /// Busca um video no Youtube pela URL e faz o download
    /// </summary>
    /// <param name="videoUrl"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="Exception"></exception>
    public async Task<string> DownloadYoutubeVideo(string videoUrl)
    {
        if (string.IsNullOrEmpty(videoUrl))
            throw new ArgumentException("A URL do vídeo não pode ser nula ou vazia.");

        var videoInfo = await _youtubeClient.Videos.GetAsync(videoUrl);
        var streamManifest = await _youtubeClient.Videos.Streams.GetManifestAsync(videoUrl);

        // Obter streams Muxed (Vídeo + Áudio)
        var muxedStreams = streamManifest
            .GetMuxedStreams()
            .Where(s => s.Container == Container.Mp4); // Verifica se o container é MP4 diretamente

        IStreamInfo selectedStream;

        Guid videoId = Guid.NewGuid();
        var videoDate = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

        if (muxedStreams.Any())
        {
            // Seleciona a melhor qualidade Muxed disponível
            selectedStream = muxedStreams.GetWithHighestVideoQuality();
        }
        else
        {
            // Fallback: Obter streams separadas de vídeo e áudio
            var videoOnlyStream = streamManifest.GetVideoOnlyStreams().GetWithHighestVideoQuality();
            var audioOnlyStream = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();

            if (videoOnlyStream == null || audioOnlyStream == null)
                throw new Exception("Nenhuma stream de vídeo ou áudio disponível para este vídeo.");           

            // Caminho para salvar arquivos temporários
            var tempVideoPath = Path.Combine(_videoOutputPath, $"video_{videoId}_{videoDate}.mp4");
            var tempAudioPath = Path.Combine(_videoOutputPath, $"audio_{videoId}_{videoDate}.mp4");
            var outputPath = Path.Combine(_videoOutputPath, $"{GetSafeFileName(videoInfo.Title)}.mp4");

            // Download de streams separadas
            await _youtubeClient.Videos.Streams.DownloadAsync(videoOnlyStream, tempVideoPath);
            await _youtubeClient.Videos.Streams.DownloadAsync(audioOnlyStream, tempAudioPath);

            // Mesclar vídeo e áudio usando FFmpeg
            await MergeVideoAndAudioAsync(tempVideoPath, tempAudioPath, outputPath);

            // Remover arquivos temporários
            File.Delete(tempVideoPath);
            File.Delete(tempAudioPath);

            return outputPath;
        }

        // Caminho para salvar o vídeo (caso Muxed esteja disponível)
        var videoOutputPath = Path.Combine(_videoOutputPath, $"{GetSafeFileName(videoInfo.Title)}.mp4");

        // Download da stream selecionada
        await _youtubeClient.Videos.Streams.DownloadAsync(selectedStream, videoOutputPath);

        return videoOutputPath;
    }

    /// <summary>
    /// Função para mesclar vídeo e áudio com FFmpeg
    /// </summary>
    /// <param name="videoPath"></param>
    /// <param name="audioPath"></param>
    /// <param name="outputPath"></param>
    /// <returns></returns>
    private async Task MergeVideoAndAudioAsync(string videoPath, string audioPath, string outputPath)
    {
        await FFmpeg.Conversions.New()
            .AddParameter($"-i \"{videoPath}\"") // Entrada de vídeo
            .AddParameter($"-i \"{audioPath}\"") // Entrada de áudio
            .AddParameter("-c:v copy -c:a aac")  // Codec de vídeo e áudio
            .SetOutput(outputPath)              // Saída
            .Start();
    }

    /// <summary>
    /// Função para limpar o nome do arquivo
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    private string GetSafeFileName(string fileName)
    {
        return string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));
    }
}
