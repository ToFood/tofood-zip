using System;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace ToFood.Domain.Helpers;

public static class JWTHelper
{
    /// <summary>
    /// Obtém o ID do usuário autenticado.
    /// </summary>
    public static Guid GetAuthenticatedUserId(IHttpContextAccessor httpContextAccessor)
    {
        if (httpContextAccessor == null)
        {
            throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        var user = httpContextAccessor.HttpContext?.User;
        if (user == null)
        {
            return default;
        }

        // Busca o claim "nameid" que contém o ID do usuário
        var userIdClaim = user.Claims.FirstOrDefault(c => c.Type == "userId");
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return default;
        }

        return userId;
    }

    /// <summary>
    /// Obtém o email do usuário autenticado.
    /// </summary>
    /// <returns>O email do usuário autenticado ou null se não for encontrado.</returns>
    public static string? GetAuthenticatedUserEmail(IHttpContextAccessor httpContextAccessor)
    {
        if (httpContextAccessor == null)
        {
            throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        var user = httpContextAccessor.HttpContext?.User;
        if (user == null)
        {
            return null;
        }

        // Busca o claim "email" que contém o email do usuário
        var emailClaim = user.Claims.FirstOrDefault(c => c.Type == "userEmail");
        return emailClaim?.Value;
    }

}
