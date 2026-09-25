namespace ControleFinanceiro.Aplicacao.Dtos.Requisicoes;

/// <summary>
/// Corpo do PUT em lote de tetos: o estado da tela inteira num mês.
/// Categoria com <see cref="TetoDeCategoria.ValorLimite"/> nulo tem o teto removido.
/// </summary>
public sealed record RequisicaoDeTetosDoMes(
    string Competencia,
    IReadOnlyList<TetoDeCategoria> Tetos);

public sealed record TetoDeCategoria(Guid CategoriaId, decimal? ValorLimite);
