# Imagem base para o runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

# Imagem para build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG configuration=Release
WORKDIR /src

# Copia apenas os arquivos de projeto e restaura dependências
COPY ["ToFood.ZipAPI/ToFood.ZipAPI.csproj", "ToFood.ZipAPI/"]
RUN dotnet restore "ToFood.ZipAPI/ToFood.ZipAPI.csproj"

# Copia todo o código e compila o projeto
COPY . .
WORKDIR "/src/ToFood.ZipAPI"
RUN dotnet build "ToFood.ZipAPI.csproj" -c $configuration -o /app/build

# Publicação do projeto
FROM build AS publish
ARG configuration=Release
RUN dotnet publish "ToFood.ZipAPI.csproj" -c $configuration -o /app/publish

# Imagem final para execução
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ToFood.ZipAPI.dll"]
