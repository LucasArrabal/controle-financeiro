using ControleFinanceiro.Aplicacao.Contratos;
using ControleFinanceiro.Aplicacao.Validacao;
using FluentValidation;

namespace ControleFinanceiro.Aplicacao.CasosDeUso.Despesas.RegistrarDespesa;

public sealed class RegistrarDespesaValidador : AbstractValidator<RegistrarDespesaComando>
{
    public RegistrarDespesaValidador(IProvedorDeDataHora provedorDeDataHora)
    {
        RuleFor(comando => comando.Descricao).DescricaoDeLancamentoValida();
        RuleFor(comando => comando.Valor).ValorDeLancamentoValido();
        RuleFor(comando => comando.DataDoGasto).DataDoGastoValida(provedorDeDataHora.HojeNoFusoDoUsuario);
        RuleFor(comando => comando.CategoriaId).CategoriaInformada();
        RuleFor(comando => comando.FormaDePagamento).FormaDePagamentoValida();
    }
}
