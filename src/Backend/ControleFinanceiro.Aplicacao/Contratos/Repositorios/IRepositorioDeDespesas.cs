using ControleFinanceiro.Dominio.Entidades;
using ControleFinanceiro.Dominio.ObjetosDeValor;

namespace ControleFinanceiro.Aplicacao.Contratos.Repositorios;

public interface IRepositorioDeDespesas
{
    Task<IReadOnlyList<Despesa>> ListarPorCompetenciaAsync(
        Guid usuarioId,
        CompetenciaMensal competencia,
        CancellationToken cancelamento);

    Task<Despesa?> ObterPorIdAsync(Guid usuarioId, Guid despesaId, CancellationToken cancelamento);

    Task InserirAsync(Despesa despesa, CancellationToken cancelamento);

    Task AtualizarAsync(Despesa despesa, CancellationToken cancelamento);

    Task ExcluirAsync(Guid usuarioId, Guid despesaId, CancellationToken cancelamento);
}
