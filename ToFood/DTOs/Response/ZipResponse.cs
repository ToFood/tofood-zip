namespace ToFood.Domain.DTOs.Response;

/// <summary>
/// Representa um Zip na listagem.
/// </summary>
public class ZipResponse
{
    /// <summary>
    /// Identificador do Zip.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Nome do arquivo do Zip.
    /// </summary>
    public string? FileName { get; set; }

    /// <summary>
    /// Data de criação do Zip.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Status do Zip.
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Extensão do Zip.
    /// </summary>
    public string? Extension { get; set; } = "zip";
}
