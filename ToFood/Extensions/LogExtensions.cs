﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using ToFood.Domain.Entities.NonRelational;
using ToFood.Domain.Helpers;

namespace ToFood.Domain.Extensions;

/// <summary>
/// Implementação de um logger personalizado que grava logs no MongoDB.
/// </summary>
public class MongoDbLogger : ILogger
{
    private readonly string _categoryName; // Categoria do logger (geralmente o nome do serviço/classe).
    private readonly IMongoCollection<Log> _logCollection; // Coleção MongoDB onde os logs serão armazenados.
    private readonly IHttpContextAccessor? _httpContextAccessor; // Dependência opcional para acessar o contexto HTTP.

    /// <summary>
    /// Construtor para inicializar o logger.
    /// </summary>
    /// <param name="categoryName">Nome da categoria do logger.</param>
    /// <param name="logCollection">Coleção do MongoDB onde os logs serão armazenados.</param>
    /// <param name="httpContextAccessor">Acessor para o contexto HTTP (opcional).</param>
    public MongoDbLogger(string categoryName, IMongoCollection<Log> logCollection, IHttpContextAccessor? httpContextAccessor = null)
    {
        _categoryName = categoryName; // Inicializa a categoria.
        _logCollection = logCollection; // Inicializa a coleção de logs.
        _httpContextAccessor = httpContextAccessor; // Inicializa o contexto HTTP.
    }

    /// <summary>
    /// Implementação do método BeginScope, usada para criar escopos de logging.
    /// Retorna null porque esta implementação não utiliza escopos.
    /// </summary>
    /// <typeparam name="TState">Tipo do estado do escopo.</typeparam>
    /// <param name="state">Estado do escopo.</param>
    /// <returns>Retorna null, indicando que escopos não são suportados.</returns>
    public IDisposable? BeginScope<TState>(TState state) => null;

    /// <summary>
    /// Verifica se o nível de log está habilitado.
    /// </summary>
    public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

    /// <summary>
    /// Registra um log na coleção do MongoDB.
    /// </summary>
    /// <typeparam name="TState">Tipo do estado associado ao log.</typeparam>
    /// <param name="logLevel">Nível do log.</param>
    /// <param name="eventId">Identificador do evento associado ao log.</param>
    /// <param name="state">Estado do log, que contém informações adicionais.</param>
    /// <param name="exception">Exceção associada ao log (se houver).</param>
    /// <param name="formatter">Função que formata o estado e a exceção em uma mensagem de log.</param>
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;

        // Obtenha o fuso horário local
        var localTimeZone = TimeZoneInfo.Local;

        // Cria um objeto de log enriquecido com informações do contexto HTTP.
        var logEntry = new Log
        {
            Level = logLevel.ToString(),
            Message = formatter(state, exception),
            Timestamp = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, localTimeZone), // Converte UTC para o horário local
            ServiceName = _categoryName,
            ControllerName = GetControllerName(state),
            MethodName = GetMethodName(state),
            RequestId = GetRequestId(),
            UserId = JWTHelper.GetAuthenticatedUserId(_httpContextAccessor).ToString() ?? null,
            Request = GetRequestContext(),
            Response = GetResponseContext(),
            Exception = exception?.ToString()
        };

        // Insere o log no MongoDB.
        _logCollection.InsertOne(logEntry);
    }

    /// <summary>
    /// Recupera o nome da operação atual Controlador, se disponível no contexto HTTP.
    /// </summary>
    private string? GetControllerName<TState>(TState state)
    {
        var routeData = _httpContextAccessor?.HttpContext?.GetRouteData();
        if (routeData != null)
        {
            var controller = routeData.Values["controller"]?.ToString();

            if (!string.IsNullOrEmpty(controller))
            {
                return $"{controller}Controller";
            }
        }

        // Se não for possível obter o nome do controlador/ação, retorna o estado (se for string)
        return state is string operationName ? operationName : null;
    }

    /// <summary>
    /// Recupera o nome da operação atual Método/Ação, se disponível no contexto HTTP.
    /// </summary>
    private string? GetMethodName<TState>(TState state)
    {
        var routeData = _httpContextAccessor?.HttpContext?.GetRouteData();
        if (routeData != null)
        {
            var action = routeData.Values["action"]?.ToString();

            if (!string.IsNullOrEmpty(action))
            {
                return $"{action}";
            }
        }

        // Se não for possível obter o nome do controlador/ação, retorna o estado (se for string)
        return state is string operationName ? operationName : null;
    }


    /// <summary>
    /// Recupera o ID único da requisição (se disponível no contexto HTTP).
    /// </summary>
    private string? GetRequestId()
    {
        return _httpContextAccessor?.HttpContext?.TraceIdentifier;
    }

    /// <summary>
    /// Recupera informações da requisição HTTP (incluindo o corpo, se disponível).
    /// </summary>
    private RequestLog? GetRequestContext()
    {
        var request = _httpContextAccessor?.HttpContext?.Request;
        if (request == null) return null;

        // Lê o corpo da requisição
        var requestBody = ""; //GetRequestBodyAsync(request).Result; // Comentei aqui pq o Body fica muito grande por causa dos Arquivos

        return new RequestLog
        {
            Url = request.Path.ToString(),
            Method = request.Method,
            Headers = request.Headers.ToDictionary(h => h.Key, h => string.Join(", ", new {h.Value })),
            Body = requestBody
        };
    }


    /// <summary>
    /// Pega o Body da requisição
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    private async Task<string?> GetRequestBodyAsync(HttpRequest request)
    {
        if (request.Body == null || !request.Body.CanRead)
            return null;

        // Permite que o stream da requisição seja lido novamente
        request.EnableBuffering();

        // Lê o corpo da requisição
        using (var reader = new StreamReader(request.Body, leaveOpen: true))
        {
            var body = await reader.ReadToEndAsync();
            request.Body.Position = 0; // Reseta a posição do stream para permitir que outros componentes leiam o corpo
            return body;
        }
    }



    /// <summary>
    /// Recupera informações da resposta HTTP (se disponível).
    /// </summary>
    private ResponseLog? GetResponseContext()
    {
        var response = _httpContextAccessor?.HttpContext?.Response;
        if (response == null) return null;

        return new ResponseLog
        {
            StatusCode = response.StatusCode,
            Headers = response.Headers.ToDictionary(h => h.Key, h => string.Join(", ", new {h.Value })),
            Body = null, // Corpo da resposta pode ser incluído se necessário.
            ProcessingTimeMs = 0 // Este campo pode ser calculado com middleware ou lógica personalizada.
        };
    }
}
