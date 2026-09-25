namespace ControleFinanceiro.Aplicacao.CasosDeUso.Receitas.EditarReceita;

public sealed record EditarReceitaComando(
    Guid Id,
    string Descricao,
    decimal Valor,
    DateOnly DataDoRecebimento,
    bool Recorrente);
