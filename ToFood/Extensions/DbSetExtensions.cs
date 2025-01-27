using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Npgsql;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ToFood.Domain.Extensions;

/// <summary>
/// Contém métodos de extensão para o DbSet.
/// </summary>
public static class DbSetExtensions
{
    /// <summary>
    /// Atualiza uma entidade com base no ID e uma expressão de atualização.
    /// </summary>
    /// <typeparam name="TEntity">Tipo da entidade.</typeparam>
    /// <param name="dbSet">DbSet da entidade.</param>
    /// <param name="entityId">ID da entidade a ser atualizada.</param>
    /// <param name="updateExpression">Expressão com os campos e valores a serem atualizados.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Número de registros afetados.</returns>
    public static async Task<int> UpdateAsync<TEntity>(
        this DbSet<TEntity> dbSet,
        long entityId,
        Expression<Func<TEntity, TEntity>> updateExpression,
        CancellationToken cancellationToken = default) where TEntity : class
    {
        var updateClause = new StringBuilder();
        var parameters = new List<object>();

        // Obtém o nome da tabela a partir do atributo [Table], se presente
        var tableName = typeof(TEntity).GetCustomAttribute<TableAttribute>(false)?.Name ?? typeof(TEntity).Name;

        // Extrai as propriedades do corpo da expressão
        if (updateExpression.Body is MemberInitExpression initExpression)
        {
            foreach (var binding in initExpression.Bindings)
            {
                var memberAssignment = (MemberAssignment)binding;
                var propName = memberAssignment.Member.Name;

                // Obtém o nome da coluna a partir do atributo [Column], se presente
                var propertyInfo = typeof(TEntity).GetProperty(propName);
                var columnAttribute = propertyInfo?.GetCustomAttribute<ColumnAttribute>(false);
                var columnName = columnAttribute?.Name ?? propName;

                // Extrai o valor da expressão diretamente
                var value = memberAssignment.Expression is ConstantExpression constantExpr
                    ? constantExpr.Value
                    : Expression.Lambda(memberAssignment.Expression, updateExpression.Parameters)
                      .Compile().DynamicInvoke(Activator.CreateInstance<TEntity>());

                // Tratamento de enum para compatibilidade com PostgreSQL
                if (propertyInfo?.PropertyType.IsEnum == true)
                {
                    value = Convert.ChangeType(value, Enum.GetUnderlyingType(propertyInfo.PropertyType));
                }

                updateClause.Append($"\"{columnName}\" = @p{parameters.Count}, ");
                parameters.Add(new NpgsqlParameter($"@p{parameters.Count}", value ?? DBNull.Value));
            }

            // Remove a última vírgula
            if (updateClause.Length > 0)
                updateClause.Length -= 2;
        }
        else
        {
            throw new ArgumentException("A expressão de atualização deve ser uma expressão de inicialização de membro.");
        }

        // Monta o SQL de atualização
        var sql = $@"
            UPDATE ""{tableName}""
            SET {updateClause}
            WHERE ""id"" = @EntityId";

        parameters.Add(new NpgsqlParameter("@EntityId", entityId));

        // Executa a consulta SQL no contexto do banco de dados
        var context = dbSet.GetService<ICurrentDbContext>().Context;
        return await context.Database.ExecuteSqlRawAsync(sql, parameters.ToArray(), cancellationToken);
    }
}
