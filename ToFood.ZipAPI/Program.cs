using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System.Text;
using Microsoft.OpenApi.Models;
using ToFood.Domain.Entities.NonRelational;
using ToFood.Domain.Factories;
using ToFood.Domain.Helpers;
using ToFood.Domain.Extensions;
using ToFood.Domain.Services.TokenManager;


var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// Recuperar os segredos do AWS Secrets Manager
var secrets = await SecretsHelper.GetSecretsAWS(builder.Configuration);

// Adicionar os segredos ao builder.Configuration
foreach (var secret in secrets)
{
    builder.Configuration[secret.Key] = secret.Value;
}

// Configuração do JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder?.Configuration["Jwt:Issuer"],
            ValidAudience = builder?.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder?.Configuration["Jwt:Key"] ?? "")) // Use uma chave secreta segura
        };
    });

builder.Services.AddAuthorization();

// Registra o IHttpContextAccessor no contêiner de dependências
builder.Services.AddHttpContextAccessor();

// Configuração do banco de dados usando o DatabaseFactory
DatabaseFactory.ConfigureDatabases(builder.Services, builder.Configuration);

// Recupera a coleção de logs para o logger
var serviceProvider = builder.Services.BuildServiceProvider();
var logCollection = serviceProvider.GetRequiredService<IMongoCollection<Log>>();
var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();

// Configura o logger customizado
builder.Logging.ClearProviders();
builder.Logging.AddProvider(new MongoDBLoggerProvider(logCollection, httpContextAccessor));

// DI (Injeção de Dependência)
// Registra os serviços do domínio
builder.Services.AddDomainServices();

// Adiciona o serviço de CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("ToFoodCORS", policy =>
    {
        policy.AllowAnyOrigin()  // Permite qualquer origem
              .AllowAnyMethod()  // Permite qualquer método HTTP (GET, POST, etc.)
              .AllowAnyHeader(); // Permite qualquer cabeçalho
    });
});

// Configuração do Swagger para incluir suporte a JWT
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "ToFood API",
        Description = "API para autenticação e serviços relacionados ao ToFood"
    });

    // Configura o esquema de segurança para autenticação JWT
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insira o token JWT no formato: Bearer {seu token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>() // Sem escopos específicos
        }
    });
});

// Adiciona suporte para controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Configuração do pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Ativa o CORS antes de outras configurações
app.UseCors("ToFoodCORS");

app.UseHttpsRedirection();

// Adiciona autenticação e autorização
app.UseAuthentication();
app.UseAuthorization();

// Mapeia automaticamente as rotas das controllers
app.MapControllers();

// Informações úteis de Inicialização
Console.WriteLine($"🧊 .NET Version: [{Environment.Version}]"); // Exibe a versão do .NET
AWSTokenManager.TestAWSConnection(builder.Configuration);
Console.WriteLine($"🛜 Aplicação rodando na porta: [{builder.Configuration["ASPNETCORE_URLS"]}]");
Console.WriteLine($"✳️ Swagger rodando na porta: [{builder.Configuration["ASPNETCORE_URLS"]}/swagger]");

app.Run();
