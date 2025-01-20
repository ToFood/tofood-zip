using ToFood.Domain.Enums;

namespace ToFood.Domain.DTOs.Response;

/// <summary>
/// Representa um vídeo na listagem.
/// </summary>
public class VideoResponse
{
    /// <summary>
    /// Identificador do vídeo.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Nome do arquivo do vídeo.
    /// </summary>
    public string? FileName { get; set; }

    /// <summary>
    /// Data de criação do vídeo.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Status do vídeo.
    /// </summary>
    public string? Status { get; set; }
}
