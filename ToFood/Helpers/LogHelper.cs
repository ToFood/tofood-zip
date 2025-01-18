using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using ToFood.Domain.Entities.NonRelational;

namespace ToFood.Domain.Helpers
{
    public class MongoDbLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly IMongoCollection<Log> _logCollection;

        public MongoDbLogger(string categoryName, IMongoCollection<Log> logCollection)
        {
            _categoryName = categoryName;
            _logCollection = logCollection;
        }

        public IDisposable? BeginScope<TState>(TState state) => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;

            var logEntry = new Log
            {
                Level = logLevel.ToString(),
                Message = formatter(state, exception),
                Timestamp = DateTime.UtcNow,
                ServiceName = _categoryName,
                Metadata = new Dictionary<string, object>
                {
                    { "EventId", eventId.Id },
                    { "EventName", eventId.Name }
                },
                Exception = exception?.ToString()
            };

            _logCollection.InsertOne(logEntry);
        }
    }
}
