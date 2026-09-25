namespace ControleFinanceiro.Aplicacao.CasosDeUso.Categorias.ListarCategorias;

/// <summary>
/// Lista as categorias do usuário. Por padrão traz só as ativas — o formulário de despesa
/// não deve oferecer categoria desativada.
/// </summary>
public sealed record ListarCategoriasConsulta(bool IncluirInativas = false);
