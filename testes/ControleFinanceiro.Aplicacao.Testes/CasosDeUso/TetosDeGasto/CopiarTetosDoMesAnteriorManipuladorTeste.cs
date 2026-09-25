using ControleFinanceiro.Aplicacao.CasosDeUso.TetosDeGasto.CopiarTetosDoMesAnterior;
using ControleFinanceiro.Aplicacao.Testes.Dubles;
using ControleFinanceiro.Dominio.Excecoes;
using ControleFinanceiro.Dominio.ObjetosDeValor;
using FluentAssertions;

namespace ControleFinanceiro.Aplicacao.Testes.CasosDeUso.TetosDeGasto;

public sealed class CopiarTetosDoMesAnteriorManipuladorTeste
{
    private static readonly CompetenciaMensal Agosto = CompetenciaMensal.Criar(2026, 8);
    private static readonly CompetenciaMensal Setembro = CompetenciaMensal.Criar(2026, 9);

    private readonly RepositorioDeTetosDeGastoEmMemoria _tetos = new();
    private readonly RepositorioDeCategoriasEmMemoria _categorias = new();

    private CopiarTetosDoMesAnteriorManipulador CriarManipulador() =>
        new(_tetos, _categorias, new ProvedorDoUsuarioDeTeste());

    [Fact]
    public async Task DeveCopiarOsLimitesDoMesAnterior()
    {
        var mercado = _categorias.Adicionar(nome: "Mercado");
        var lazer = _categorias.Adicionar(nome: "Lazer", cor: "#6A1B9A");
        _tetos.Adicionar(mercado.Id, Agosto, 1500m);
        _tetos.Adicionar(lazer.Id, Agosto, 400m);

        var resposta = await CriarManipulador().ManipularAsync(
            new CopiarTetosDoMesAnteriorComando("2026-09"),
            CancellationToken.None);

        resposta.Single(linha => linha.NomeDaCategoria == "Mercado").ValorLimite.Should().Be(1500m);
        resposta.Single(linha => linha.NomeDaCategoria == "Lazer").ValorLimite.Should().Be(400m);
    }

    [Fact]
    public async Task DeveManterOsTetosDoMesDeOrigemIntactos()
    {
        var mercado = _categorias.Adicionar(nome: "Mercado");
        _tetos.Adicionar(mercado.Id, Agosto, 1500m);

        await CriarManipulador().ManipularAsync(
            new CopiarTetosDoMesAnteriorComando("2026-09"),
            CancellationToken.None);

        _tetos.Gravados.Should().HaveCount(2);
        _tetos.Gravados.Should().Contain(teto => teto.Competencia.Equals(Agosto));
        _tetos.Gravados.Should().Contain(teto => teto.Competencia.Equals(Setembro));
    }

    [Fact]
    public async Task DeveVirarOAnoAoCopiarParaJaneiro()
    {
        var mercado = _categorias.Adicionar(nome: "Mercado");
        _tetos.Adicionar(mercado.Id, CompetenciaMensal.Criar(2025, 12), 1500m);

        var resposta = await CriarManipulador().ManipularAsync(
            new CopiarTetosDoMesAnteriorComando("2026-01"),
            CancellationToken.None);

        resposta.Single().ValorLimite.Should().Be(1500m);
    }

    [Fact]
    public async Task CategoriaDesativadaDepoisNaoDeveGanharTetoNoMesNovo()
    {
        var antiga = _categorias.Adicionar(nome: "Antiga", ativa: false);
        _tetos.Adicionar(antiga.Id, Agosto, 300m);

        var resposta = await CriarManipulador().ManipularAsync(
            new CopiarTetosDoMesAnteriorComando("2026-09"),
            CancellationToken.None);

        resposta.Should().BeEmpty();
        _tetos.Gravados.Should().ContainSingle()
            .Which.Competencia.Should().Be(Agosto);
    }

    [Fact]
    public async Task MesAnteriorSemTetosDeveSerRecusado()
    {
        _categorias.Adicionar(nome: "Mercado");

        var acao = () => CriarManipulador().ManipularAsync(
            new CopiarTetosDoMesAnteriorComando("2026-09"),
            CancellationToken.None);

        await acao.Should()
            .ThrowAsync<RegraDeNegocioVioladaException>()
            .WithMessage("*2026-08*");
    }

    [Fact]
    public async Task CompetenciaInvalidaDeveSerRecusada()
    {
        var acao = () => CriarManipulador().ManipularAsync(
            new CopiarTetosDoMesAnteriorComando("setembro"),
            CancellationToken.None);

        await acao.Should().ThrowAsync<RegraDeNegocioVioladaException>();
    }
}
