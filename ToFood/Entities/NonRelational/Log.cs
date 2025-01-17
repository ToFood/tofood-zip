using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace ToFood.Domain.Entities.NonRelational;

/// <summary>
/// Representa um log armazenado no MongoDB.
/// </summary>
public class Log
{
    /// <summary>
    /// Identificador único do log (gerado automaticamente pelo MongoDB).
    /// </summary>
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    /// <summary>
    /// Nível do log (ex.: Info, Error, Debug).
    /// </summary>
    [BsonElement("level")]
    public string? Level { get; set; }

    /// <summary>
    /// Mensagem do log.
    /// </summary>
    [BsonElement("message")]
    public string? Message { get; set; }

    /// <summary>
    /// Data e hora em que o log foi criado.
    /// </summary>
    [BsonElement("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
