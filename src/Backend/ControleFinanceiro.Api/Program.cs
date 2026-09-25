using ControleFinanceiro.Api.Configuracao;
using ControleFinanceiro.Api.Middlewares;
using ControleFinanceiro.Aplicacao;
using ControleFinanceiro.Infraestrutura.Dados;
using ControleFinanceiro.Infraestrutura.Dados.Migracoes;

var construtor = WebApplication.CreateBuilder(args);

const string ChaveDaStringDeConexao = "CONTROLEFINANCEIRO_STRING_DE_CONEXAO";
const string ChaveDeSementeDeExemplo = "CONTROLEFINANCEIRO_SEMEAR_DADOS_DE_EXEMPLO";
const string ChaveDeMigracoesNoStart = "CONTROLEFINANCEIRO_APLICAR_MIGRACOES_NO_START";
const string ChaveDeCriacaoDoBanco = "CONTROLEFINANCEIRO_CRIAR_BANCO_SE_NAO_EXISTIR";

// Render e Cloud Run escolhem a porta e a informam em PORT; localmente quem manda é o
// launchSettings. Sem isto o contêiner sobe numa porta que a plataforma não observa.
var portaDaPlataforma = construtor.Configuration["PORT"];

if (!string.IsNullOrWhiteSpace(portaDaPlataforma))
{
    construtor.WebHost.UseUrls($"http://0.0.0.0:{portaDaPlataforma}");
}

var stringDeConexao = construtor.Configuration[ChaveDaStringDeConexao];

if (string.IsNullOrWhiteSpace(stringDeConexao))
{
    throw new InvalidOperationException(
        $"A variável de ambiente {ChaveDaStringDeConexao} não foi definida. " +
        "Exemplo: Host=localhost;Port=5432;Database=controle_financeiro;Username=controle;Password=...");
}

construtor.Services.AddControllers();
construtor.Services.AddProblemDetails();
construtor.Services.AdicionarDocumentacaoOpenApi();
construtor.Services.AdicionarCorsDoFrontend(construtor.Configuration);
construtor.Services.AdicionarCasosDeUso();
construtor.Services.AdicionarPersistenciaPostgres(stringDeConexao);

var aplicacao = construtor.Build();

if (construtor.Configuration.GetValue(ChaveDeMigracoesNoStart, defaultValue: true))
{
    ExecutorDeMigracoes.Executar(
        stringDeConexao,
        semearDadosDeExemplo: construtor.Configuration.GetValue(ChaveDeSementeDeExemplo, defaultValue: false),
        // Num Postgres gerenciado (Neon, por exemplo) o banco já vem criado e o usuário da
        // aplicação costuma não ter permissão de `create database`. Lá isto entra como false.
        criarBancoSeNaoExistir: construtor.Configuration.GetValue(ChaveDeCriacaoDoBanco, defaultValue: true),
        registrador: aplicacao.Services.GetRequiredService<ILoggerFactory>()
            .CreateLogger(nameof(ExecutorDeMigracoes)));
}

// Primeiro da fila: qualquer exceção abaixo dele vira ProblemDetails.
aplicacao.UseMiddleware<TratamentoGlobalDeErrosMiddleware>();

aplicacao.UseCors(ConfiguracaoDeCors.NomeDaPolitica);
aplicacao.UsarSwaggerEmDesenvolvimento();

aplicacao.MapControllers();
aplicacao.MapHealthChecks("/health");

aplicacao.Run();

/// <summary>Exposta para que os testes de integração possam hospedar a API.</summary>
public partial class Program;
