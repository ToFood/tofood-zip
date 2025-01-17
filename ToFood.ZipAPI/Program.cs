using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ToFood.Domain.Factories;    // Para DatabaseFactory
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

// Configura��o do banco de dados usando o DatabaseFactory
DatabaseFactory.ConfigureDatabases(builder.Services, builder.Configuration);

// DI (Inje��o de Depend�ncia)
builder.Services.AddScoped<ZipService>();
builder.Services.AddScoped<YoutubeService>();

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
