var builder = WebApplication.CreateBuilder(args);

// Configuração de serviços

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

// Adiciona roteamento para controllers
app.UseRouting();

app.UseAuthorization(); // Middleware para autenticação/validação de roles, se necessário

// Mapeia automaticamente as rotas das controllers
app.MapControllers();

app.Run();
