using ControleFinanceiro.Dominio.ObjetosDeValor;
using FluentValidation;

namespace ControleFinanceiro.Aplicacao.CasosDeUso.TetosDeGasto.DefinirTetosDoMes;

public sealed class DefinirTetosDoMesValidador : AbstractValidator<DefinirTetosDoMesComando>
{
    public DefinirTetosDoMesValidador()
    {
        RuleFor(comando => comando.Competencia)
            .Must(texto => CompetenciaMensal.TentarAnalisar(texto, out _))
            .WithMessage($"Competência deve estar no formato {CompetenciaMensal.Formato} (ex.: 2026-09).");

        RuleFor(comando => comando.Limites)
            .NotNull().WithMessage("Informe os tetos a gravar.");

        RuleForEach(comando => comando.Limites).ChildRules(limite =>
        {
            limite.RuleFor(item => item.CategoriaId)
                .NotEmpty().WithMessage("Categoria do teto é obrigatória.");

            // Zero é um teto válido ("nenhum gasto permitido"); negativo, não.
            limite.RuleFor(item => item.ValorLimite)
                .GreaterThanOrEqualTo(0m)
                .When(item => item.ValorLimite.HasValue)
                .WithMessage("Teto de gasto não pode ser negativo.");

            limite.RuleFor(item => item.ValorLimite)
                .LessThanOrEqualTo(ValorMonetario.LimiteMaximo)
                .When(item => item.ValorLimite.HasValue)
                .WithMessage($"Teto de gasto não pode ultrapassar {ValorMonetario.LimiteMaximo:N2}.");
        });

        RuleFor(comando => comando.Limites)
            .Must(limites => limites.Select(limite => limite.CategoriaId).Distinct().Count() == limites.Count)
            .When(comando => comando.Limites is not null)
            .WithMessage("A mesma categoria aparece mais de uma vez.");
    }
}
