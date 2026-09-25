namespace ControleFinanceiro.Aplicacao.CasosDeUso.Despesas.ListarDespesasDoMes;

/// <summary><paramref name="Competencia"/> no formato <c>aaaa-MM</c>, ex.: <c>2026-09</c>.</summary>
public sealed record ListarDespesasDoMesConsulta(string Competencia);
