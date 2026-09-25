using ControleFinanceiro.Aplicacao.CasosDeUso.PainelResumoMensal;
using ControleFinanceiro.Aplicacao.Contratos.Repositorios;
using ControleFinanceiro.Aplicacao.Dtos.Respostas;
using ControleFinanceiro.Aplicacao.Testes.Dubles;
using ControleFinanceiro.Dominio.Excecoes;
using FluentAssertions;

namespace ControleFinanceiro.Aplicacao.Testes.CasosDeUso.PainelResumoMensal;

public sealed class ObterResumoMensalManipuladorTeste
{
    private static readonly Guid Mercado = Guid.Parse("a1000000-0000-4000-8000-000000000002");
    private static readonly Guid Lazer = Guid.Parse("a1000000-0000-4000-8000-000000000004");

    private static Task<RespostaDoResumoMensal> Obter(ResumoMensalAgregado agregado)
    {
        var manipulador = new ObterResumoMensalManipulador(
            new ConsultaDeResumoMensalFixa(agregado),
            new ProvedorDoUsuarioDeTeste());

        return manipulador.ManipularAsync(
            new ObterResumoMensalConsulta("2026-09"),
            CancellationToken.None);
    }

    [Fact]
    public async Task DeveCalcularSaldoEPercentualDaRendaComprometida()
    {
        var resumo = await Obter(new ResumoMensalAgregado(
            TotalDeReceitas: 8500m,
            TotalDeDespesas: 6120.45m,
            GastosPorCategoria: []));

        resumo.Saldo.Should().Be(2379.55m);
        resumo.PercentualDaRendaComprometida.Should().Be(72.01m);
    }

    [Fact]
    public async Task MesSemReceitaDeveDevolverRendaComprometidaZero()
    {
        // A divisão por zero aqui derrubaria o painel inteiro.
        var resumo = await Obter(new ResumoMensalAgregado(0m, 500m, []));

        resumo.PercentualDaRendaComprometida.Should().Be(0m);
        resumo.Saldo.Should().Be(-500m);
    }

    [Fact]
    public async Task MesSemNadaDeveDevolverTudoZerado()
    {
        var resumo = await Obter(new ResumoMensalAgregado(0m, 0m, []));

        resumo.TotalDeReceitas.Should().Be(0m);
        resumo.TotalDeDespesas.Should().Be(0m);
        resumo.Saldo.Should().Be(0m);
        resumo.PercentualDaRendaComprometida.Should().Be(0m);
        resumo.GastosPorCategoria.Should().BeEmpty();
    }

    [Fact]
    public async Task DeveCalcularAFatiaDeCadaCategoriaSobreOTotalGasto()
    {
        var resumo = await Obter(new ResumoMensalAgregado(
            TotalDeReceitas: 10_000m,
            TotalDeDespesas: 2000m,
            GastosPorCategoria:
            [
                new GastoDeCategoriaAgregado(Mercado, "Mercado", "#2E7D32", 1500m, 1500m),
                new GastoDeCategoriaAgregado(Lazer, "Lazer", "#6A1B9A", 500m, 400m),
            ]));

        resumo.GastosPorCategoria[0].PercentualDoTotalGasto.Should().Be(75m);
        resumo.GastosPorCategoria[1].PercentualDoTotalGasto.Should().Be(25m);
    }

    [Fact]
    public async Task TotalGastoZeroDeveDevolverFatiaZeroEmVezDeDividirPorZero()
    {
        var resumo = await Obter(new ResumoMensalAgregado(
            TotalDeReceitas: 5000m,
            TotalDeDespesas: 0m,
            GastosPorCategoria:
            [
                new GastoDeCategoriaAgregado(Mercado, "Mercado", "#2E7D32", 0m, 1500m),
            ]));

        resumo.GastosPorCategoria[0].PercentualDoTotalGasto.Should().Be(0m);
    }

    [Fact]
    public async Task CategoriaSemTetoNaoDeveTerPercentualNemSituacao()
    {
        // É o que faz a tela mostrar a linha sem barra de progresso.
        var resumo = await Obter(new ResumoMensalAgregado(
            TotalDeReceitas: 5000m,
            TotalDeDespesas: 500m,
            GastosPorCategoria:
            [
                new GastoDeCategoriaAgregado(Lazer, "Lazer", "#6A1B9A", 500m, TetoDefinido: null),
            ]));

        var lazer = resumo.GastosPorCategoria[0];

        lazer.TetoDefinido.Should().BeNull();
        lazer.PercentualDoTetoConsumido.Should().BeNull();
        lazer.SituacaoDoTeto.Should().BeNull();
    }

    [Theory]
    [InlineData(700, 1000, "DentroDoLimite")]
    [InlineData(800, 1000, "DentroDoLimite")]
    [InlineData(900, 1000, "EmAtencao")]
    [InlineData(1000, 1000, "EmAtencao")]
    [InlineData(1000.01, 1000, "Estourado")]
    public async Task DeveClassificarASituacaoDoTeto(
        decimal valorGasto,
        decimal teto,
        string situacaoEsperada)
    {
        var resumo = await Obter(new ResumoMensalAgregado(
            TotalDeReceitas: 10_000m,
            TotalDeDespesas: valorGasto,
            GastosPorCategoria:
            [
                new GastoDeCategoriaAgregado(Mercado, "Mercado", "#2E7D32", valorGasto, teto),
            ]));

        resumo.GastosPorCategoria[0].SituacaoDoTeto.Should().Be(situacaoEsperada);
    }

    [Fact]
    public async Task TetoZeroComGastoDeveAparecerComoEstourado()
    {
        var resumo = await Obter(new ResumoMensalAgregado(
            TotalDeReceitas: 5000m,
            TotalDeDespesas: 50m,
            GastosPorCategoria:
            [
                new GastoDeCategoriaAgregado(Lazer, "Lazer", "#6A1B9A", 50m, TetoDefinido: 0m),
            ]));

        var lazer = resumo.GastosPorCategoria[0];

        lazer.SituacaoDoTeto.Should().Be("Estourado");
        // Teto zero não tem percentual consumido calculável: a regra devolve 0, não infinito.
        lazer.PercentualDoTetoConsumido.Should().Be(0m);
    }

    [Fact]
    public async Task DeveDevolverACompetenciaNormalizada()
    {
        var resumo = await Obter(new ResumoMensalAgregado(0m, 0m, []));

        resumo.Competencia.Should().Be("2026-09");
    }

    [Fact]
    public async Task CompetenciaInvalidaDeveSerRecusada()
    {
        var manipulador = new ObterResumoMensalManipulador(
            new ConsultaDeResumoMensalFixa(new ResumoMensalAgregado(0m, 0m, [])),
            new ProvedorDoUsuarioDeTeste());

        var acao = () => manipulador.ManipularAsync(
            new ObterResumoMensalConsulta("setembro"),
            CancellationToken.None);

        await acao.Should().ThrowAsync<RegraDeNegocioVioladaException>();
    }
}
