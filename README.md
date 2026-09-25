# Controle Financeiro

Aplicação de controle financeiro pessoal: lançar gastos por categoria, registrar entradas,
definir um teto por categoria e ver num painel para onde o dinheiro do mês está indo.

> **Estado atual: em produção.** Cinco telas de pé — despesas, receitas, categorias, tetos por
> categoria e painel do mês — sobre o backend completo, com migrations, Swagger, health check e
> 202 testes de unidade. Publicado em Neon + Render + Vercel; veja [Deploy](#deploy).

---

## Arquitetura

Clean Architecture em projetos separados, com a dependência sempre apontando para dentro:

```
Api  →  Aplicacao  →  Dominio
                 ↖
        Infraestrutura.Dados  (implementa os contratos da Aplicacao)
```

| Projeto | Papel | Pode referenciar |
|---|---|---|
| `ControleFinanceiro.Dominio` | Entidades, objetos de valor e regras de negócio. C# puro. | **nada** |
| `ControleFinanceiro.Aplicacao` | Casos de uso, contratos de repositório, DTOs, validadores. | `Dominio` |
| `ControleFinanceiro.Infraestrutura.Dados` | Dapper, Npgsql, migrations, relógio, usuário atual. | `Aplicacao` |
| `ControleFinanceiro.Api` | Controllers, middleware de erros, CORS, Swagger, DI. | `Aplicacao`, `Infraestrutura.Dados` |

A regra de dependência é verificável: `ControleFinanceiro.Dominio.csproj` não tem nenhum
`PackageReference` nem `ProjectReference`. Se algum aparecer lá, a regra foi quebrada.

Nenhuma entidade de domínio cruza a fronteira HTTP: entrada e saída são sempre DTOs
(`Dtos/Requisicoes` e `Dtos/Respostas`).

---

## Pré-requisitos

| Ferramenta | Versão |
|---|---|
| .NET SDK | 10.0 |
| Docker + Docker Compose | qualquer versão atual |
| Node.js + npm | 20 LTS ou superior |

---

## Como subir local

```bash
docker compose up -d
```

Sobe o PostgreSQL 16 em `localhost:5432` (banco `controle_financeiro`, usuário `controle`).

### Sem Docker

O Docker Desktop exige virtualização por hardware (VT-x / AMD-V) ligada na UEFI. Onde isso não
estiver disponível, um PostgreSQL 16 instalado direto no Windows serve igual — basta criar a
mesma role e o mesmo banco que o compose criaria:

```bash
winget install --id PostgreSQL.PostgreSQL.16 --silent --custom "--superpassword controle_local --serverport 5432"
```

```sql
create role controle with login createdb password 'controle_local';
create database controle_financeiro owner controle;
```

A string de conexão é a mesma, então nada mais no projeto muda.

```bash
dotnet run --project src/Backend/ControleFinanceiro.Api
```

A API sobe em `https://localhost:5001` e abre o Swagger em `https://localhost:5001/swagger`.

Na primeira execução o certificado de desenvolvimento pode não estar confiável. Resolva com:

```bash
dotnet dev-certs https --trust
```

Isso é obrigatório: sem o certificado confiável o navegador bloqueia as chamadas do frontend
para a API e a tela abre vazia.

Por fim, o frontend:

```bash
cd src/Frontend/controle-financeiro-web && npm start
```

Aplicação em `http://localhost:4200`.

---

## Frontend

Angular 22 com componentes standalone, signals e Angular Material. A pasta `src/app` segue o
mesmo critério do backend — o nome diz o que tem dentro:

| Pasta | O que vive ali |
|---|---|
| `nucleo/` | Infraestrutura transversal: URL da API, interceptador de erros HTTP, formatadores |
| `compartilhado/` | Modelos, serviços e componentes usados por mais de uma funcionalidade |
| `funcionalidades/` | Uma pasta por tela, com seus componentes e serviços próprios |
| `layout/` | Barra superior, trilha lateral do desktop e navegação inferior do celular |

Pontos que valem saber:

- **Dois pontos de quebra, cada um com sua decisão.** `ObservadorDeTamanhoDeTela` expõe
  `ehTelaEstreita` (até 899px → navegação inferior, botão flutuante, formulário recolhido) e
  `comportaTabela` (a partir de 1200px → tabela; abaixo disso, lista tocável). São limites
  diferentes de propósito: onde a navegação muda de lugar não é onde a tabela deixa de caber.
  Nenhum componente mede a janela por conta própria.
- **Claro e escuro sem trabalho extra.** Toda cor vem das variáveis `--mat-sys-*` do tema
  Material 3, e o `color-scheme: light dark` segue a preferência do sistema.
- **A competência é global.** `EstadoDaCompetencia` é um serviço de raiz com signals; trocar o mês
  na barra do topo recarrega a tela ativa por um `effect`, e vai valer igual para receitas, tetos
  e painel.
- **Categorias não são exclusivas de uma tela.** A tela `/categorias` cria, edita, desativa
  (soft delete) e reativa — reaproveitando o `ServicoDeCategorias` que já existia desde a Fase 2
  para alimentar os seletores de despesa/receita/teto. Nenhuma duplicação de estado: criar uma
  categoria ali atualiza o cache que as outras telas já leem.
- **Erros da API viram aviso.** `interceptador-de-erros-http.ts` lê o ProblemDetails e mostra a
  mensagem num snackbar. O erro continua subindo, então o formulário não se limpa quando a
  gravação falha.
- **Valor aceita os dois formatos.** `1.234,56` e `1234.56` chegam ao mesmo número.
  Ponto sozinho com três dígitos depois é separador de milhar; qualquer outra coisa é decimal.
- **Data é data pura.** A conversão entre a `Date` do datepicker e o `aaaa-MM-dd` da API passa
  longe de `toISOString()`, que devolveria o dia anterior de madrugada no fuso de São Paulo.
- **URL da API por ambiente.** `environment.ts` no desenvolvimento; em produção o build troca pelo
  `environment.production.ts`, cujo `__URL_DA_API__` o deploy substitui pelo endereço real.

### Comandos

```bash
npm start          # servidor de desenvolvimento em http://localhost:4200
npm run build      # build de produção em dist/
npm test           # testes unitários
```

---

## Migrations

As migrations rodam **automaticamente no start da API** (DbUp). Não há comando separado.

- Os scripts ficam em `src/Backend/ControleFinanceiro.Infraestrutura.Dados/Migracoes/Scripts/`
  e são compilados como recursos embutidos — o binário publicado carrega as migrações consigo.
- DbUp controla o que já foi aplicado na tabela `schemaversions`.
- Para adicionar uma migration, crie um `.sql` novo com o próximo número (`006_...`).
  Ele é incluído automaticamente pelo glob do `.csproj`.
- Para desligar a execução no start: `CONTROLEFINANCEIRO_APLICAR_MIGRACOES_NO_START=false`.

### Dados de exemplo

Com `CONTROLEFINANCEIRO_SEMEAR_DADOS_DE_EXEMPLO=true` (já ligado no perfil de desenvolvimento),
o start também insere 20 despesas, 3 receitas e 4 tetos **no mês corrente**, para o painel já
abrir preenchido. O script não semeia duas vezes o mesmo mês e vive numa pasta separada
(`Migracoes/ScriptsDeExemplo/`), fora do caminho das migrations de esquema.

---

## Variáveis de ambiente

Connection string e segredos vêm **só** de variável de ambiente — nada disso é versionado.

| Variável | Obrigatória | Padrão | Para quê |
|---|---|---|---|
| `CONTROLEFINANCEIRO_STRING_DE_CONEXAO` | sim | — | Conexão Npgsql com o PostgreSQL |
| `CONTROLEFINANCEIRO_ORIGENS_PERMITIDAS` | não | `http://localhost:4200` | Origens liberadas no CORS, separadas por vírgula |
| `CONTROLEFINANCEIRO_SEMEAR_DADOS_DE_EXEMPLO` | não | `false` | Insere os lançamentos de demonstração |
| `CONTROLEFINANCEIRO_APLICAR_MIGRACOES_NO_START` | não | `true` | Aplica as migrations ao subir a API |
| `CONTROLEFINANCEIRO_CRIAR_BANCO_SE_NAO_EXISTIR` | não | `true` | Cria o banco caso não exista. **`false` em Postgres gerenciado**, onde o banco já vem pronto e o usuário não tem `create database` |
| `PORT` | não | — | Injetada por Render e Cloud Run; quando presente, manda na porta da API |

Para desenvolvimento local, o perfil de execução em
`src/Backend/ControleFinanceiro.Api/Properties/launchSettings.json` já define essas variáveis
apontando para o container do `docker-compose` — por isso `dotnet run` funciona direto.
Esse arquivo só vale para `dotnet run` local e nunca vai para a imagem publicada.

Fora do desenvolvimento, defina você mesmo:

```powershell
$env:CONTROLEFINANCEIRO_STRING_DE_CONEXAO = "Host=...;Port=5432;Database=...;Username=...;Password=...;SSL Mode=Require"
```

---

## Endpoints disponíveis nesta fase

```
GET    /api/v1/categorias?incluirInativas=false
POST   /api/v1/categorias
PUT    /api/v1/categorias/{id}
DELETE /api/v1/categorias/{id}          # desativa (soft delete)

GET    /api/v1/despesas?competencia=2026-09
POST   /api/v1/despesas
PUT    /api/v1/despesas/{id}
DELETE /api/v1/despesas/{id}

GET    /api/v1/receitas?competencia=2026-09
POST   /api/v1/receitas
PUT    /api/v1/receitas/{id}
DELETE /api/v1/receitas/{id}
POST   /api/v1/receitas/{id}/replicar?meses=11   # copia a recorrente nos próximos meses

GET    /api/v1/tetos-de-gasto?competencia=2026-09
PUT    /api/v1/tetos-de-gasto                    # upsert em lote
POST   /api/v1/tetos-de-gasto/copiar-do-mes-anterior?competencia=2026-09

GET    /api/v1/painel/resumo-mensal?competencia=2026-09

GET    /health
```

`competencia` é sempre `aaaa-MM`.

Erros saem em **ProblemDetails (RFC 7807)**:

| Situação | HTTP |
|---|---|
| Campo inválido (FluentValidation) | `400` com a lista de erros por campo |
| Id inexistente | `404` |
| Regra de negócio violada | `422` |
| Erro inesperado | `500` — sem stack trace em produção |

---

## Testes

```bash
dotnet test
```

202 testes em dois projetos, com xUnit e FluentAssertions:

| Projeto | Cobre |
|---|---|
| `testes/ControleFinanceiro.Dominio.Testes` | Situação do teto nos limites exatos (80% e 100%), percentuais com total zero, `ValorMonetario`, `CompetenciaMensal` e as invariantes das quatro entidades |
| `testes/ControleFinanceiro.Aplicacao.Testes` | Validadores de comando, cálculo do painel, replicação de recorrente e gravação em lote dos tetos |

Os dublês ficam em `Dubles/` e são escritos à mão — repositórios guardados numa lista, relógio
parado — para o teste ler como cenário em vez de configuração de mock. Não há dependência de
biblioteca de mocking.

Nada aqui toca o banco: são testes de unidade e rodam em menos de meio segundo.

```bash
cd src/Frontend/controle-financeiro-web && npm test
```

---

## Deploy

Tudo está preparado, mas nada foi publicado. O plano é o do briefing:

| Peça | Serviço | Arquivo que prepara |
|---|---|---|
| Banco | Neon (Postgres serverless) | — só a string de conexão |
| API | Render (Docker, free) | [`render.yaml`](render.yaml) + [`Dockerfile`](src/Backend/ControleFinanceiro.Api/Dockerfile) |
| Frontend | Vercel | [`vercel.json`](src/Frontend/controle-financeiro-web/vercel.json) |

> A Vercel não executa .NET — ela serve só o frontend estático. A API vai para o Render
> (ou Cloud Run, com o mesmo Dockerfile).

### 1. Banco na Neon

Crie um projeto e um banco `controle_financeiro`. Pegue a string de conexão **pooled** e
converta para o formato do Npgsql:

```
Host=ep-xxx-pooler.sa-east-1.aws.neon.tech;Database=controle_financeiro;Username=...;Password=...;SSL Mode=Require;Trust Server Certificate=true
```

Não é preciso rodar migration à mão: a API aplica as pendentes ao subir.

### 2. API no Render

No Render, **New + → Blueprint**, apontando para este repositório. O `render.yaml` define
o serviço, o health check em `/health` e as variáveis. As duas marcadas com `sync: false`
o Render pergunta na primeira vez e guarda cifradas:

- `CONTROLEFINANCEIRO_STRING_DE_CONEXAO` — a da Neon, do passo 1
- `CONTROLEFINANCEIRO_ORIGENS_PERMITIDAS` — o domínio da Vercel, sem barra no fim

O plano free hiberna após inatividade: a primeira chamada depois de um tempo parado leva
uns 50 segundos. Como as migrations rodam no start, esse cold start inclui aplicá-las.

### 3. Frontend na Vercel

Importe o repositório com **Root Directory** `src/Frontend/controle-financeiro-web`.
O `vercel.json` já define o build e o rewrite de SPA — sem ele, recarregar `/painel`
direto no navegador daria 404.

Defina uma variável de ambiente no projeto da Vercel:

```
URL_DA_API = https://controle-financeiro-api.onrender.com/api/v1
```

O `npm run build:producao` compila e roda `scripts/aplicar-url-da-api.mjs`, que troca o
marcador `__URL_DA_API__` do bundle pelo endereço real. O script falha o build se a variável
estiver ausente, malformada ou se o marcador não for encontrado — publicar um bundle sem a
URL certa quebraria toda chamada à API, e é melhor descobrir no build do que em produção.

### 4. Fechar o círculo do CORS

Com o domínio da Vercel em mãos, volte ao Render e ajuste
`CONTROLEFINANCEIRO_ORIGENS_PERMITIDAS`. A API só aceita as origens configuradas.

---

## Decisões que valem saber

- **Datas.** `data_do_gasto` e `data_do_recebimento` são `date` puro: o dia em que o dinheiro
  entrou ou saiu, sem fuso. Só `criado_em` é `timestamptz`, sempre gravado em UTC. Converter
  fuso numa data pura deslocaria o lançamento de dia.
- **Dinheiro.** Sempre `decimal` no C# e `numeric(14,2)` no banco. Nunca `double`/`float`.
- **Multiusuário.** Ainda não há login, mas todas as tabelas já têm `usuario_id`, preenchido com
  o GUID fixo `11111111-1111-1111-1111-111111111111`. Ligar autenticação depois é trocar
  `ProvedorDoUsuarioPadrao` por uma implementação que leia o usuário do token — nenhum SQL e
  nenhum caso de uso muda.
- **Categorias não são apagadas.** `DELETE` desativa. Despesas antigas continuam com nome e cor
  no painel; a categoria desativada some do formulário de lançamento.
- **O painel é uma consulta só.** `ComandosSqlDoResumoMensal` soma receitas, gastos por categoria
  e junta os tetos numa única ida ao banco: nenhuma despesa individual sobe para o C#. Os
  percentuais e a cor de cada barra vêm depois, das calculadoras do domínio — onde as regras moram.
- **Chart.js só é baixado no painel.** As rotas do painel ficam em `rotas-do-painel.ts` justamente
  para isso: registrar o `provideCharts` no `app.config.ts` arrastaria a biblioteca de gráficos
  (~208 kB) para o pacote inicial de quem só abre a tela de despesas.
- **Teto zero não é ausência de teto.** Linha inexistente em `tetos_de_gasto_mensal` significa
  "sem limite definido"; `valor_limite = 0` é um limite de verdade e quer dizer "nenhum gasto
  permitido". Na tela, campo vazio remove o teto e `0` grava zero.
- **Replicar recorrente não duplica.** Meses que já tiverem uma receita com a mesma descrição
  são pulados, e a resposta diz quais foram criados e quais ignorados — rodar duas vezes o mesmo
  "replicar salário" não gera lançamento em dobro.
- **Google Sheets não é o banco.** A interface `IExportadorDeResumoMensal` deixa a porta aberta
  para exportar o resumo de um mês para uma planilha numa fase futura, mas a leitura e a gravação
  do sistema são sempre PostgreSQL.
