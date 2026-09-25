using ControleFinanceiro.Aplicacao.CasosDeUso.Receitas.RegistrarReceita;
using ControleFinanceiro.Aplicacao.Testes.Dubles;
using FluentAssertions;
using FluentValidation.Results;

namespace ControleFinanceiro.Aplicacao.Testes.CasosDeUso.Receitas;

public sealed class RegistrarReceitaValidadorTeste
{
    private static readonly DateOnly Hoje = ProvedorDeDataHoraFixo.HojeDeReferencia;

    private readonly RegistrarReceitaValidador _validador = new();

    private ValidationResult Validar(
        string descricao = "Salário",
        decimal valor = 7800m,
        DateOnly? dataDoRecebimento = null,
        bool recorrente = false) =>
        _validador.Validate(new RegistrarReceitaComando(
            descricao,
            valor,
            dataDoRecebimento ?? Hoje,
            recorrente));

    [Fact]
    public void ComandoCompletoDeveSerValido()
    {
        Validar().IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("x")]
    public void DeveRecusarDescricaoCurta(string descricao)
    {
        Validar(descricao: descricao).IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void DeveRecusarValorNaoPositivo(decimal valor)
    {
        Validar(valor: valor)
            .Errors.Should()
            .Contain(erro => erro.ErrorMessage.Contains("maior que zero"));
    }

    [Fact]
    public void DeveAceitarDataBemNoFuturo()
    {
        // Receita não herda o limite de 1 ano da despesa: é o que permite replicar salário.
        Validar(dataDoRecebimento: Hoje.AddYears(2)).IsValid.Should().BeTrue();
    }

    [Fact]
    public void DeveRecusarDataNaoInformada()
    {
        Validar(dataDoRecebimento: default(DateOnly))
            .Errors.Should()
            .Contain(erro => erro.ErrorMessage.Contains("obrigatória"));
    }
}
