namespace ControleFinanceiro.Aplicacao.Dtos.Respostas;

/// <summary>Payload do painel mensal, montado a partir de uma única consulta agregada.</summary>
public sealed record RespostaDoResumoMensal(
    string Competencia,
    decimal TotalDeReceitas,
    decimal TotalDeDespesas,
    decimal Saldo,
    decimal PercentualDaRendaComprometida,
    IReadOnlyList<RespostaDeGastoPorCategoria> GastosPorCategoria);
