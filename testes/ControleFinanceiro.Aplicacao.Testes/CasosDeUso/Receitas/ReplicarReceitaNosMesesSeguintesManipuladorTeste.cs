using ControleFinanceiro.Aplicacao.CasosDeUso.Receitas.ReplicarReceitaNosMesesSeguintes;
using ControleFinanceiro.Aplicacao.Excecoes;
using ControleFinanceiro.Aplicacao.Testes.Dubles;
using ControleFinanceiro.Dominio.Excecoes;
using FluentAssertions;
using FluentValidation;

namespace ControleFinanceiro.Aplicacao.Testes.CasosDeUso.Receitas;

public sealed class ReplicarReceitaNosMesesSeguintesManipuladorTeste
{
    private readonly RepositorioDeReceitasEmMemoria _receitas = new();

    private ReplicarReceitaNosMesesSeguintesManipulador CriarManipulador() =>
        new(
            _receitas,
            new ProvedorDoUsuarioDeTeste(),
            new ProvedorDeDataHoraFixo(),
            new ReplicarReceitaNosMesesSeguintesValidador());

    [Fact]
    public async Task DeveCriarUmaCopiaPorMesSeguinte()
    {
        var salario = _receitas.Adicionar(dataDoRecebimento: new DateOnly(2026, 9, 5));

        var resultado = await CriarManipulador().ManipularAsync(
            new ReplicarReceitaNosMesesSeguintesComando(salario.Id, 3),
            CancellationToken.None);

        resultado.CompetenciasCriadas.Should().Equal("2026-10", "2026-11", "2026-12");
        resultado.CompetenciasIgnoradas.Should().BeEmpty();
        _receitas.Gravadas.Should().HaveCount(4);
    }

    [Fact]
    public async Task DeveVirarOAnoQuandoAOrigemEstaNoFimDoAno()
    {
        var salario = _receitas.Adicionar(dataDoRecebimento: new DateOnly(2026, 11, 5));

        var resultado = await CriarManipulador().ManipularAsync(
            new ReplicarReceitaNosMesesSeguintesComando(salario.Id, 3),
            CancellationToken.None);

        resultado.CompetenciasCriadas.Should().Equal("2026-12", "2027-01", "2027-02");
    }

    [Fact]
    public async Task ReplicarDuasVezesNaoDeveDuplicarOSalario()
    {
        // É o erro mais provável nesta tela: clicar "replicar" de novo no mês seguinte.
        var salario = _receitas.Adicionar(dataDoRecebimento: new DateOnly(2026, 9, 5));
        var manipulador = CriarManipulador();

        await manipulador.ManipularAsync(
            new ReplicarReceitaNosMesesSeguintesComando(salario.Id, 3),
            CancellationToken.None);

        var segunda = await manipulador.ManipularAsync(
            new ReplicarReceitaNosMesesSeguintesComando(salario.Id, 3),
            CancellationToken.None);

        segunda.CompetenciasCriadas.Should().BeEmpty();
        segunda.CompetenciasIgnoradas.Should().Equal("2026-10", "2026-11", "2026-12");
        _receitas.Gravadas.Should().HaveCount(4);
    }

    [Fact]
    public async Task DevePularApenasOsMesesJaOcupados()
    {
        var salario = _receitas.Adicionar(dataDoRecebimento: new DateOnly(2026, 9, 5));
        _receitas.Adicionar(descricao: "Salário", dataDoRecebimento: new DateOnly(2026, 11, 5));

        var resultado = await CriarManipulador().ManipularAsync(
            new ReplicarReceitaNosMesesSeguintesComando(salario.Id, 3),
            CancellationToken.None);

        resultado.CompetenciasCriadas.Should().Equal("2026-10", "2026-12");
        resultado.CompetenciasIgnoradas.Should().Equal("2026-11");
    }

    [Fact]
    public async Task ReceitaNaoRecorrenteNaoDeveSerReplicada()
    {
        var avulsa = _receitas.Adicionar(descricao: "Bônus", recorrente: false);

        var acao = () => CriarManipulador().ManipularAsync(
            new ReplicarReceitaNosMesesSeguintesComando(avulsa.Id, 3),
            CancellationToken.None);

        await acao.Should()
            .ThrowAsync<RegraDeNegocioVioladaException>()
            .WithMessage("*recorrentes*");
    }

    [Fact]
    public async Task ReceitaInexistenteDeveSerRecusadaComoNaoEncontrada()
    {
        var acao = () => CriarManipulador().ManipularAsync(
            new ReplicarReceitaNosMesesSeguintesComando(Guid.NewGuid(), 3),
            CancellationToken.None);

        await acao.Should().ThrowAsync<RecursoNaoEncontradoException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(25)]
    public async Task QuantidadeDeMesesForaDaFaixaDeveSerRecusada(int meses)
    {
        var salario = _receitas.Adicionar();

        var acao = () => CriarManipulador().ManipularAsync(
            new ReplicarReceitaNosMesesSeguintesComando(salario.Id, meses),
            CancellationToken.None);

        await acao.Should().ThrowAsync<ValidationException>();
    }
}
