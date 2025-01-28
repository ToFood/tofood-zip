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

    /// <summary>
    /// Testa a conexão com a AWS.
    /// </summary>
    /// <param name="configuration">Configuração do sistema.</param>
    public static async void TestAWSConnection(IConfiguration configuration)
    {
        try
        {
            // Obter o cliente AWS configurado
            var sqsClient = GetAWSClient(configuration);

            // Tentar listar filas para testar a conexão
            var response = await sqsClient.ListQueuesAsync(new Amazon.SQS.Model.ListQueuesRequest());

            // Se a lista for recuperada, conexão está ok
            Console.WriteLine("📦 AWS - Conexão bem sucedida com a Cloud");
            Console.WriteLine($"🐇 SQS - Conexão bem sucedida com a Queue Service. Filas disponíveis: {string.Join(", ", response.QueueUrls)}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erro ao testar a conexão com a AWS & SQS: {ex.Message}");
        }
    }
}
