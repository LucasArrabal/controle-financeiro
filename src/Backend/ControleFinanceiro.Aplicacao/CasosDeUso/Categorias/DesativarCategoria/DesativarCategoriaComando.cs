namespace ControleFinanceiro.Aplicacao.CasosDeUso.Categorias.DesativarCategoria;

/// <summary>
/// Soft delete de categoria. Categoria nunca é apagada: ela some do formulário de lançamento,
/// mas os gastos históricos continuam apontando para ela no painel.
/// </summary>
public sealed record DesativarCategoriaComando(Guid Id);
