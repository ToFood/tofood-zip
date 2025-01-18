using MongoDB.Driver;
using ToFood.Domain.Entities.NonRelational;

namespace ToFood.Domain.DB.NonRelational.MongoDB;

/// <summary>
/// Contexto específico para logs no MongoDB.
/// </summary>
public class MongoContext : ToFoodNonRelationalContext
{
    /// <summary>
    /// Inicializa o contexto para MongoDB.
    /// </summary>
    /// <param name="connectionString">String de conexão.</param>
    /// <param name="databaseName">Nome do banco de dados.</param>
    public MongoContext(string connectionString, string databaseName)
        : base(connectionString, databaseName) { }


    /// <summary>
    /// Coleção de logs.
    /// </summary>
    public IMongoCollection<Log>? Logs { get; private set; }

    /// <summary>
    /// Configura as coleções do contexto.
    /// </summary>
    protected override void ConfigureCollections()
    {
        Logs = MongoDatabase.GetCollection<Log>("logs");
    }
}
