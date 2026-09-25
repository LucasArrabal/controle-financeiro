using System.Reflection;
using DbUp;
using DbUp.Engine;
using Microsoft.Extensions.Logging;

namespace ControleFinanceiro.Infraestrutura.Dados.Migracoes;

/// <summary>
/// Aplica os scripts <c>.sql</c> versionados no start da API. Os scripts são recursos
/// embutidos no assembly, então o binário publicado carrega as migrações consigo — não há
/// pasta de scripts para esquecer no deploy.
/// </summary>
public static class ExecutorDeMigracoes
{
    private const string PrefixoDosScriptsDeEsquema =
        "ControleFinanceiro.Infraestrutura.Dados.Migracoes.Scripts.";

    private const string PrefixoDosScriptsDeExemplo =
        "ControleFinanceiro.Infraestrutura.Dados.Migracoes.ScriptsDeExemplo.";

    /// <summary>
    /// Cria o banco se ele não existir e aplica o que ainda não foi aplicado.
    /// <paramref name="semearDadosDeExemplo"/> liga os lançamentos de demonstração — nunca em produção.
    /// </summary>
    public static void Executar(
        string stringDeConexao,
        bool semearDadosDeExemplo,
        bool criarBancoSeNaoExistir,
        ILogger registrador)
    {
        if (criarBancoSeNaoExistir)
        {
            // Esta chamada conecta no banco `postgres` para verificar e criar o banco alvo.
            // Em Postgres gerenciado o banco já existe e o usuário raramente tem essa permissão.
            EnsureDatabase.For.PostgresqlDatabase(stringDeConexao);
        }

        string[] prefixosAceitos = semearDadosDeExemplo
            ? [PrefixoDosScriptsDeEsquema, PrefixoDosScriptsDeExemplo]
            : [PrefixoDosScriptsDeEsquema];

        var migrador = DeployChanges.To
            .PostgresqlDatabase(stringDeConexao)
            .WithScriptsEmbeddedInAssembly(
                Assembly.GetExecutingAssembly(),
                nomeDoRecurso => prefixosAceitos.Any(prefixo =>
                    nomeDoRecurso.StartsWith(prefixo, StringComparison.Ordinal)))
            .WithVariablesDisabled()
            .WithTransactionPerScript()
            .LogToConsole()
            .Build();

        var resultado = migrador.PerformUpgrade();

        if (!resultado.Successful)
        {
            throw new InvalidOperationException(
                $"Falha ao aplicar a migração '{resultado.ErrorScript?.Name}'.",
                resultado.Error);
        }

        RegistrarResultado(resultado, registrador);
    }

    private static void RegistrarResultado(DatabaseUpgradeResult resultado, ILogger registrador)
    {
        var nomesDosScripts = resultado.Scripts.Select(script => script.Name).ToList();

        if (nomesDosScripts.Count == 0)
        {
            registrador.LogInformation("Banco já estava atualizado; nenhuma migração aplicada.");
            return;
        }

        registrador.LogInformation(
            "Migrações aplicadas ({Quantidade}): {Scripts}",
            nomesDosScripts.Count,
            string.Join(", ", nomesDosScripts));
    }
}
