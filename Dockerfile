# ===== build =====
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore ./WebApplication2/WebApplication2.csproj
RUN dotnet publish ./WebApplication2/WebApplication2.csproj -c Release -o /app/publish

# ===== run =====
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final

# Crear usuario no-root para seguridad
RUN adduser --disabled-password --gecos '' --uid 1001 appuser

WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080

# Copiar archivos publicados
COPY --from=build --chown=appuser:appuser /app/publish .

# Cambiar a usuario no-root
USER appuser

EXPOSE 8080
ENTRYPOINT ["dotnet","WebApplication2.dll"]

