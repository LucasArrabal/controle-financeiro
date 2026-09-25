namespace ControleFinanceiro.Aplicacao.CasosDeUso.Receitas.ExcluirReceita;

/// <summary>Exclusão definitiva — receita não tem soft delete.</summary>
public sealed record ExcluirReceitaComando(Guid Id);
