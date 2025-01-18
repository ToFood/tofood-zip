using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using ToFood.Domain.Entities.NonRelational;

namespace ToFood.Domain.Helpers
{
    public class MongoDbLoggerProvider : ILoggerProvider
    {
        private readonly IMongoCollection<Log> _logCollection;

        public MongoDbLoggerProvider(IMongoCollection<Log> logCollection)
        {
            _logCollection = logCollection;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new MongoDbLogger(categoryName, _logCollection);
        }

        public void Dispose()
        {
            // Dispose de recursos, se necessário
        }
    }
}
