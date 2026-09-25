using ControleFinanceiro.Dominio.Entidades;
using ControleFinanceiro.Dominio.ObjetosDeValor;

namespace ControleFinanceiro.Aplicacao.Contratos.Repositorios;

public interface IRepositorioDeTetosDeGasto
{
    Task<IReadOnlyList<TetoDeGastoMensal>> ListarPorCompetenciaAsync(
        Guid usuarioId,
        CompetenciaMensal competencia,
        CancellationToken cancelamento);

    /// <summary>
    /// Aplica de uma vez, numa transação, o estado dos tetos de um mês:
    /// <paramref name="tetos"/> são gravados (inserido ou atualizado conforme já existam) e
    /// <paramref name="categoriasSemTeto"/> têm o teto removido — que é diferente de teto zero,
    /// pois zero significa "nenhum gasto permitido" e ausência significa "sem limite definido".
    /// </summary>
    Task SalvarEmLoteAsync(
        Guid usuarioId,
        CompetenciaMensal competencia,
        IReadOnlyCollection<TetoDeGastoMensal> tetos,
        IReadOnlyCollection<Guid> categoriasSemTeto,
        CancellationToken cancelamento);
}
