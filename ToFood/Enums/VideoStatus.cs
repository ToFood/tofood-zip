using System.ComponentModel;

namespace ToFood.Domain.Enums;

/// <summary>
/// Enum que representa o status do processamento de um vídeo.
/// </summary>
public enum VideoStatus
{
    [Description("Aguardando processamento")]
    Pending = 1,

    [Description("Em processamento")]
    Processing = 2,

    [Description("Processado com sucesso")]
    Completed = 3,

    [Description("Falha no processamento")]
    Failed = 4
}
