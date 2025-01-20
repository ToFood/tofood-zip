public static class RequestSanitizer
{
    /// <summary>
    /// Sanitiza as propriedades sensíveis de um objeto.
    /// </summary>
    public static object Sanitize<T>(T request)
    {
        if (request == null) return new { };

        var sanitizedObject = new Dictionary<string, object?>();

        // Obtém todas as propriedades públicas da classe
        var properties = typeof(T).GetProperties();

        foreach (var property in properties)
        {
            var value = property.GetValue(request);

            // Verifica se a propriedade é sensível (baseada no nome ou em um atributo)
            if (IsSensitive(property))
            {
                sanitizedObject[property.Name] = "***"; // Mascara o valor
            }
            else
            {
                sanitizedObject[property.Name] = value; // Mantém o valor original
            }
        }

        return sanitizedObject;
    }

    /// <summary>
    /// Verifica se uma propriedade é sensível com base no nome ou em atributos.
    /// </summary>
    private static bool IsSensitive(System.Reflection.PropertyInfo property)
    {
        // Convenção: propriedades com "Password" no nome são consideradas sensíveis
        if (property.Name.Contains("Password", StringComparison.OrdinalIgnoreCase)) return true;

        // Verifica se a propriedade possui um atributo customizado [Sensitive]
        var hasSensitiveAttribute = Attribute.IsDefined(property, typeof(SensitiveAttribute));
        return hasSensitiveAttribute;
    }
}

/// <summary>
/// Atributo para marcar propriedades como sensíveis.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class SensitiveAttribute : Attribute { }
