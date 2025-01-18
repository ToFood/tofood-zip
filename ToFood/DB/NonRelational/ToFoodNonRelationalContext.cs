using MongoDB.Driver;

namespace ToFood.Domain.DB.NonRelational;

/// <summary>
/// Contexto genérico para bancos não-relacionais.
/// </summary>
public abstract class ToFoodNonRelationalContext
{
    protected readonly IMongoDatabase MongoDatabase;

    /// <summary>
    /// Inicializa o contexto para o banco não-relacional.
    /// </summary>
    /// <param name="connectionString">String de conexão.</param>
    /// <param name="databaseName">Nome do banco de dados.</param>
    protected ToFoodNonRelationalContext(string connectionString, string databaseName)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentNullException(nameof(connectionString), $"A string de conexão com o {databaseName} não pode ser nula ou vazia.");

        if (string.IsNullOrWhiteSpace(databaseName))
            throw new ArgumentNullException(nameof(databaseName), "O nome do banco de dados não pode ser nulo ou vazio.");

        var client = new MongoClient(connectionString);
        MongoDatabase = client.GetDatabase(databaseName);

        ConfigureCollections();
    }

    /// <summary>
    /// Método abstrato para configurar as coleções.
    /// </summary>
    protected abstract void ConfigureCollections();
}
