using Amazon;
using Amazon.Runtime;
using Amazon.SQS;
using Microsoft.Extensions.Configuration;

namespace ToFood.Domain.Services.TokenManager;

public class AWSTokenManager
{
    /// <summary>
    /// Retorna um cliente AmazonSQSClient configurado com credenciais personalizadas.
    /// </summary>
    public static AmazonSQSClient GetAWSClient(IConfiguration configuration)
    {
        try
        {
            // Criação do cliente SQS com as credenciais da configuração
            return new AmazonSQSClient(
                new BasicAWSCredentials(configuration["AWS:AccessKey"], configuration["AWS:SecretKey"]),
                RegionEndpoint.GetBySystemName(configuration["AWS:Region"] ?? "us-east-1")
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao criar o cliente SQS: {ex.Message}");
            throw;
        }
    }
}
