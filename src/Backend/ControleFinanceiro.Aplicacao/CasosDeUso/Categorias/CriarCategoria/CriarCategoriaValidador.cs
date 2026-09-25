using ControleFinanceiro.Aplicacao.Validacao;
using FluentValidation;

namespace ControleFinanceiro.Aplicacao.CasosDeUso.Categorias.CriarCategoria;

public sealed class CriarCategoriaValidador : AbstractValidator<CriarCategoriaComando>
{
    public CriarCategoriaValidador()
    {
        RuleFor(comando => comando.Nome).NomeDeCategoriaValido();
        RuleFor(comando => comando.Tipo).TipoDeCategoriaValido();
        RuleFor(comando => comando.CorHexadecimal).CorHexadecimalValida();
    }
}
