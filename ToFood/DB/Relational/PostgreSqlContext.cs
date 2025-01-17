using Microsoft.EntityFrameworkCore;

namespace ToFood.Domain.DB.Relational;

/// <summary>
/// Contexto específico para PostgreSQL.
/// </summary>
public class PostgreSqlContext : ToFoodRelationalContext
{
    /// <summary>
    /// Inicializa o contexto PostgreSQL com as opções fornecidas.
    /// </summary>
    public PostgreSqlContext(DbContextOptions<PostgreSqlContext> options) : base(options) { }
}
