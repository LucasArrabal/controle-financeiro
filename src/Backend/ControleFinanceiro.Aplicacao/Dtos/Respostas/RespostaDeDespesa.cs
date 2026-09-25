namespace ControleFinanceiro.Aplicacao.Dtos.Respostas;

/// <summary>
/// Despesa como a API a devolve, já com nome e cor da categoria para a tabela do mês
/// não precisar de uma segunda chamada.
/// </summary>
public sealed record RespostaDeDespesa(
    Guid Id,
    string Descricao,
    decimal Valor,
    DateOnly DataDoGasto,
    Guid CategoriaId,
    string NomeDaCategoria,
    string CorDaCategoria,
    string? FormaDePagamento,
    string? Observacao,
    DateTimeOffset CriadoEm);
