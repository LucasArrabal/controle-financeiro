namespace ControleFinanceiro.Aplicacao.CasosDeUso.Receitas.ReplicarReceitaNosMesesSeguintes;

/// <summary>
/// Copia uma receita recorrente para os próximos <paramref name="QuantidadeDeMeses"/> meses,
/// mantendo descrição, valor e o dia do recebimento.
/// </summary>
public sealed record ReplicarReceitaNosMesesSeguintesComando(Guid Id, int QuantidadeDeMeses);
