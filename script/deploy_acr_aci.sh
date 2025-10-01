#!/usr/bin/env bash
set -euo pipefail

# --------- Ajuste esses valores antes de rodar -----------
RG="rg-mottu-sprint"
LOCATION="brazilsouth"
ACR_NAME="mottuacr$((RANDOM % 10000))"   # tem que ser único e lowercase
IMAGE_TAG="v1"
APP_NAME="mottuaci"
SQL_SERVER="mottusql$((RANDOM % 10000))"
SQL_ADMIN="sqladminuser"
SQL_PASS="StrongPass!234"   # Troque por algo seguro
DB_NAME="MottuDb"
DNS_LABEL="mottuapp$((RANDOM % 10000))"
# ---------------------------------------------------------

echo "1) criar resource group"
az group create -n $RG -l $LOCATION

echo "2) criar ACR"
az acr create -n $ACR_NAME -g $RG --sku Standard --admin-enabled true

echo "3) build local e push (alternativa: az acr build)"
LOGIN_SERVER="${ACR_NAME}.azurecr.io"
FULL_IMAGE="${LOGIN_SERVER}/mottuprojeto:${IMAGE_TAG}"

# build local
docker build -t $FULL_IMAGE .

# login e push
az acr login -n $ACR_NAME
docker push $FULL_IMAGE

echo "4) criar Azure SQL Server e DB"
az sql server create -n $SQL_SERVER -g $RG -l $LOCATION -u $SQL_ADMIN -p $SQL_PASS
az sql db create -g $RG -s $SQL_SERVER -n $DB_NAME --service-objective S0

# permitir acesso de serviços Azure (simplificação para ACI)
az sql server firewall-rule create -g $RG -s $SQL_SERVER -n AllowAzureServices --start-ip-address 0.0.0.0 --end-ip-address 0.0.0.0

# pegar credenciais ACR para ACI
ACR_USERNAME=$(az acr credential show -n $ACR_NAME --query "username" -o tsv)
ACR_PASSWORD=$(az acr credential show -n $ACR_NAME --query "passwords[0].value" -o tsv)

# montar connection string para Azure SQL
CONN_STR="Server=tcp:${SQL_SERVER}.database.windows.net,1433;Initial Catalog=${DB_NAME};User ID=${SQL_ADMIN};Password=${SQL_PASS};"

echo "5) criar ACI com a imagem (passando ConnectionStrings__Default como env)"
az container create -g $RG -n $APP_NAME \
  --image $FULL_IMAGE \
  --dns-name-label $DNS_LABEL \
  --ports 80 \
  --registry-login-server $LOGIN_SERVER \
  --registry-username $ACR_USERNAME \
  --registry-password $ACR_PASSWORD \
  --environment-variables ConnectionStrings__Default="$CONN_STR"

echo "FIM. Acesse via: http://${DNS_LABEL}.${LOCATION}.azurecontainer.io (ou veja o FQDN exibido)"
