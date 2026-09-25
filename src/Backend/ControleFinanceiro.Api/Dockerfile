# Multi-stage: o SDK só existe na etapa de compilação; a imagem final leva apenas o runtime.
# O contexto de build é a RAIZ do repositório, porque a API precisa dos outros três projetos:
#   docker build -f src/Backend/ControleFinanceiro.Api/Dockerfile -t controle-financeiro-api .

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS compilacao
WORKDIR /origem

# Copia primeiro só os arquivos de projeto: o restore fica em cache enquanto as dependências
# não mudarem, mesmo quando o código muda.
COPY Directory.Build.props ./
COPY src/Backend/ControleFinanceiro.Dominio/ControleFinanceiro.Dominio.csproj                       src/Backend/ControleFinanceiro.Dominio/
COPY src/Backend/ControleFinanceiro.Aplicacao/ControleFinanceiro.Aplicacao.csproj                   src/Backend/ControleFinanceiro.Aplicacao/
COPY src/Backend/ControleFinanceiro.Infraestrutura.Dados/ControleFinanceiro.Infraestrutura.Dados.csproj src/Backend/ControleFinanceiro.Infraestrutura.Dados/
COPY src/Backend/ControleFinanceiro.Api/ControleFinanceiro.Api.csproj                               src/Backend/ControleFinanceiro.Api/

RUN dotnet restore src/Backend/ControleFinanceiro.Api/ControleFinanceiro.Api.csproj

COPY src/Backend/ ./src/Backend/

RUN dotnet publish src/Backend/ControleFinanceiro.Api/ControleFinanceiro.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /aplicacao

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS execucao
WORKDIR /aplicacao

# Render e Cloud Run injetam a porta em PORT, que o Program.cs respeita. Este 8080 é só o
# padrão de quem roda a imagem na mão, sem plataforma nenhuma.
ENV ASPNETCORE_HTTP_PORTS=8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

# Em produção o banco já existe e o usuário da aplicação não cria banco.
ENV CONTROLEFINANCEIRO_CRIAR_BANCO_SE_NAO_EXISTIR=false

# Fuso do Brasil disponível no contêiner, para o "hoje" das regras de data.
ENV TZ=America/Sao_Paulo

# Não roda como root.
USER $APP_UID

COPY --from=compilacao /aplicacao ./

ENTRYPOINT ["dotnet", "ControleFinanceiro.Api.dll"]
