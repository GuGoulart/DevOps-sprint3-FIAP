# Stage 1 - build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia o csproj e restaura dependências
COPY ["MottuProjeto.csproj", "./"]
RUN dotnet restore "./MottuProjeto.csproj"

# Copia todo o código e publica
COPY . .
RUN dotnet publish "./MottuProjeto.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2 - runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copia os arquivos publicados ANTES de criar o usuário
COPY --from=build /app/publish .

# Cria usuário não-root e ajusta permissões DEPOIS de copiar
RUN adduser --disabled-password --gecos "" appuser \
    && chown -R appuser:appuser /app \
    && chmod -R 755 /app

# Use usuário não-root
USER appuser

# Configura para usar porta 8080 (usuários não-root não podem usar porta 80)
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "MottuProjeto.dll"]