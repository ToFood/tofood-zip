using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ToFood.Domain.Entities.NonRelational;

/// <summary>
/// Representa um log armazenado no MongoDB.
/// </summary>
public class Log
{
    /// <summary>
    /// Identificador único do log.
    /// </summary>
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }

    /// <summary>
    /// Mensagem do log.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Nível do log (ex.: Information, Warning, Error, Debug).
    /// </summary>
    public string? Level { get; set; }

    /// <summary>
    /// Data e hora do log (UTC).
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Nome do serviço ou componente que gerou o log.
    /// </summary>
    public string? ServiceName { get; set; }

    /// <summary>
    /// Nome do método ou operação que gerou o log.
    /// </summary>
    public string? OperationName { get; set; }

    /// <summary>
    /// Identificador único da requisição (para rastreamento).
    /// </summary>
    public string? RequestId { get; set; }

    /// <summary>
    /// Dados da requisição associados ao log.
    /// </summary>
    public RequestLog? Request { get; set; }

    /// <summary>
    /// Dados da resposta associados ao log.
    /// </summary>
    public ResponseLog? Response { get; set; }

    /// <summary>
    /// Informações adicionais ou metadados.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Representa os detalhes de uma requisição para fins de log.
/// </summary>
public class RequestLog
{
    /// <summary>
    /// URL da requisição.
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// Método HTTP usado (GET, POST, PUT, DELETE, etc.).
    /// </summary>
    public string? Method { get; set; }

    /// <summary>
    /// Cabeçalhos da requisição.
    /// </summary>
    public Dictionary<string, string>? Headers { get; set; }

    /// <summary>
    /// Corpo da requisição (se aplicável).
    /// </summary>
    public string? Body { get; set; }
}

/// <summary>
/// Representa os detalhes de uma resposta para fins de log.
/// </summary>
public class ResponseLog
{
    /// <summary>
    /// Código de status HTTP retornado (ex.: 200, 404, 500).
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Cabeçalhos da resposta.
    /// </summary>
    public Dictionary<string, string>? Headers { get; set; }

    /// <summary>
    /// Corpo da resposta (se aplicável).
    /// </summary>
    public string? Body { get; set; }

    /// <summary>
    /// Tempo total de processamento da requisição (em milissegundos).
    /// </summary>
    public long ProcessingTimeMs { get; set; }
}