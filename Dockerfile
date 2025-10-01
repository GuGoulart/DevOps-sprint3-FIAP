# Stage 1 - build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia o csproj e restaura dependências
COPY ["MottuProjeto.csproj", "./"]
RUN dotnet restore "./MottuProjeto.csproj"

# Copia todo o código e publica
COPY . .
RUN dotnet publish "./MottuProjeto.csproj" -c Release -o /app/publish

# Stage 2 - runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Cria usuário não-root e garante permissões
RUN adduser --disabled-password --gecos "" appuser \
    && chown -R appuser:appuser /app

# Copia os arquivos publicados
COPY --from=build /app/publish .

# Use usuário não-root
USER appuser

# Variável para Kestrel
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

ENTRYPOINT ["dotnet", "MottuProjeto.dll"]
