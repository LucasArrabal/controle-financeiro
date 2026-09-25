namespace ControleFinanceiro.Aplicacao.CasosDeUso.Despesas.EditarDespesa;

public sealed record EditarDespesaComando(
    Guid Id,
    string Descricao,
    decimal Valor,
    DateOnly DataDoGasto,
    Guid CategoriaId,
    string? FormaDePagamento,
    string? Observacao);
