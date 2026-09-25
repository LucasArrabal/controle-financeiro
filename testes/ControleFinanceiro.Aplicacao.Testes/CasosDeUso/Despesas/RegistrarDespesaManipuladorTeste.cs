using ControleFinanceiro.Aplicacao.CasosDeUso.Despesas.RegistrarDespesa;
using ControleFinanceiro.Aplicacao.Excecoes;
using ControleFinanceiro.Aplicacao.Testes.Dubles;
using ControleFinanceiro.Dominio.Excecoes;
using FluentAssertions;
using FluentValidation;

namespace ControleFinanceiro.Aplicacao.Testes.CasosDeUso.Despesas;

public sealed class RegistrarDespesaManipuladorTeste
{
    private static readonly DateOnly Hoje = ProvedorDeDataHoraFixo.HojeDeReferencia;

    private readonly RepositorioDeDespesasEmMemoria _despesas = new();
    private readonly RepositorioDeCategoriasEmMemoria _categorias = new();

    private RegistrarDespesaManipulador CriarManipulador() =>
        new(
            _despesas,
            _categorias,
            new ProvedorDoUsuarioDeTeste(),
            new ProvedorDeDataHoraFixo(),
            new RegistrarDespesaValidador(new ProvedorDeDataHoraFixo()));

    private static RegistrarDespesaComando Comando(Guid categoriaId) =>
        new("Açougue", 158.90m, Hoje, categoriaId, "Cartão", null);

    [Fact]
    public async Task DeveGravarADespesaEDevolverACategoriaJunto()
    {
        var mercado = _categorias.Adicionar(nome: "Mercado", cor: "#2E7D32");

        var resposta = await CriarManipulador()
            .ManipularAsync(Comando(mercado.Id), CancellationToken.None);

        resposta.Descricao.Should().Be("Açougue");
        resposta.Valor.Should().Be(158.90m);
        // A tabela do mês precisa do nome e da cor sem uma segunda chamada.
        resposta.NomeDaCategoria.Should().Be("Mercado");
        resposta.CorDaCategoria.Should().Be("#2E7D32");

        _despesas.Gravadas.Should().ContainSingle();
    }

    [Fact]
    public async Task CategoriaInexistenteDeveSerRecusadaComoNaoEncontrada()
    {
        var acao = () => CriarManipulador()
            .ManipularAsync(Comando(Guid.NewGuid()), CancellationToken.None);

        await acao.Should().ThrowAsync<RecursoNaoEncontradoException>();
        _despesas.Gravadas.Should().BeEmpty();
    }

    [Fact]
    public async Task CategoriaDesativadaNaoDeveAceitarNovoLancamento()
    {
        var desativada = _categorias.Adicionar(nome: "Antiga", ativa: false);

        var acao = () => CriarManipulador()
            .ManipularAsync(Comando(desativada.Id), CancellationToken.None);

        await acao.Should()
            .ThrowAsync<RegraDeNegocioVioladaException>()
            .WithMessage("*desativada*");

        _despesas.Gravadas.Should().BeEmpty();
    }

    [Fact]
    public async Task ComandoInvalidoNaoDeveChegarAoRepositorio()
    {
        var mercado = _categorias.Adicionar();

        var acao = () => CriarManipulador().ManipularAsync(
            new RegistrarDespesaComando("x", 0m, Hoje, mercado.Id, null, null),
            CancellationToken.None);

        await acao.Should().ThrowAsync<ValidationException>();
        _despesas.Gravadas.Should().BeEmpty();
    }
}
