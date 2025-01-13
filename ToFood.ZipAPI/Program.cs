var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers(); // Adiciona suporte para controllers
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Adiciona roteamento para controllers
app.UseRouting();

app.UseAuthorization(); // Middleware para autenticação/validação de roles, se necessário

// Mapeia automaticamente as rotas das controllers
app.MapControllers();

app.Run();
