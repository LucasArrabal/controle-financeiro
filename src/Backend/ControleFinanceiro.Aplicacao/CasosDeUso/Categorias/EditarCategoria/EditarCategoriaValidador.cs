using ControleFinanceiro.Aplicacao.Validacao;
using FluentValidation;

namespace ControleFinanceiro.Aplicacao.CasosDeUso.Categorias.EditarCategoria;

public sealed class EditarCategoriaValidador : AbstractValidator<EditarCategoriaComando>
{
    public EditarCategoriaValidador()
    {
        RuleFor(comando => comando.Id)
            .NotEmpty().WithMessage("Id da categoria é obrigatório.");

        RuleFor(comando => comando.Nome).NomeDeCategoriaValido();
        RuleFor(comando => comando.Tipo).TipoDeCategoriaValido();
        RuleFor(comando => comando.CorHexadecimal).CorHexadecimalValida();
    }
}
