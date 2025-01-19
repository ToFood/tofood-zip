using System.ComponentModel;

namespace ToFood.Domain.Enums;

/// <summary>
/// Enum que representa o status do processamento de um arquivo ZIP.
/// </summary>
public enum ZipStatus
{
    [Description("Aguardando processamento")]
    Pending = 1,

    [Description("Processado com sucesso")]
    Completed = 2,

    [Description("Falha no processamento")]
    Failed = 3
}
