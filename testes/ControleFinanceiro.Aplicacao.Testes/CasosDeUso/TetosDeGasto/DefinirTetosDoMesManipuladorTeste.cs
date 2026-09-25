using ControleFinanceiro.Aplicacao.CasosDeUso.TetosDeGasto.DefinirTetosDoMes;
using ControleFinanceiro.Aplicacao.Testes.Dubles;
using ControleFinanceiro.Dominio.Excecoes;
using ControleFinanceiro.Dominio.ObjetosDeValor;
using FluentAssertions;

namespace ControleFinanceiro.Aplicacao.Testes.CasosDeUso.TetosDeGasto;

public sealed class DefinirTetosDoMesManipuladorTeste
{
    private static readonly CompetenciaMensal Setembro = CompetenciaMensal.Criar(2026, 9);

    private readonly RepositorioDeTetosDeGastoEmMemoria _tetos = new();
    private readonly RepositorioDeCategoriasEmMemoria _categorias = new();

    private DefinirTetosDoMesManipulador CriarManipulador() =>
        new(_tetos, _categorias, new ProvedorDoUsuarioDeTeste(), new DefinirTetosDoMesValidador());

    [Fact]
    public async Task DeveGravarOsLimitesInformados()
    {
        var mercado = _categorias.Adicionar(nome: "Mercado");

        var resposta = await CriarManipulador().ManipularAsync(
            new DefinirTetosDoMesComando("2026-09", [new LimiteDeCategoria(mercado.Id, 1500m)]),
            CancellationToken.None);

        resposta.Should().ContainSingle()
            .Which.ValorLimite.Should().Be(1500m);
    }

    [Fact]
    public async Task LimiteNuloDeveRemoverOTetoDaCategoria()
    {
        var mercado = _categorias.Adicionar(nome: "Mercado");
        _tetos.Adicionar(mercado.Id, Setembro, 1500m);

        var resposta = await CriarManipulador().ManipularAsync(
            new DefinirTetosDoMesComando("2026-09", [new LimiteDeCategoria(mercado.Id, null)]),
            CancellationToken.None);

        resposta.Single().ValorLimite.Should().BeNull();
        _tetos.Gravados.Should().BeEmpty();
    }

    [Fact]
    public async Task LimiteZeroDeveSerGravadoENaoConfundidoComAusencia()
    {
        // Zero é "nenhum gasto permitido"; ausência é "sem limite". São coisas diferentes.
        var lazer = _categorias.Adicionar(nome: "Lazer", cor: "#6A1B9A");

        var resposta = await CriarManipulador().ManipularAsync(
            new DefinirTetosDoMesComando("2026-09", [new LimiteDeCategoria(lazer.Id, 0m)]),
            CancellationToken.None);

        resposta.Single().ValorLimite.Should().Be(0m);
        _tetos.Gravados.Should().ContainSingle();
    }

    [Fact]
    public async Task DeveSobrescreverOLimiteJaExistente()
    {
        var mercado = _categorias.Adicionar(nome: "Mercado");
        _tetos.Adicionar(mercado.Id, Setembro, 1500m);

        await CriarManipulador().ManipularAsync(
            new DefinirTetosDoMesComando("2026-09", [new LimiteDeCategoria(mercado.Id, 1750m)]),
            CancellationToken.None);

        _tetos.Gravados.Should().ContainSingle()
            .Which.ValorLimite.Quantia.Should().Be(1750m);
    }

    [Fact]
    public async Task DeveDevolverUmaLinhaPorCategoriaAtivaMesmoSemTeto()
    {
        // A tela precisa da linha para o usuário poder definir o primeiro limite da categoria.
        var mercado = _categorias.Adicionar(nome: "Mercado");
        _categorias.Adicionar(nome: "Lazer", cor: "#6A1B9A");

        var resposta = await CriarManipulador().ManipularAsync(
            new DefinirTetosDoMesComando("2026-09", [new LimiteDeCategoria(mercado.Id, 1500m)]),
            CancellationToken.None);

        resposta.Should().HaveCount(2);
        resposta.Single(linha => linha.NomeDaCategoria == "Lazer").ValorLimite.Should().BeNull();
    }

    [Fact]
    public async Task CategoriaDesconhecidaDeveSerRecusada()
    {
        _categorias.Adicionar(nome: "Mercado");

        var acao = () => CriarManipulador().ManipularAsync(
            new DefinirTetosDoMesComando("2026-09", [new LimiteDeCategoria(Guid.NewGuid(), 100m)]),
            CancellationToken.None);

        await acao.Should()
            .ThrowAsync<RegraDeNegocioVioladaException>()
            .WithMessage("*não existe ou está desativada*");
    }

    [Fact]
    public async Task CategoriaDesativadaNaoDeveAceitarTeto()
    {
        var antiga = _categorias.Adicionar(nome: "Antiga", ativa: false);

        var acao = () => CriarManipulador().ManipularAsync(
            new DefinirTetosDoMesComando("2026-09", [new LimiteDeCategoria(antiga.Id, 100m)]),
            CancellationToken.None);

        await acao.Should().ThrowAsync<RegraDeNegocioVioladaException>();
    }
}
