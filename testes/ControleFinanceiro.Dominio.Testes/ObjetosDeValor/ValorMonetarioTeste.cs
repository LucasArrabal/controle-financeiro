using ControleFinanceiro.Dominio.Excecoes;
using ControleFinanceiro.Dominio.ObjetosDeValor;
using FluentAssertions;

namespace ControleFinanceiro.Dominio.Testes.ObjetosDeValor;

public sealed class ValorMonetarioTeste
{
    [Fact]
    public void CriarDeveAceitarZero()
    {
        // Zero é válido para teto de gasto ("nenhum gasto permitido").
        ValorMonetario.Criar(0m).Quantia.Should().Be(0m);
    }

    [Fact]
    public void CriarDeveRejeitarNegativo()
    {
        var acao = () => ValorMonetario.Criar(-0.01m);

        acao.Should()
            .Throw<RegraDeNegocioVioladaException>()
            .WithMessage("*não pode ser negativo*");
    }

    [Fact]
    public void CriarDeveRejeitarAcimaDoLimiteDaColuna()
    {
        var acao = () => ValorMonetario.Criar(ValorMonetario.LimiteMaximo + 0.01m);

        acao.Should().Throw<RegraDeNegocioVioladaException>();
    }

    [Fact]
    public void CriarPositivoDeveRejeitarZero()
    {
        var acao = () => ValorMonetario.CriarPositivo(0m);

        acao.Should()
            .Throw<RegraDeNegocioVioladaException>()
            .WithMessage("*maior que zero*");
    }

    [Theory]
    [InlineData(1.234, 1.23)]
    [InlineData(1.235, 1.24)] // meio arredonda para longe do zero, não para o par
    [InlineData(1.236, 1.24)]
    public void DeveArredondarParaDuasCasas(decimal informado, decimal esperado)
    {
        ValorMonetario.Criar(informado).Quantia.Should().Be(esperado);
    }

    [Fact]
    public void DeveSomarArredondando()
    {
        var soma = ValorMonetario.Criar(0.015m) + ValorMonetario.Criar(0.015m);

        // 0,02 + 0,02 — cada parcela é arredondada antes da soma.
        soma.Quantia.Should().Be(0.04m);
    }

    [Fact]
    public void DeveComparar()
    {
        var menor = ValorMonetario.Criar(10m);
        var maior = ValorMonetario.Criar(20m);

        (maior > menor).Should().BeTrue();
        (menor < maior).Should().BeTrue();
        (menor <= ValorMonetario.Criar(10m)).Should().BeTrue();
    }

    [Fact]
    public void ZeroDeveSerReconhecidoComoZeroENaoPositivo()
    {
        ValorMonetario.Zero.EhZero.Should().BeTrue();
        ValorMonetario.Zero.EhPositivo.Should().BeFalse();
    }
}
