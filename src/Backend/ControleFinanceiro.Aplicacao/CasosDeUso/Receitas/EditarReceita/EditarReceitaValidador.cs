using ControleFinanceiro.Aplicacao.Validacao;
using FluentValidation;

namespace ControleFinanceiro.Aplicacao.CasosDeUso.Receitas.EditarReceita;

public sealed class EditarReceitaValidador : AbstractValidator<EditarReceitaComando>
{
    public EditarReceitaValidador()
    {
        RuleFor(comando => comando.Id)
            .NotEmpty().WithMessage("Id da receita é obrigatório.");

        RuleFor(comando => comando.Descricao).DescricaoDeLancamentoValida();
        RuleFor(comando => comando.Valor).ValorDeLancamentoValido();
        RuleFor(comando => comando.DataDoRecebimento).DataInformada("Data do recebimento");
    }
}
