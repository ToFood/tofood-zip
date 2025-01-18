using MongoDB.Driver;
using ToFood.Domain.Entities.NonRelational;

namespace ToFood.Domain.DB.NonRelational.MongoDB;

public class MongoContext : ToFoodNonRelationalContext
{
    public MongoContext(string connectionString, string databaseName)
        : base(connectionString, databaseName) { }

    public IMongoCollection<Log>? Logs { get; private set; }

    protected override void ConfigureCollections()
    {
        Logs = MongoDatabase.GetCollection<Log>("logs");
    }
}
