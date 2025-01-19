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


// Configura��o do JWT
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

// Registra o IHttpContextAccessor no cont�iner de depend�ncias
builder.Services.AddHttpContextAccessor();

// Configura��o do banco de dados usando o DatabaseFactory
DatabaseFactory.ConfigureDatabases(builder.Services, builder.Configuration);

// Recupera a cole��o de logs para o logger
var serviceProvider = builder.Services.BuildServiceProvider();
var logCollection = serviceProvider.GetRequiredService<IMongoCollection<Log>>();
var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();

// Configura o logger customizado
builder.Logging.ClearProviders();
builder.Logging.AddProvider(new MongoDbLoggerProvider(logCollection, httpContextAccessor));

// DI (Inje��o de Depend�ncia)
builder.Services.AddScoped<ZipService>();
builder.Services.AddScoped<YoutubeService>();
builder.Services.AddScoped<LogHelper>();
builder.Services.AddScoped<AuthService>();

// Adiciona o servi�o de CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("ToFoodCORS", policy =>
    {
        policy.AllowAnyOrigin()  // Permite qualquer origem
              .AllowAnyMethod()  // Permite qualquer m�todo HTTP (GET, POST, etc.)
              .AllowAnyHeader(); // Permite qualquer cabe�alho
    });
});

// Adiciona suporte para controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configura��o do pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Ativa o CORS antes de outras configura��es
app.UseCors("ToFoodCORS");

app.UseHttpsRedirection();

// Adiciona autentica��o e autoriza��o
app.UseAuthentication();
app.UseAuthorization();

// Mapeia automaticamente as rotas das controllers
app.MapControllers();

app.Run();
