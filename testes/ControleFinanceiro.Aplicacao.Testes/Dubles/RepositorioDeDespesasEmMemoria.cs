using ControleFinanceiro.Aplicacao.Contratos.Repositorios;
using ControleFinanceiro.Dominio.Entidades;
using ControleFinanceiro.Dominio.ObjetosDeValor;

namespace ControleFinanceiro.Aplicacao.Testes.Dubles;

internal sealed class RepositorioDeDespesasEmMemoria : IRepositorioDeDespesas
{
    private readonly List<Despesa> _despesas = [];

    public IReadOnlyList<Despesa> Gravadas => _despesas;

    public Task<IReadOnlyList<Despesa>> ListarPorCompetenciaAsync(
        Guid usuarioId,
        CompetenciaMensal competencia,
        CancellationToken cancelamento) =>
        Task.FromResult<IReadOnlyList<Despesa>>(
            _despesas
                .Where(despesa => despesa.UsuarioId == usuarioId)
                .Where(despesa => despesa.Competencia.Equals(competencia))
                .OrderByDescending(despesa => despesa.DataDoGasto)
                .ToList());

    public Task<Despesa?> ObterPorIdAsync(
        Guid usuarioId,
        Guid despesaId,
        CancellationToken cancelamento) =>
        Task.FromResult(
            _despesas.SingleOrDefault(
                despesa => despesa.UsuarioId == usuarioId && despesa.Id == despesaId));

    public Task InserirAsync(Despesa despesa, CancellationToken cancelamento)
    {
        _despesas.Add(despesa);
        return Task.CompletedTask;
    }

    public Task AtualizarAsync(Despesa despesa, CancellationToken cancelamento) =>
        Task.CompletedTask;

    public Task ExcluirAsync(Guid usuarioId, Guid despesaId, CancellationToken cancelamento)
    {
        _despesas.RemoveAll(despesa => despesa.UsuarioId == usuarioId && despesa.Id == despesaId);
        return Task.CompletedTask;
    }
}
