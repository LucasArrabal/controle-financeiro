namespace ControleFinanceiro.Aplicacao.CasosDeUso.Despesas.RegistrarDespesa;

public sealed record RegistrarDespesaComando(
    string Descricao,
    decimal Valor,
    DateOnly DataDoGasto,
    Guid CategoriaId,
    string? FormaDePagamento,
    string? Observacao);
