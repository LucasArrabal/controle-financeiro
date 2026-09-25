namespace ControleFinanceiro.Aplicacao.Dtos.Requisicoes;

/// <summary>Corpo dos POST e PUT de receita. O id vem da rota, nunca do corpo.</summary>
public sealed record RequisicaoDeReceita(
    string Descricao,
    decimal Valor,
    DateOnly DataDoRecebimento,
    bool Recorrente);
