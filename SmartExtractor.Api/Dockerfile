# --- Etapa 1: Build y Publish ---
# Usamos el SDK de .NET como base para compilar
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar archivos de proyecto y restaurar dependencias
# (Hacemos esto primero para aprovechar el caché de capas de Docker)
COPY ["SmartExtractor.Api.csproj", "./"]
RUN dotnet restore "./SmartExtractor.Api.csproj"

# Copiar el resto del código fuente y compilar
COPY . .
RUN dotnet build "./SmartExtractor.Api.csproj" -c Release -o /app/build

# Publicar la aplicación optimizada
FROM build AS publish
RUN dotnet publish "./SmartExtractor.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# --- Etapa 2: Runtime Final ---
# Usamos la imagen base del runtime ASP.NET Core (mucho más pequeña)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Copiar lo publicado desde la etapa anterior
COPY --from=publish /app/publish .

# Comando de entrada para ejecutar la API
# Asegúrate de cambiar 'MiApi.dll' por el nombre real de tu assembly
ENTRYPOINT ["dotnet", "SmartExtractor.Api.dll"]