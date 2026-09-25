namespace ControleFinanceiro.Aplicacao.Dtos.Respostas;

/// <summary>Receita como a API a devolve.</summary>
public sealed record RespostaDeReceita(
    Guid Id,
    string Descricao,
    decimal Valor,
    DateOnly DataDoRecebimento,
    bool Recorrente,
    DateTimeOffset CriadoEm);
