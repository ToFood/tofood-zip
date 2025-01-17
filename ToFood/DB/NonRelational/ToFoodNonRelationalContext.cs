using MongoDB.Driver;
using ToFood.Domain.Entities.NonRelational;

namespace ToFood.Domain.DB.NonRelational;

/// <summary>
/// Contexto genérico para bancos não-relacionais (MongoDB).
/// </summary>
public class ToFoodNonRelationalContext
{
    private readonly IMongoDatabase _database;

    /// <summary>
    /// Inicializa o contexto para o banco não-relacional (MongoDB).
    /// </summary>
    /// <param name="connectionString">String de conexão com o MongoDB.</param>
    /// <param name="databaseName">Nome do banco de dados no MongoDB.</param>
    public ToFoodNonRelationalContext(string connectionString, string databaseName)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentNullException(nameof(connectionString), "A string de conexão com o MongoDB não pode ser nula ou vazia.");

        if (string.IsNullOrWhiteSpace(databaseName))
            throw new ArgumentNullException(nameof(databaseName), "O nome do banco de dados não pode ser nulo ou vazio.");

        var client = new MongoClient(connectionString);
        _database = client.GetDatabase(databaseName);
    }

    /// <summary>
    /// Retorna a coleção de logs.
    /// </summary>
    public IMongoCollection<Log> Logs => _database.GetCollection<Log>("logs");

    // Adicione outras coleções conforme necessário:
    // public IMongoCollection<AnotherEntity> AnotherCollection => _database.GetCollection<AnotherEntity>("another_collection");
}
