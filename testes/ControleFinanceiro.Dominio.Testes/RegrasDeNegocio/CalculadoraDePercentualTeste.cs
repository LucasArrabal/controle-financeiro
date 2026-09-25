using ControleFinanceiro.Dominio.RegrasDeNegocio;
using FluentAssertions;

namespace ControleFinanceiro.Dominio.Testes.RegrasDeNegocio;

public sealed class CalculadoraDePercentualTeste
{
    [Theory]
    [InlineData(0, 1000, 0)]
    [InlineData(250, 1000, 25)]
    [InlineData(1000, 1000, 100)]
    [InlineData(1500, 1000, 150)] // parte maior que o total é legítimo: teto estourado
    public void DeveCalcularAParticipacao(decimal parte, decimal total, decimal esperado)
    {
        CalculadoraDePercentual.DeParticipacao(parte, total).Should().Be(esperado);
    }

    [Fact]
    public void TotalZeroDeveDevolverZeroEmVezDeDividirPorZero()
    {
        // É a regra que mantém o painel renderizável num mês sem nenhum lançamento.
        CalculadoraDePercentual.DeParticipacao(parte: 500m, total: 0m).Should().Be(0m);
    }

    [Fact]
    public void TotalZeroComParteZeroDeveDevolverZero()
    {
        CalculadoraDePercentual.DeParticipacao(parte: 0m, total: 0m).Should().Be(0m);
    }

    [Fact]
    public void TotalNegativoDeveDevolverZero()
    {
        CalculadoraDePercentual.DeParticipacao(parte: 100m, total: -500m).Should().Be(0m);
    }

    [Fact]
    public void DeveArredondarParaDuasCasas()
    {
        // 1/3 = 33,3333...%
        CalculadoraDePercentual.DeParticipacao(parte: 1m, total: 3m).Should().Be(33.33m);
    }

    [Fact]
    public void DeveArredondarMeioParaLongeDoZero()
    {
        // 1840 / 6120,45 = 30,0631...% → 30,06
        CalculadoraDePercentual.DeParticipacao(parte: 1840m, total: 6120.45m).Should().Be(30.06m);
    }

    [Fact]
    public void AsParticipacoesDeUmTotalDevemSomarCemAproximadamente()
    {
        // Cada fatia é arredondada por conta própria, então a soma pode fugir de 100 por
        // centésimos. O painel aceita isso; o que não pode é fugir muito.
        var total = 5233.70m;
        decimal[] fatias = [2774.15m, 1176.85m, 846.70m, 436.00m];

        var soma = fatias.Sum(fatia => CalculadoraDePercentual.DeParticipacao(fatia, total));

        soma.Should().BeApproximately(100m, precision: 0.05m);
    }
}
