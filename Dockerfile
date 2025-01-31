FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG configuration=Release
WORKDIR /src
COPY ["ToFood.ZipAPI/ToFood.ZipAPI.csproj", "ToFood.ZipAPI/"]
RUN dotnet restore "ToFood.ZipAPI/ToFood.ZipAPI.csproj"
COPY . .
WORKDIR "/src/ToFood.ZipAPI"
RUN dotnet build "ToFood.ZipAPI.csproj" -c $configuration -o /app/build

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "ToFood.ZipAPI.csproj" -c $configuration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ToFood.ZipAPI.dll"]
