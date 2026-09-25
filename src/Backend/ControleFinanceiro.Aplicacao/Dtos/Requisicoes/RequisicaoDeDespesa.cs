namespace ControleFinanceiro.Aplicacao.Dtos.Requisicoes;

/// <summary>Corpo dos POST e PUT de despesa. O id vem da rota, nunca do corpo.</summary>
public sealed record RequisicaoDeDespesa(
    string Descricao,
    decimal Valor,
    DateOnly DataDoGasto,
    Guid CategoriaId,
    string? FormaDePagamento,
    string? Observacao);
