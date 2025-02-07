# Etapa 1: Build da aplicação
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copia os arquivos da solução e os projetos para permitir melhor cache do Docker
COPY ToFood.sln ./
COPY ToFood/*.csproj ToFood/
COPY Queues/ToFood.FileNotification/*.csproj Queues/ToFood.FileNotification/
COPY ToFood.ZipAPI/*.csproj ToFood.ZipAPI/
COPY Tests/*.csproj Tests/
COPY Workers/*.csproj Workers/

# Restaura as dependências da solução inteira
RUN dotnet restore

# Copia todos os arquivos do projeto para a imagem
COPY . .

# Compila a API principal
WORKDIR /app/ToFood.ZipAPI
RUN dotnet publish -c Release -o /out /p:UseAppHost=false

# Etapa 2: Runtime da aplicação
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Configura a aplicação para escutar em todas as interfaces
ENV ASPNETCORE_URLS=http://+:9090

# Copia a aplicação compilada da etapa anterior
COPY --from=build /out ./

# Expõe a porta usada pela aplicação
EXPOSE 9090

# Comando de inicialização
ENTRYPOINT ["dotnet", "ToFood.ZipAPI.dll"]