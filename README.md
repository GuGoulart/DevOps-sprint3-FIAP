# MotoMoto

# SPRINT3 - DevOps Tools & Cloud Computing

## Descrição da Solução

Sistema de gerenciamento de motocicletas e usuários desenvolvido em .NET 8.0, implementando uma API REST completa com operações CRUD. A aplicação permite o cadastro, consulta, atualização e exclusão de motocicletas, áreas e usuários, com autenticação e implementação de HATEOAS para navegação dinâmica da API.

### Fluxo de Deploy:
1. **Build da Aplicação**: Compilação do código .NET
2. **Criação da Imagem Docker**: Build do Dockerfile
3. **Push para ACR**: Upload da imagem para Azure Container Registry
4. **Deploy no ACI**: Criação do container instance a partir da imagem do ACR
5. **Conexão com Banco**: Container se conecta ao banco de dados em nuvem

### Fluxo de Requisição:

Cliente → ACI (API .NET) → Banco de Dados → ACI → Cliente

## Estrutura do Banco de Dados

O arquivo `script/script_bd.sql` contém o DDL completo das tabelas com:
- Definição de todas as tabelas
- Chaves primárias e estrangeiras
- Comentários explicativos
- Constraints e índices

### Tabelas Principais:
- **Motos**: Cadastro de motocicletas
- **Usuarios**: Dados dos usuários do sistema
- **Areas**: Áreas operacionais

## Pré-requisitos

- Azure CLI instalado
- Docker instalado
- Conta ativa no Azure
- Git instalado
- .NET 8.0 SDK (para desenvolvimento local)

## Passo a Passo para Deploy

### 1️⃣ Clone do Repositório
```bash
git clone https://github.com/SEU_USUARIO/SPRINT1-DOTNET-MAIN.git
cd SPRINT1-DOTNET-MAIN
```

### 2️⃣ Configuração do Banco de Dados
Criar Banco de Dados na Azure (exemplo com Azure SQL):
## Login no Azure
az login

## Criar grupo de recursos
az group create --name rg-mottu-projeto --location eastus

## Criar SQL Server
az sql server create \
  --name sql-mottu-server \
  --resource-group rg-mottu-projeto \
  --location eastus \
  --admin-user adminmottu \
  --admin-password SuaSenhaSegura123!

## Criar banco de dados
az sql db create \
  --resource-group rg-mottu-projeto \
  --server sql-mottu-server \
  --name db-mottu \
  --service-objective Basic

## Configurar firewall para permitir acesso do Azure
az sql server firewall-rule create \
  --resource-group rg-mottu-projeto \
  --server sql-mottu-server \
  --name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0

  ## Conectar ao banco e executar script_bd.sql
  Você pode usar Azure Data Studio, SQL Server Management Studio ou Azure Portal

  ### 3️⃣ Configurar String de Conexão
Edite o arquivo appsettings.json com sua connection string:
``` bash
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:sql-mottu-server.database.windows.net,1433;Database=db-mottu;User ID=adminmottu;Password=SuaSenhaSegura123!;Encrypt=True;"
  }
}
```

### 4️⃣ Build da Imagem Docker

## Build da imagem Docker
docker build -t mottu-projeto:v1 .

## Testar localmente (opcional)
docker run -p 8080:8080 mottu-projeto:v1

### 5️⃣ Criar Azure Container Registry (ACR)

## Criar ACR
az acr create \
  --resource-group rg-mottu-projeto \
  --name acrmottuprojeto \
  --sku Basic \
  --location eastus

## Fazer login no ACR
az acr login --name acrmottuprojeto

### 6️⃣ Push da Imagem para ACR

## Tag da imagem
docker tag mottu-projeto:v1 acrmottuprojeto.azurecr.io/mottu-projeto:v1

## Push para ACR
docker push acrmottuprojeto.azurecr.io/mottu-projeto:v1

## Verificar imagem no ACR
az acr repository list --name acrmottuprojeto --output table

### 7️⃣ Habilitar Admin no ACR
## Habilitar usuário admin no ACR
az acr update --name acrmottuprojeto --admin-enabled true

## Obter credenciais
az acr credential show --name acrmottuprojeto

### 8️⃣ Deploy no Azure Container Instance (ACI)

## Criar container instance
az container create \
  --resource-group rg-mottu-projeto \
  --name aci-mottu-projeto \
  --image acrmottuprojeto.azurecr.io/mottu-projeto:v1 \
  --cpu 1 \
  --memory 1.5 \
  --registry-login-server acrmottuprojeto.azurecr.io \
  --registry-username acrmottuprojeto \
  --registry-password $(az acr credential show --name acrmottuprojeto --query "passwords[0].value" -o tsv) \
  --dns-name-label mottu-projeto-app \
  --ports 8080 \
  --environment-variables \
    ConnectionStrings__DefaultConnection="Server=tcp:sql-mottu-server.database.windows.net,1433;Database=db-mottu;User ID=adminmottu;Password=SuaSenhaSegura123!;Encrypt=True;"

  ### 9️⃣ Verificar Deploy
  ## Verificar status do container
az container show \
  --resource-group rg-mottu-projeto \
  --name aci-mottu-projeto \
  --query "{Status:instanceView.state, IP:ipAddress.fqdn}" \
  --output table

## Obter logs
az container logs \
  --resource-group rg-mottu-projeto \
  --name aci-mottu-projeto

  ### 🔟 Acessar a Aplicação
  Acessar sua aplicação

 ## Testes da API
  Endpoints Disponíveis:

  ### 1. POST - Criar Moto
```bash
 curl -X POST http://mottu-projeto-app.eastus.azurecontainer.io:8080/api/motos \
  -H "Content-Type: application/json" \
  -d '{
    "modelo": "Honda CG 160",
    "marca": "Honda",
    "ano": 2024,
    "placa": "ABC1234",
    "quilometragem": 0,
    "disponivel": true
  }'

  Exemplo de JSON:
  {
  "modelo": "Honda CG 160",
  "marca": "Honda",
  "ano": 2024,
  "placa": "ABC1234",
  "quilometragem": 0,
  "disponivel": true
}
```
 ### 2. GET - Listar Todas as Motos

 ``` bash
curl -X GET http://mottu-projeto-app.eastus.azurecontainer.io:8080/api/motos
```
 ### 3. GET - Buscar Moto por ID
 ``` bash
curl -X GET http://mottu-projeto-app.eastus.azurecontainer.io:8080/api/motos/1
```
 ### 4. PUT - Atualizar Moto
  ``` bash
  curl -X PUT http://mottu-projeto-app.eastus.azurecontainer.io:8080/api/motos/1 \
  -H "Content-Type: application/json" \
  -d '{
    "id": 1,
    "modelo": "Honda CG 160 Titan",
    "marca": "Honda",
    "ano": 2024,
    "placa": "ABC1234",
    "quilometragem": 5000,
    "disponivel": true
  }'

  Exemplo de JSON:
{
  "id": 1,
  "modelo": "Honda CG 160 Titan",
  "marca": "Honda",
  "ano": 2024,
  "placa": "ABC1234",
  "quilometragem": 5000,
  "disponivel": true
}
````
 ### 5. DELETE - Excluir Moto
 ``` bash
 curl -X DELETE http://mottu-projeto-app.eastus.azurecontainer.io:8080/api/motos/1
```

## 🐳 Informações Docker
Imagem Base

Utiliza imagens oficiais da Microsoft: mcr.microsoft.com/dotnet/aspnet:8.0
Container NÃO roda como root (configurado com usuário não privilegiado)

Dockerfile
Localizado na raiz do projeto, implementa:

Multi-stage build para otimização
Usuário não privilegiado
Exposição da porta 8080

Docker Compose
Disponível para execução local em docker-compose.yml
```bash
# Executar localmente
docker-compose up -d

# Parar containers
docker-compose down
```

### Scripts Auxiliares
Script de Deploy ACR + ACI
Localizado em: script/deploy_acr_aci.sh

```` bash
# Dar permissão de execução
chmod +x script/deploy_acr_aci.sh

# Executar
./script/deploy_acr_aci.sh

````
### Comandos Docker Utilizados
```bash
# Build
docker build -t mottu-projeto:v1 .

# Tag
docker tag mottu-projeto:v1 acrmottuprojeto.azurecr.io/mottu-projeto:v1

# Push
docker push acrmottuprojeto.azurecr.io/mottu-projeto:v1

# Run (teste local)
docker run -p 8080:8080 mottu-projeto:v1

# Logs
docker logs <container-id>

# Listar containers
docker ps

````

## Segurança

✅ Container NÃO roda como root/admin
✅ Imagens oficiais utilizadas
✅ Strings de conexão via variáveis de ambiente
✅ HTTPS configurado
✅ Autenticação JWT implementada

## Estrutura do Projeto
<img width="489" height="371" alt="image" src="https://github.com/user-attachments/assets/4f711d8f-2bb7-4bd0-bc3a-8e87ae7a0b3d" />

-------------------
# Integrantes do Projeto
- Alice Teixeira Caldeira RM556293
- Gustavo Goulart Bretas RM555708
- Victor Nieves Britto Medeiros RM554557







 
    
