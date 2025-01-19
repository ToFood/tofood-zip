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
    public virtual DbSet<User> Users { get; set; }

    /// <summary>
    /// Representa a tabela [videos] no banco de dados.
    /// </summary>
    public virtual DbSet<Video> Videos { get; set; }

    /// <summary>
    /// Representa a tabela [zip_files] no banco de dados.
    /// </summary>
    public virtual DbSet<ZipFile> ZipFiles { get; set; }

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

            // Relacionamento: Um usuário tem muitos vídeos
            entity.HasMany(u => u.Videos)
                  .WithOne(v => v.User)
                  .HasForeignKey(v => v.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configuração da entidade Video
        modelBuilder.Entity<Video>(entity =>
        {
            entity.ToTable("videos");
            entity.HasKey(v => v.Id);
            entity.Property(v => v.FileName).IsRequired().HasMaxLength(255);
            entity.Property(v => v.FilePath).IsRequired().HasMaxLength(1024);
            entity.Property(v => v.Status).IsRequired();
            entity.Property(v => v.CreatedAt).IsRequired();
            entity.Property(v => v.UpdatedAt).IsRequired();

            // Relacionamento: Um vídeo tem muitos arquivos ZIP
            entity.HasMany(v => v.ZipFiles)
                  .WithOne(z => z.Video)
                  .HasForeignKey(z => z.VideoId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configuração da entidade ZipFile
        modelBuilder.Entity<ZipFile>(entity =>
        {
            entity.ToTable("zip_files");
            entity.HasKey(z => z.Id);
            entity.Property(z => z.FilePath).IsRequired().HasMaxLength(1024);
            entity.Property(z => z.Status).IsRequired();
            entity.Property(z => z.CreatedAt).IsRequired();
            entity.Property(z => z.UpdatedAt).IsRequired();
        });
    }
}
