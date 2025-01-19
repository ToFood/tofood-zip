using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System.Text;
using ToFood.Domain.Entities.NonRelational;
using ToFood.Domain.Factories;
using ToFood.Domain.Helpers;
using ToFood.Domain.Services;

var builder = WebApplication.CreateBuilder(args);


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
            ValidIssuer = "your-issuer",
            ValidAudience = "your-audience",
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
builder.Logging.AddProvider(new MongoDbLoggerProvider(logCollection, httpContextAccessor));

// DI (Injeção de Dependência)
builder.Services.AddScoped<ZipService>();
builder.Services.AddScoped<YoutubeService>();
builder.Services.AddScoped<LogHelper>();
builder.Services.AddScoped<AuthService>();

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

// Adiciona suporte para controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

app.Run();
