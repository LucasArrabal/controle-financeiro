namespace ControleFinanceiro.Aplicacao.CasosDeUso.Receitas.RegistrarReceita;

public sealed record RegistrarReceitaComando(
    string Descricao,
    decimal Valor,
    DateOnly DataDoRecebimento,
    bool Recorrente);
