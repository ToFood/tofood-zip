using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using ToFood.Domain.Entities.NonRelational;

namespace ToFood.Domain.Helpers
{
    public class MongoDbLoggerProvider : ILoggerProvider
    {
        private readonly IMongoCollection<Log> _logCollection;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MongoDbLoggerProvider(IMongoCollection<Log> logCollection, IHttpContextAccessor httpContextAccessor)
        {
            _logCollection = logCollection;
            _httpContextAccessor = httpContextAccessor;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new MongoDbLogger(categoryName, _logCollection, _httpContextAccessor);
        }

        public void Dispose()
        {
            // Dispose de recursos, se necessário
        }
    }
}
