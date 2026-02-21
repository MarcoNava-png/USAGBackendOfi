# ===== build =====
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore ./WebApplication2/WebApplication2.csproj
RUN dotnet publish ./WebApplication2/WebApplication2.csproj -c Release -o /app/publish

# ===== run =====
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final

# Instalar dependencias nativas para SkiaSharp/QuestPDF (libfontconfig, libfreetype, fuentes)
RUN apt-get update && apt-get install -y --no-install-recommends \
    libfontconfig1 \
    libfreetype6 \
    fonts-liberation \
    fonts-dejavu-core \
    fontconfig \
    && fc-cache -f -v \
    && rm -rf /var/lib/apt/lists/*

# Crear usuario no-root para seguridad
RUN adduser --disabled-password --gecos '' --uid 1001 appuser

WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080

# Copiar archivos publicados
COPY --from=build --chown=appuser:appuser /app/publish .

# Crear directorio de uploads con permisos para appuser
RUN mkdir -p /app/uploads && chown appuser:appuser /app/uploads

# Cambiar a usuario no-root
USER appuser

EXPOSE 8080
ENTRYPOINT ["dotnet","WebApplication2.dll"]
