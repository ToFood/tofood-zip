using Amazon;
using Amazon.Runtime;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace ToFood.Domain.Helpers;

public class SecretsHelper
{
    /// <summary>
    /// Recupera os segredos do Secrets Manager com base na configuração do appsettings.json.
    /// </summary>
    /// <param name="configuration">Configuração do sistema contendo as credenciais AWS.</param>
    /// <returns>Um dicionário contendo as variáveis de ambiente do segredo especificado.</returns>
    /// <summary>
    /// Recupera os segredos do AWS Secrets Manager e os retorna em um formato plano.
    /// </summary>
    /// <param name="configuration">Configuração do sistema contendo as credenciais AWS.</param>
    /// <returns>Um dicionário contendo todas as chaves e valores do segredo em formato plano.</returns>
    public static async Task<IDictionary<string, string>> GetSecretsAWS(IConfiguration configuration)
    {
        try
        {
            Console.WriteLine($"SecretManager ENV: {Environment.GetEnvironmentVariable("AWS__SecretManager")} | SecretManager Configuration: {configuration["AWS:SecretManager"]}");

            // Criar cliente do Secrets Manager usando as credenciais do appsettings.json
            var secretsClient = new AmazonSecretsManagerClient(
                new BasicAWSCredentials(
                    configuration["AWS:AccessKey"] ?? Environment.GetEnvironmentVariable("AWS__AccessKey"),
                    configuration["AWS:SecretKey"] ?? Environment.GetEnvironmentVariable("AWS__SecretKey")
                ),
                RegionEndpoint.GetBySystemName(configuration["AWS:Region"] ?? Environment.GetEnvironmentVariable("AWS__Region") ??  "us-east-1")
            );

            // Recuperar o nome do segredo do appsettings.json
            var secretName = configuration["AWS:SecretManager"] ?? Environment.GetEnvironmentVariable("AWS__SecretManager");
            if (string.IsNullOrEmpty(secretName))
            {
                throw new ApplicationException("O nome do segredo não foi configurado em 'AWS:SecretManager' no appsettings.json.");
            }

            // Recuperar o valor do segredo do Secrets Manager
            var secretValueResponse = await secretsClient.GetSecretValueAsync(new GetSecretValueRequest
            {
                SecretId = secretName,
                VersionStage = "AWSCURRENT" // Versão mais recente
            });

            if (!string.IsNullOrEmpty(secretValueResponse.SecretString))
            {
                // Converter o JSON do segredo em um dicionário plano
                var secretData = JObject.Parse(secretValueResponse.SecretString);
                var flattenedSecrets = new Dictionary<string, string>();
                JsonHelper.FlattenJson(secretData, flattenedSecrets);
                return flattenedSecrets;
            }

            return new Dictionary<string, string>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erro ao recuperar o segredo '{configuration["AWS:SecretManager"]}': {ex.Message}");
            throw;
        }
    }

}
