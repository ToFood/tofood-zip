using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using ToFood.Domain.Entities.NonRelational;

namespace ToFood.Domain.Helpers
{
    /// <summary>
    /// Provedor de log personalizado que envia logs para o MongoDB.
    /// </summary>
    public class MongoDbLoggerProvider : ILoggerProvider
    {
        private readonly IMongoCollection<Log> _logCollection;
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Inicializa uma nova instância do provedor de log do MongoDB.
        /// </summary>
        /// <param name="logCollection">A coleção do MongoDB onde os logs serão armazenados.</param>
        /// <param name="httpContextAccessor">Acessor para o contexto HTTP atual, usado para capturar informações do usuário ou requisição.</param>
        public MongoDbLoggerProvider(IMongoCollection<Log> logCollection, IHttpContextAccessor httpContextAccessor)
        {
            _logCollection = logCollection;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Cria uma instância do logger associada a uma categoria específica.
        /// </summary>
        /// <param name="categoryName">O nome da categoria de log (geralmente o nome da classe que está gerando logs).</param>
        /// <returns>Uma instância de <see cref="ILogger"/> configurada para gravar no MongoDB.</returns>
        public ILogger CreateLogger(string categoryName)
        {
            return new MongoDbLogger(categoryName, _logCollection, _httpContextAccessor);
        }

        /// <summary>
        /// Libera os recursos associados ao provedor de log.
        /// </summary>
        public void Dispose()
        {
            // Dispose de recursos, se necessário
        }
    }
}
