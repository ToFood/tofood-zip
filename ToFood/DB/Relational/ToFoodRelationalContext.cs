using Microsoft.EntityFrameworkCore;
using ToFood.Domain.Entities.Relational;

namespace ToFood.Domain.DB.Relational;

/// <summary>
/// Contexto genérico para bancos relacionais.
/// </summary>
public class ToFoodRelationalContext : DbContext
{
    /// <summary>
    /// Inicializa o contexto com as opções fornecidas.
    /// </summary>
    public ToFoodRelationalContext(DbContextOptions options) : base(options) { }

    /// <summary>
    /// Representa a tabela [users] no banco de dados.
    /// </summary>
    public DbSet<User> Users { get; set; }

    /// <summary>
    /// Configuração do mapeamento das entidades.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuração da entidade User
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(255);
            entity.Property(u => u.PasswordHash).HasMaxLength(255);
            entity.Property(u => u.CreatedAt).IsRequired();
        });

        // Adicione configurações para outras entidades aqui
    }
}
