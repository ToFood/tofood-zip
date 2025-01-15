var builder = WebApplication.CreateBuilder(args);

// Configura��o de servi�os

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

app.UseAuthorization(); // Middleware para autentica��o/valida��o de roles, se necess�rio

// Mapeia automaticamente as rotas das controllers
app.MapControllers();

app.Run();
