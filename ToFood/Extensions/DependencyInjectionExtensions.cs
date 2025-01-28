using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Logging;
using ToFood.Domain.DB.Relational;
using ToFood.Domain.Interfaces;
using ToFood.Domain.Services;
using ToFood.Domain.Services.Notifications;
using ToFood.Domain.Services.TokenManager;

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
        services.AddScoped<VideoService>();
        services.AddScoped<YoutubeService>();
        services.AddScoped<LogHelper>();
        services.AddScoped<EmailService>();
        services.AddScoped<NotificationService>();

        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<AWSTokenManager>();


        return services;
    }
}
