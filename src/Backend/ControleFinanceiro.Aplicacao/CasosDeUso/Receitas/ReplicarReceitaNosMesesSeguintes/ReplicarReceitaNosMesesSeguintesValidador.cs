using FluentValidation;

namespace ControleFinanceiro.Aplicacao.CasosDeUso.Receitas.ReplicarReceitaNosMesesSeguintes;

public sealed class ReplicarReceitaNosMesesSeguintesValidador
    : AbstractValidator<ReplicarReceitaNosMesesSeguintesComando>
{
    public const int QuantidadeMaximaDeMeses = 24;

    public ReplicarReceitaNosMesesSeguintesValidador()
    {
        RuleFor(comando => comando.Id)
            .NotEmpty().WithMessage("Id da receita é obrigatório.");

        RuleFor(comando => comando.QuantidadeDeMeses)
            .InclusiveBetween(1, QuantidadeMaximaDeMeses)
            .WithMessage($"A replicação aceita de 1 a {QuantidadeMaximaDeMeses} meses.");
    }
}
