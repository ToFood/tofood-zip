using MongoDB.Driver;
using ToFood.Domain.Entities.NonRelational;

namespace ToFood.Domain.DB.NonRelational.MongoDB;

/// <summary>
/// Representa o contexto do MongoDB para a aplicação ToFood.
/// Permite acesso e configuração de coleções no banco de dados MongoDB.
/// </summary>
public class MongoDBContext : ToFoodNonRelationalContext
{
    /// <summary>
    /// Inicializa uma nova instância de <see cref="MongoDBContext"/> com a string de conexão e o nome do banco de dados especificados.
    /// </summary>
    /// <param name="connectionString">A string de conexão com o MongoDB.</param>
    /// <param name="databaseName">O nome do banco de dados.</param>
    public MongoDBContext(string connectionString, string databaseName)
        : base(connectionString, databaseName) { }

    /// <summary>
    /// Representa a coleção de logs no banco de dados MongoDB.
    /// </summary>
    public IMongoCollection<Log>? Logs { get; private set; }

    /// <summary>
    /// Configura as coleções do banco de dados, vinculando a classe Log à coleção "logs".
    /// </summary>
    protected override void ConfigureCollections()
    {
        Logs = MongoDatabase.GetCollection<Log>("logs");
    }
}
