using ControleFinanceiro.Dominio.Entidades;
using ControleFinanceiro.Dominio.ObjetosDeValor;

namespace ControleFinanceiro.Aplicacao.Contratos.Repositorios;

public interface IRepositorioDeReceitas
{
    Task<IReadOnlyList<Receita>> ListarPorCompetenciaAsync(
        Guid usuarioId,
        CompetenciaMensal competencia,
        CancellationToken cancelamento);

    Task<Receita?> ObterPorIdAsync(Guid usuarioId, Guid receitaId, CancellationToken cancelamento);

    Task InserirAsync(Receita receita, CancellationToken cancelamento);

    /// <summary>Grava várias receitas de uma vez, numa transação — usado pela replicação do salário.</summary>
    Task InserirVariasAsync(IReadOnlyCollection<Receita> receitas, CancellationToken cancelamento);

    Task AtualizarAsync(Receita receita, CancellationToken cancelamento);

    Task ExcluirAsync(Guid usuarioId, Guid receitaId, CancellationToken cancelamento);

    /// <summary>
    /// Competências, entre as informadas, que já têm alguma receita com a mesma descrição.
    /// A replicação usa isso para não duplicar um salário que já foi lançado.
    /// </summary>
    Task<IReadOnlyList<CompetenciaMensal>> ListarCompetenciasComDescricaoAsync(
        Guid usuarioId,
        string descricao,
        IReadOnlyCollection<CompetenciaMensal> competencias,
        CancellationToken cancelamento);
}
