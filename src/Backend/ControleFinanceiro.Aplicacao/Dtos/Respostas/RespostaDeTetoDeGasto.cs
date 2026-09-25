namespace ControleFinanceiro.Aplicacao.Dtos.Respostas;

/// <summary>
/// Uma linha da tela de tetos: sempre uma por categoria ativa, mesmo sem limite definido.
/// <see cref="ValorLimite"/> nulo é "sem teto"; zero é "nenhum gasto permitido".
/// </summary>
public sealed record RespostaDeTetoDeGasto(
    Guid CategoriaId,
    string NomeDaCategoria,
    string CorHexadecimal,
    decimal? ValorLimite);
