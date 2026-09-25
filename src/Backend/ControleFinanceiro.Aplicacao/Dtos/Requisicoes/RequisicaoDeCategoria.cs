namespace ControleFinanceiro.Aplicacao.Dtos.Requisicoes;

/// <summary>Corpo dos POST e PUT de categoria.</summary>
public sealed record RequisicaoDeCategoria(
    string Nome,
    string Tipo,
    string CorHexadecimal);
