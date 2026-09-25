namespace ControleFinanceiro.Aplicacao.Dtos.Respostas;

/// <summary>
/// Resultado de replicar uma receita recorrente. As competências ignoradas já tinham um
/// lançamento com a mesma descrição — a replicação não duplica salário já registrado.
/// </summary>
public sealed record RespostaDaReplicacaoDeReceita(
    IReadOnlyList<string> CompetenciasCriadas,
    IReadOnlyList<string> CompetenciasIgnoradas);
