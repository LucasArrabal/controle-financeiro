using ControleFinanceiro.Dominio.Entidades;
using ControleFinanceiro.Dominio.Enumeradores;
using FluentValidation;

namespace ControleFinanceiro.Aplicacao.Validacao;

/// <summary>
/// Regras de campo compartilhadas entre criar e editar categoria, para as duas telas
/// devolverem exatamente a mesma mensagem de erro.
/// </summary>
public static class RegrasDeValidacaoDeCategoria
{
    public static IRuleBuilderOptions<T, string> NomeDeCategoriaValido<T>(
        this IRuleBuilder<T, string> regra) =>
        regra
            .NotEmpty().WithMessage("Nome da categoria é obrigatório.")
            .MinimumLength(CategoriaDeGasto.TamanhoMinimoDoNome)
            .WithMessage($"Nome da categoria deve ter no mínimo {CategoriaDeGasto.TamanhoMinimoDoNome} caracteres.")
            .MaximumLength(CategoriaDeGasto.TamanhoMaximoDoNome)
            .WithMessage($"Nome da categoria deve ter no máximo {CategoriaDeGasto.TamanhoMaximoDoNome} caracteres.");

    public static IRuleBuilderOptions<T, string> TipoDeCategoriaValido<T>(
        this IRuleBuilder<T, string> regra) =>
        regra
            .NotEmpty().WithMessage("Tipo da categoria é obrigatório.")
            .Must(tipo => Enum.TryParse<TipoDeCategoria>(tipo, ignoreCase: true, out var convertido)
                          && Enum.IsDefined(convertido))
            .WithMessage($"Tipo da categoria deve ser um destes: {string.Join(", ", Enum.GetNames<TipoDeCategoria>())}.");

    public static IRuleBuilderOptions<T, string> CorHexadecimalValida<T>(
        this IRuleBuilder<T, string> regra) =>
        regra
            .NotEmpty().WithMessage("Cor da categoria é obrigatória.")
            .Matches("^#([0-9a-fA-F]{6})$")
            .WithMessage("Cor da categoria deve estar no formato #RRGGBB (ex.: #2E7D32).");
}
