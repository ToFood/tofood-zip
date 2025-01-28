using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;

namespace ToFood.Domain.Services.TokenManager;

public class AWSTokenManager
{
    public static async Task<string> GetSecretAsync()
    {
        string secretName = "rds-db-credentials/db-ISQ2EZTX2TTIE6RLSZRJSH33ZE/tofood/1737161920362";
        string region = "us-east-1";

        IAmazonSecretsManager client = new AmazonSecretsManagerClient(RegionEndpoint.GetBySystemName(region));

        var request = new GetSecretValueRequest
        {
            SecretId = secretName,
            VersionStage = "AWSCURRENT" // VersionStage defaults to AWSCURRENT if unspecified.
        };

        try
        {
            var response = await client.GetSecretValueAsync(request);

            // Retornar o segredo como string
            return response.SecretString;
        }
        catch (Exception e)
        {
            // Lidar com exceções (log ou tratamento específico)
            throw new Exception($"Erro ao obter o segredo do AWS Secrets Manager: {e.Message}", e);
        }
    }
}
