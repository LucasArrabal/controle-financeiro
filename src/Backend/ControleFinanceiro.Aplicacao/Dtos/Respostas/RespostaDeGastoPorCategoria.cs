namespace ControleFinanceiro.Aplicacao.Dtos.Respostas;

/// <summary>
/// Uma fatia do gráfico de pizza. <see cref="TetoDefinido"/> nulo significa categoria
/// sem teto no mês — a tela mostra a linha, mas sem barra de progresso.
/// </summary>
public sealed record RespostaDeGastoPorCategoria(
    Guid CategoriaId,
    string NomeDaCategoria,
    string CorHexadecimal,
    decimal ValorGasto,
    decimal PercentualDoTotalGasto,
    decimal? TetoDefinido,
    decimal? PercentualDoTetoConsumido,
    string? SituacaoDoTeto);
