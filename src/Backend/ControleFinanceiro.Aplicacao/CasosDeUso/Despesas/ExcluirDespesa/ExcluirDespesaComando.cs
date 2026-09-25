namespace ControleFinanceiro.Aplicacao.CasosDeUso.Despesas.ExcluirDespesa;

/// <summary>Exclusão definitiva — despesa não tem soft delete, um lançamento errado some de vez.</summary>
public sealed record ExcluirDespesaComando(Guid Id);
