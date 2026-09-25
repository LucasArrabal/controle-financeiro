namespace ControleFinanceiro.Aplicacao.CasosDeUso.TetosDeGasto.CopiarTetosDoMesAnterior;

/// <summary><paramref name="Competencia"/> é o mês de destino, no formato <c>aaaa-MM</c>.</summary>
public sealed record CopiarTetosDoMesAnteriorComando(string Competencia);
