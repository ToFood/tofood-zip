using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ToFood.Domain.Factories;    // Para DatabaseFactory
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

// Configuração do banco de dados usando o DatabaseFactory
DatabaseFactory.ConfigureDatabases(builder.Services, builder.Configuration);

// DI (Injeção de Dependência)
builder.Services.AddScoped<ZipService>();
builder.Services.AddScoped<YoutubeService>();

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
