namespace ControleFinanceiro.Aplicacao.CasosDeUso.TetosDeGasto.DefinirTetosDoMes;

/// <summary>
/// Upsert em lote: o estado da tela de tetos num mês.
/// <see cref="LimiteDeCategoria.ValorLimite"/> nulo remove o teto da categoria.
/// </summary>
public sealed record DefinirTetosDoMesComando(
    string Competencia,
    IReadOnlyList<LimiteDeCategoria> Limites);

public sealed record LimiteDeCategoria(Guid CategoriaId, decimal? ValorLimite);
