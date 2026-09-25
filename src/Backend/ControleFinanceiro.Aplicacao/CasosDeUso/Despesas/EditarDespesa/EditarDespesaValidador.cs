using ControleFinanceiro.Aplicacao.Contratos;
using ControleFinanceiro.Aplicacao.Validacao;
using FluentValidation;

namespace ControleFinanceiro.Aplicacao.CasosDeUso.Despesas.EditarDespesa;

public sealed class EditarDespesaValidador : AbstractValidator<EditarDespesaComando>
{
    public EditarDespesaValidador(IProvedorDeDataHora provedorDeDataHora)
    {
        RuleFor(comando => comando.Id)
            .NotEmpty().WithMessage("Id da despesa é obrigatório.");

        RuleFor(comando => comando.Descricao).DescricaoDeLancamentoValida();
        RuleFor(comando => comando.Valor).ValorDeLancamentoValido();
        RuleFor(comando => comando.DataDoGasto).DataDoGastoValida(provedorDeDataHora.HojeNoFusoDoUsuario);
        RuleFor(comando => comando.CategoriaId).CategoriaInformada();
        RuleFor(comando => comando.FormaDePagamento).FormaDePagamentoValida();
    }
}
