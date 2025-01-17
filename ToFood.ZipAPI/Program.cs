using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])) // Use uma chave secreta segura
        };
    });

builder.Services.AddAuthorization();

// Adiciona o DbContext ao cont�iner de inje��o de depend�ncias
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

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

// Adiciona roteamento para controllers
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization(); ; // Middleware para autentica��o/valida��o de roles, se necess�rio

// Mapeia automaticamente as rotas das controllers
app.MapControllers();

app.Run();
