using ControleFinanceiro.Aplicacao.Validacao;
using FluentValidation;

namespace ControleFinanceiro.Aplicacao.CasosDeUso.Receitas.RegistrarReceita;

public sealed class RegistrarReceitaValidador : AbstractValidator<RegistrarReceitaComando>
{
    public RegistrarReceitaValidador()
    {
        RuleFor(comando => comando.Descricao).DescricaoDeLancamentoValida();
        RuleFor(comando => comando.Valor).ValorDeLancamentoValido();

        // Receita não tem limite de data no futuro: salário do mês que vem é lançamento legítimo,
        // e a replicação de recorrente cria justamente lançamentos adiantados.
        RuleFor(comando => comando.DataDoRecebimento).DataInformada("Data do recebimento");
    }
}
