using Newtonsoft.Json.Linq;

namespace ToFood.Domain.Helpers;

internal class JsonHelper
{
    /// <summary>
    /// Função recursiva para "achatar" o JSON em um formato plano.
    /// </summary>
    /// <param name="token">O JToken atual.</param>
    /// <param name="flattenedSecrets">O dicionário onde as chaves/valores serão armazenados.</param>
    /// <param name="parentKey">A chave do nível pai (para manter o contexto das chaves aninhadas).</param>
    internal static void FlattenJson(JToken token, IDictionary<string, string> flattenedSecrets, string parentKey = "")
    {
        if (token is JObject obj)
        {
            foreach (var property in obj.Properties())
            {
                var newKey = string.IsNullOrEmpty(parentKey) ? property.Name : $"{parentKey}:{property.Name}";
                FlattenJson(property.Value, flattenedSecrets, newKey);
            }
        }
        else if (token is JArray arr)
        {
            for (int i = 0; i < arr.Count; i++)
            {
                var newKey = $"{parentKey}[{i}]";
                FlattenJson(arr[i], flattenedSecrets, newKey);
            }
        }
        else
        {
            flattenedSecrets[parentKey] = token.ToString();
        }
    }
}
