namespace ControleFinanceiro.Aplicacao.CasosDeUso.Categorias.EditarCategoria;

public sealed record EditarCategoriaComando(
    Guid Id,
    string Nome,
    string Tipo,
    string CorHexadecimal,
    bool Ativa);
