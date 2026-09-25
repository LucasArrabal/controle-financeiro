using ControleFinanceiro.Aplicacao.Contratos.Repositorios;
using ControleFinanceiro.Dominio.Entidades;
using ControleFinanceiro.Dominio.ObjetosDeValor;
using ControleFinanceiro.Infraestrutura.Dados.Persistencia.ComandosSql;
using ControleFinanceiro.Infraestrutura.Dados.Persistencia.Conexao;
using ControleFinanceiro.Infraestrutura.Dados.Persistencia.Repositorios.Registros;
using Dapper;

namespace ControleFinanceiro.Infraestrutura.Dados.Persistencia.Repositorios;

public sealed class RepositorioDeDespesas : IRepositorioDeDespesas
{
    private readonly IFabricaDeConexaoPostgres _fabricaDeConexao;

    public RepositorioDeDespesas(IFabricaDeConexaoPostgres fabricaDeConexao) =>
        _fabricaDeConexao = fabricaDeConexao;

    public async Task<IReadOnlyList<Despesa>> ListarPorCompetenciaAsync(
        Guid usuarioId,
        CompetenciaMensal competencia,
        CancellationToken cancelamento)
    {
        await using var conexao = await _fabricaDeConexao.AbrirConexaoAsync(cancelamento);

        var registros = await conexao.QueryAsync<RegistroDeDespesa>(
            new CommandDefinition(
                ComandosSqlDeDespesa.ListarPorCompetencia,
                new
                {
                    UsuarioId = usuarioId,
                    PrimeiroDia = competencia.PrimeiroDia,
                    UltimoDia = competencia.UltimoDia
                },
                cancellationToken: cancelamento));

        return registros.Select(Reidratar).ToList();
    }

    public async Task<Despesa?> ObterPorIdAsync(
        Guid usuarioId,
        Guid despesaId,
        CancellationToken cancelamento)
    {
        await using var conexao = await _fabricaDeConexao.AbrirConexaoAsync(cancelamento);

        var registro = await conexao.QuerySingleOrDefaultAsync<RegistroDeDespesa>(
            new CommandDefinition(
                ComandosSqlDeDespesa.ObterPorId,
                new { UsuarioId = usuarioId, Id = despesaId },
                cancellationToken: cancelamento));

        return registro is null ? null : Reidratar(registro);
    }

    public async Task InserirAsync(Despesa despesa, CancellationToken cancelamento)
    {
        await using var conexao = await _fabricaDeConexao.AbrirConexaoAsync(cancelamento);

        await conexao.ExecuteAsync(
            new CommandDefinition(
                ComandosSqlDeDespesa.Inserir,
                new
                {
                    despesa.Id,
                    despesa.UsuarioId,
                    despesa.Descricao,
                    Valor = despesa.Valor.Quantia,
                    despesa.DataDoGasto,
                    despesa.CategoriaId,
                    despesa.FormaDePagamento,
                    despesa.Observacao,
                    CriadoEm = despesa.CriadoEm.UtcDateTime
                },
                cancellationToken: cancelamento));
    }

    public async Task AtualizarAsync(Despesa despesa, CancellationToken cancelamento)
    {
        await using var conexao = await _fabricaDeConexao.AbrirConexaoAsync(cancelamento);

        await conexao.ExecuteAsync(
            new CommandDefinition(
                ComandosSqlDeDespesa.Atualizar,
                new
                {
                    despesa.Id,
                    despesa.UsuarioId,
                    despesa.Descricao,
                    Valor = despesa.Valor.Quantia,
                    despesa.DataDoGasto,
                    despesa.CategoriaId,
                    despesa.FormaDePagamento,
                    despesa.Observacao
                },
                cancellationToken: cancelamento));
    }

    public async Task ExcluirAsync(Guid usuarioId, Guid despesaId, CancellationToken cancelamento)
    {
        await using var conexao = await _fabricaDeConexao.AbrirConexaoAsync(cancelamento);

        await conexao.ExecuteAsync(
            new CommandDefinition(
                ComandosSqlDeDespesa.Excluir,
                new { UsuarioId = usuarioId, Id = despesaId },
                cancellationToken: cancelamento));
    }

    private static Despesa Reidratar(RegistroDeDespesa registro) =>
        Despesa.Restaurar(
            registro.Id,
            registro.UsuarioId,
            registro.Descricao,
            registro.Valor,
            registro.DataDoGasto,
            registro.CategoriaId,
            registro.FormaDePagamento,
            registro.Observacao,
            new DateTimeOffset(DateTime.SpecifyKind(registro.CriadoEm, DateTimeKind.Utc)));
}
