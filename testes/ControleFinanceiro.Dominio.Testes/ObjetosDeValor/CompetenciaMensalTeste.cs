using ControleFinanceiro.Dominio.Excecoes;
using ControleFinanceiro.Dominio.ObjetosDeValor;
using FluentAssertions;

namespace ControleFinanceiro.Dominio.Testes.ObjetosDeValor;

public sealed class CompetenciaMensalTeste
{
    [Fact]
    public void DeveFormatarComQuatroDigitosDeAnoEDoisDeMes()
    {
        CompetenciaMensal.Criar(2026, 9).ToString().Should().Be("2026-09");
    }

    [Fact]
    public void DeveAnalisarTextoNoFormatoEsperado()
    {
        var competencia = CompetenciaMensal.Analisar("2026-09");

        competencia.Ano.Should().Be(2026);
        competencia.Mes.Should().Be(9);
    }

    [Fact]
    public void DeveIgnorarEspacosEmVolta()
    {
        CompetenciaMensal.Analisar("  2026-09 ").ToString().Should().Be("2026-09");
    }

    [Theory]
    [InlineData("setembro")]
    [InlineData("2026-13")]
    [InlineData("2026-00")]
    [InlineData("2026/09")]
    [InlineData("26-09")]
    [InlineData("2026-9")]
    [InlineData("")]
    [InlineData(null)]
    public void DeveRejeitarTextoInvalido(string? texto)
    {
        CompetenciaMensal.TentarAnalisar(texto, out _).Should().BeFalse();
    }

    [Fact]
    public void AnalisarTextoInvalidoDeveExplicarOFormato()
    {
        var acao = () => CompetenciaMensal.Analisar("setembro");

        acao.Should()
            .Throw<RegraDeNegocioVioladaException>()
            .WithMessage("*yyyy-MM*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    public void CriarDeveRejeitarMesForaDaFaixa(int mes)
    {
        var acao = () => CompetenciaMensal.Criar(2026, mes);

        acao.Should().Throw<RegraDeNegocioVioladaException>();
    }

    [Fact]
    public void AnteriorDeJaneiroDeveSerDezembroDoAnoPassado()
    {
        CompetenciaMensal.Criar(2026, 1).Anterior().ToString().Should().Be("2025-12");
    }

    [Fact]
    public void ProximaDeDezembroDeveSerJaneiroDoAnoSeguinte()
    {
        CompetenciaMensal.Criar(2026, 12).Proxima().ToString().Should().Be("2027-01");
    }

    [Fact]
    public void DeveDelimitarOMes()
    {
        var setembro = CompetenciaMensal.Criar(2026, 9);

        setembro.PrimeiroDia.Should().Be(new DateOnly(2026, 9, 1));
        setembro.UltimoDia.Should().Be(new DateOnly(2026, 9, 30));
    }

    [Fact]
    public void DeveReconhecerFevereiroDeAnoBissexto()
    {
        CompetenciaMensal.Criar(2028, 2).UltimoDia.Should().Be(new DateOnly(2028, 2, 29));
    }

    [Fact]
    public void DeveVirDeUmaData()
    {
        CompetenciaMensal.DeData(new DateOnly(2026, 9, 21)).ToString().Should().Be("2026-09");
    }

    [Fact]
    public void DeveOrdenarPorAnoEDepoisPorMes()
    {
        CompetenciaMensal[] fora =
        [
            CompetenciaMensal.Criar(2026, 3),
            CompetenciaMensal.Criar(2025, 12),
            CompetenciaMensal.Criar(2026, 1),
        ];

        fora.OrderBy(competencia => competencia)
            .Select(competencia => competencia.ToString())
            .Should()
            .Equal("2025-12", "2026-01", "2026-03");
    }
}
