namespace ToFood.VideoProcessor.Models;

public class VideoMessage
{
    public string? VideoId { get; set; } // Identificador único do vídeo
    public string? FilePath { get; set; } // Caminho do vídeo para processamento
}
