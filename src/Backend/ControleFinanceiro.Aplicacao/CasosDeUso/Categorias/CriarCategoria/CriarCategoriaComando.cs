namespace ControleFinanceiro.Aplicacao.CasosDeUso.Categorias.CriarCategoria;

/// <summary><paramref name="Tipo"/> chega como texto e é convertido para TipoDeCategoria no validador.</summary>
public sealed record CriarCategoriaComando(
    string Nome,
    string Tipo,
    string CorHexadecimal);
