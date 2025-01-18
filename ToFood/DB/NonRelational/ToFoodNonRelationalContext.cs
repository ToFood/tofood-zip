using MongoDB.Driver;

public abstract class ToFoodNonRelationalContext
{
    /// <summary>
    /// Banco de dados MongoDB.
    /// </summary>
    protected IMongoDatabase MongoDatabase { get; }

    /// <summary>
    /// Inicializa o contexto para o banco não-relacional.
    /// </summary>
    /// <param name="connectionString">String de conexão.</param>
    /// <param name="databaseName">Nome do banco de dados.</param>
    protected ToFoodNonRelationalContext(string connectionString, string databaseName)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentNullException(nameof(connectionString), "A string de conexão não pode ser nula ou vazia.");

        if (string.IsNullOrWhiteSpace(databaseName))
            throw new ArgumentNullException(nameof(databaseName), "O nome do banco não pode ser nulo ou vazio.");

        var client = new MongoClient(connectionString);
        MongoDatabase = client.GetDatabase(databaseName);

        ConfigureCollections();
    }

    /// <summary>
    /// Método abstrato para configurar coleções no contexto.
    /// </summary>
    protected abstract void ConfigureCollections();
}
