﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Logging;
using ToFood.Domain.Services;

namespace ToFood.Domain.Extensions;

/// <summary>
/// Métodos de extensão para configurar a injeção de dependência no domínio.
/// </summary>
public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Configura os serviços do domínio no contêiner de DI.
    /// </summary>
    public static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        services.AddScoped<AuthService>();
        services.AddScoped<UserService>();
        services.AddScoped<ZipService>();
        services.AddScoped<YoutubeService>();
        services.AddScoped<LogHelper>();

        // Adicione outros serviços do domínio aqui, se necessário.

        return services;
    }
}