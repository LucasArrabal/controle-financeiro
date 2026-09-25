using ControleFinanceiro.Dominio.Entidades;
using FluentValidation;

namespace ControleFinanceiro.Aplicacao.Validacao;

/// <summary>
/// Regras específicas de despesa. O que vale também para receita mora em
/// <see cref="RegrasDeValidacaoDeLancamento"/>.
/// </summary>
public static class RegrasDeValidacaoDeDespesa
{
    public static IRuleBuilderOptions<T, DateOnly> DataDoGastoValida<T>(
        this IRuleBuilder<T, DateOnly> regra,
        DateOnly hoje)
    {
        var dataMaxima = hoje.AddMonths(Despesa.MesesDeToleranciaNoFuturo);

        return regra
            .NotEqual(default(DateOnly)).WithMessage("Data do gasto é obrigatória.")
            .LessThanOrEqualTo(dataMaxima)
            .WithMessage("Data do gasto não pode estar mais de 1 ano no futuro.");
    }

    public static IRuleBuilderOptions<T, Guid> CategoriaInformada<T>(
        this IRuleBuilder<T, Guid> regra) =>
        regra.NotEmpty().WithMessage("Categoria é obrigatória.");

    public static IRuleBuilderOptions<T, string?> FormaDePagamentoValida<T>(
        this IRuleBuilder<T, string?> regra) =>
        regra
            .MaximumLength(Despesa.TamanhoMaximoDaFormaDePagamento)
            .WithMessage($"Forma de pagamento deve ter no máximo {Despesa.TamanhoMaximoDaFormaDePagamento} caracteres.");
}
