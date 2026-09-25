using ControleFinanceiro.Infraestrutura.Dados.Persistencia.Conexao;
using Dapper;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ControleFinanceiro.Infraestrutura.Dados.Persistencia.Diagnostico;

/// <summary>
/// Abre uma conexão de verdade e roda um <c>select 1</c>. Um /health que só responde 200
/// sem tocar no banco não serve para nada num deploy com banco serverless.
/// </summary>
public sealed class VerificacaoDeSaudeDoBanco : IHealthCheck
{
    private readonly IFabricaDeConexaoPostgres _fabricaDeConexao;

    public VerificacaoDeSaudeDoBanco(IFabricaDeConexaoPostgres fabricaDeConexao) =>
        _fabricaDeConexao = fabricaDeConexao;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext contexto,
        CancellationToken cancelamento = default)
    {
        try
        {
            await using var conexao = await _fabricaDeConexao.AbrirConexaoAsync(cancelamento);

            await conexao.ExecuteScalarAsync<int>(
                new CommandDefinition("select 1;", cancellationToken: cancelamento));

            return HealthCheckResult.Healthy("PostgreSQL respondeu.");
        }
        catch (Exception excecao)
        {
            return HealthCheckResult.Unhealthy("PostgreSQL não respondeu.", excecao);
        }
    }
}
