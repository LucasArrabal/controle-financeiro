namespace ControleFinanceiro.Aplicacao.Dtos.Requisicoes;

/// <summary>
/// Corpo do PUT de categoria. Traz <paramref name="Ativa"/> porque a edição também é o
/// caminho para reativar uma categoria desativada por engano.
/// </summary>
public sealed record RequisicaoDeEdicaoDeCategoria(
    string Nome,
    string Tipo,
    string CorHexadecimal,
    bool Ativa = true);
