
# MottuProjeto - API .NET

## Descrição do Projeto

Este projeto é uma API REST desenvolvida em ASP.NET Core para atender ao desafio proposto pela Mottu no Challenge 2025. A API permite o gerenciamento de motos e áreas de atuação, com integração ao banco de dados Oracle.

## Tecnologias Utilizadas

- ASP.NET Core
- Entity Framework Core
- Oracle Database
- Swagger para documentação da API

## Como Executar o Projeto

### Pré-requisitos

- .NET SDK instalado
- Banco Oracle configurado e rodando
- String de conexão válida no arquivo `appsettings.json`

### Configuração da String de Conexão

No arquivo `appsettings.json`, configure sua string de conexão:

```
{
  "ConnectionStrings": {
    "OracleConnection": "User Id=rm555708;Password=221005;Data Source=oracle.fiap.com.br:1521/orcl"
  }
}
```

### Comandos para rodar

```
dotnet restore
dotnet build
dotnet run
```

Acesse no navegador:

```
https://localhost:5000/swagger
```

## Endpoints Disponíveis

### Área

- POST `/api/areas` - Cadastrar nova área
- GET `/api/areas` - Listar todas as áreas

### Moto

- POST `/api/motos` - Cadastrar nova moto
- GET `/api/motos` - Listar todas as motos
- GET `/api/motos/byarea?idArea={id}` - Listar motos por área
- PUT `/api/motos/{id}` - Atualizar moto
- DELETE `/api/motos/{id}` - Remover moto

## JSON de Exemplo para Testes

### Cadastro de Área

```
POST /api/areas

{
  "id": 1,
  "nome": "Zona Norte"
}
```

### Cadastro de Moto

```
POST /api/motos *é necessario cadastrar uma area para conseguir cadastrar uma moto*

{
  "placa": "ABC1234",
  "modelo": "Honda CG 160",
  "idArea": 1
}
```

### Atualizar Moto

```
PUT /api/motos/1

{
  "placa": "DEF5678",
  "modelo": "Yamaha Factor",
  "idArea": 1
}
```

## Observações

- As tabelas utilizadas no banco Oracle são:

  - T_VM_AREA
  - T_VM_MOTO


##  Desenvolvido por:
RM 556293 Alice Teixeira Caldeira  
RM 555708 Gustavo Goulart
RM 554557 Victor Medeiros

