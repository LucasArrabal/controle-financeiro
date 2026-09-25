using ControleFinanceiro.Aplicacao.Contratos.Repositorios;
using ControleFinanceiro.Dominio.Entidades;
using ControleFinanceiro.Dominio.ObjetosDeValor;
using ControleFinanceiro.Infraestrutura.Dados.Persistencia.ComandosSql;
using ControleFinanceiro.Infraestrutura.Dados.Persistencia.Conexao;
using ControleFinanceiro.Infraestrutura.Dados.Persistencia.Repositorios.Registros;
using Dapper;

namespace ControleFinanceiro.Infraestrutura.Dados.Persistencia.Repositorios;

public sealed class RepositorioDeTetosDeGasto : IRepositorioDeTetosDeGasto
{
    private readonly IFabricaDeConexaoPostgres _fabricaDeConexao;

    public RepositorioDeTetosDeGasto(IFabricaDeConexaoPostgres fabricaDeConexao) =>
        _fabricaDeConexao = fabricaDeConexao;

    public async Task<IReadOnlyList<TetoDeGastoMensal>> ListarPorCompetenciaAsync(
        Guid usuarioId,
        CompetenciaMensal competencia,
        CancellationToken cancelamento)
    {
        await using var conexao = await _fabricaDeConexao.AbrirConexaoAsync(cancelamento);

        var registros = await conexao.QueryAsync<RegistroDeTetoDeGasto>(
            new CommandDefinition(
                ComandosSqlDeTetoDeGasto.ListarPorCompetencia,
                new { UsuarioId = usuarioId, Competencia = competencia.ToString() },
                cancellationToken: cancelamento));

        return registros.Select(Reidratar).ToList();
    }

    public async Task SalvarEmLoteAsync(
        Guid usuarioId,
        CompetenciaMensal competencia,
        IReadOnlyCollection<TetoDeGastoMensal> tetos,
        IReadOnlyCollection<Guid> categoriasSemTeto,
        CancellationToken cancelamento)
    {
        if (tetos.Count == 0 && categoriasSemTeto.Count == 0)
        {
            return;
        }

        await using var conexao = await _fabricaDeConexao.AbrirConexaoAsync(cancelamento);
        await using var transacao = await conexao.BeginTransactionAsync(cancelamento);

        // A tela salva o mês inteiro de uma vez: ou vale tudo o que foi digitado, ou nada.
        if (categoriasSemTeto.Count > 0)
        {
            await conexao.ExecuteAsync(
                new CommandDefinition(
                    ComandosSqlDeTetoDeGasto.RemoverDaCategoria,
                    new
                    {
                        UsuarioId = usuarioId,
                        Competencia = competencia.ToString(),
                        CategoriaIds = categoriasSemTeto.ToArray(),
                    },
                    transaction: transacao,
                    cancellationToken: cancelamento));
        }

        if (tetos.Count > 0)
        {
            await conexao.ExecuteAsync(
                new CommandDefinition(
                    ComandosSqlDeTetoDeGasto.Salvar,
                    tetos
                        .Select(teto => new
                        {
                            teto.Id,
                            teto.UsuarioId,
                            teto.CategoriaId,
                            Competencia = teto.Competencia.ToString(),
                            ValorLimite = teto.ValorLimite.Quantia,
                        })
                        .ToList(),
                    transaction: transacao,
                    cancellationToken: cancelamento));
        }

        await transacao.CommitAsync(cancelamento);
    }

    private static TetoDeGastoMensal Reidratar(RegistroDeTetoDeGasto registro) =>
        TetoDeGastoMensal.Restaurar(
            registro.Id,
            registro.UsuarioId,
            registro.CategoriaId,
            CompetenciaMensal.Analisar(registro.Competencia),
            registro.ValorLimite);
}
