using ControleFinanceiro.Dominio.Entidades;
using ControleFinanceiro.Dominio.Excecoes;
using FluentAssertions;

namespace ControleFinanceiro.Dominio.Testes.Entidades;

public sealed class DespesaTeste
{
    private static readonly DateOnly Hoje = new(2026, 9, 21);
    private static readonly DateTimeOffset Agora = new(2026, 9, 21, 12, 0, 0, TimeSpan.Zero);
    private static readonly Guid Usuario = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid Categoria = Guid.Parse("a1000000-0000-4000-8000-000000000002");

    private static Despesa Registrar(
        string descricao = "Mercado",
        decimal valor = 100m,
        DateOnly? dataDoGasto = null,
        Guid? categoriaId = null,
        string? formaDePagamento = null,
        string? observacao = null) =>
        Despesa.Registrar(
            Usuario,
            descricao,
            valor,
            dataDoGasto ?? Hoje,
            categoriaId ?? Categoria,
            formaDePagamento,
            observacao,
            Hoje,
            Agora);

    [Fact]
    public void DeveRegistrarComOsDadosInformados()
    {
        var despesa = Registrar(descricao: "Açougue", valor: 158.90m, formaDePagamento: "Cartão");

        despesa.Descricao.Should().Be("Açougue");
        despesa.Valor.Quantia.Should().Be(158.90m);
        despesa.FormaDePagamento.Should().Be("Cartão");
        despesa.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void DeveGravarCriadoEmEmUtc()
    {
        Registrar().CriadoEm.Offset.Should().Be(TimeSpan.Zero);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void DeveRejeitarValorNaoPositivo(decimal valor)
    {
        var acao = () => Registrar(valor: valor);

        acao.Should()
            .Throw<RegraDeNegocioVioladaException>()
            .WithMessage("*maior que zero*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("x")]
    public void DeveRejeitarDescricaoCurta(string descricao)
    {
        var acao = () => Registrar(descricao: descricao);

        acao.Should().Throw<RegraDeNegocioVioladaException>();
    }

    [Fact]
    public void DeveRejeitarDescricaoAcimaDoTamanhoMaximo()
    {
        var acao = () => Registrar(descricao: new string('a', Despesa.TamanhoMaximoDaDescricao + 1));

        acao.Should().Throw<RegraDeNegocioVioladaException>();
    }

    [Fact]
    public void DeveRecortarEspacosDaDescricao()
    {
        Registrar(descricao: "  Padaria  ").Descricao.Should().Be("Padaria");
    }

    [Fact]
    public void DeveAceitarDataAteUmAnoNoFuturo()
    {
        var acao = () => Registrar(dataDoGasto: Hoje.AddMonths(12));

        acao.Should().NotThrow();
    }

    [Fact]
    public void DeveRejeitarDataMaisDeUmAnoNoFuturo()
    {
        var acao = () => Registrar(dataDoGasto: Hoje.AddMonths(12).AddDays(1));

        acao.Should()
            .Throw<RegraDeNegocioVioladaException>()
            .WithMessage("*1 ano no futuro*");
    }

    [Fact]
    public void DeveAceitarDataAntiga()
    {
        // Lançar gasto do mês passado é normal; só o futuro distante é bloqueado.
        var acao = () => Registrar(dataDoGasto: Hoje.AddYears(-3));

        acao.Should().NotThrow();
    }

    [Fact]
    public void DeveRejeitarCategoriaVazia()
    {
        var acao = () => Registrar(categoriaId: Guid.Empty);

        acao.Should().Throw<RegraDeNegocioVioladaException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void FormaDePagamentoEmBrancoDeveVirarNulo(string formaDePagamento)
    {
        Registrar(formaDePagamento: formaDePagamento).FormaDePagamento.Should().BeNull();
    }

    [Fact]
    public void ObservacaoEmBrancoDeveVirarNulo()
    {
        Registrar(observacao: "  ").Observacao.Should().BeNull();
    }

    [Fact]
    public void CompetenciaDeveSairDaDataDoGasto()
    {
        Registrar(dataDoGasto: new DateOnly(2026, 3, 15)).Competencia.ToString().Should().Be("2026-03");
    }

    [Fact]
    public void AlterarDeveAplicarAsMesmasRegras()
    {
        var despesa = Registrar();

        var acao = () => despesa.Alterar("x", 10m, Hoje, Categoria, null, null, Hoje);

        acao.Should().Throw<RegraDeNegocioVioladaException>();
    }

    [Fact]
    public void AlterarDeveTrocarOsDados()
    {
        var despesa = Registrar();

        despesa.Alterar("Hortifruti", 76.10m, Hoje.AddDays(-2), Categoria, "Pix", "feira", Hoje);

        despesa.Descricao.Should().Be("Hortifruti");
        despesa.Valor.Quantia.Should().Be(76.10m);
        despesa.DataDoGasto.Should().Be(Hoje.AddDays(-2));
        despesa.FormaDePagamento.Should().Be("Pix");
        despesa.Observacao.Should().Be("feira");
    }

    [Fact]
    public void RestaurarNaoDeveAplicarAsRegrasDeCriacao()
    {
        // Reidratação vem do banco: se a regra mudar, o histórico não pode deixar de carregar.
        var acao = () => Despesa.Restaurar(
            Guid.NewGuid(),
            Usuario,
            "x",
            10m,
            Hoje.AddYears(5),
            Categoria,
            null,
            null,
            Agora);

        acao.Should().NotThrow();
    }
}
