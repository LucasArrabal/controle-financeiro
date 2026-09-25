using ControleFinanceiro.Dominio.Entidades;
using ControleFinanceiro.Dominio.ObjetosDeValor;
using FluentValidation;

namespace ControleFinanceiro.Aplicacao.Validacao;

/// <summary>
/// Regras que valem para qualquer lançamento — despesa ou receita — para as duas telas
/// devolverem exatamente a mesma mensagem de erro nos mesmos campos.
/// </summary>
public static class RegrasDeValidacaoDeLancamento
{
    public static IRuleBuilderOptions<T, string> DescricaoDeLancamentoValida<T>(
        this IRuleBuilder<T, string> regra) =>
        regra
            .NotEmpty().WithMessage("Descrição é obrigatória.")
            .MinimumLength(Despesa.TamanhoMinimoDaDescricao)
            .WithMessage($"Descrição deve ter no mínimo {Despesa.TamanhoMinimoDaDescricao} caracteres.")
            .MaximumLength(Despesa.TamanhoMaximoDaDescricao)
            .WithMessage($"Descrição deve ter no máximo {Despesa.TamanhoMaximoDaDescricao} caracteres.");

    public static IRuleBuilderOptions<T, decimal> ValorDeLancamentoValido<T>(
        this IRuleBuilder<T, decimal> regra) =>
        regra
            .GreaterThan(0m).WithMessage("Valor deve ser maior que zero.")
            .LessThanOrEqualTo(ValorMonetario.LimiteMaximo)
            .WithMessage($"Valor não pode ultrapassar {ValorMonetario.LimiteMaximo:N2}.");

    public static IRuleBuilderOptions<T, DateOnly> DataInformada<T>(
        this IRuleBuilder<T, DateOnly> regra,
        string nomeDoCampo) =>
        regra.NotEqual(default(DateOnly)).WithMessage($"{nomeDoCampo} é obrigatória.");
}
