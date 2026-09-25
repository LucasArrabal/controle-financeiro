using ControleFinanceiro.Dominio.Entidades;
using ControleFinanceiro.Dominio.Enumeradores;
using ControleFinanceiro.Dominio.Excecoes;
using FluentAssertions;

namespace ControleFinanceiro.Dominio.Testes.Entidades;

public sealed class CategoriaDeGastoTeste
{
    private static readonly Guid Usuario = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private static CategoriaDeGasto Criar(
        string nome = "Mercado",
        TipoDeCategoria tipo = TipoDeCategoria.Mercado,
        string cor = "#2E7D32") =>
        CategoriaDeGasto.Criar(Usuario, nome, tipo, cor);

    [Fact]
    public void DeveNascerAtiva()
    {
        Criar().Ativa.Should().BeTrue();
    }

    [Fact]
    public void DeveNormalizarACorParaMaiusculo()
    {
        // A cor vai para o gráfico e para o `check` da coluna, que exige maiúsculo.
        Criar(cor: "#2e7d32").CorHexadecimal.Should().Be("#2E7D32");
    }

    [Fact]
    public void DeveRecortarEspacosDaCor()
    {
        Criar(cor: " #2e7d32 ").CorHexadecimal.Should().Be("#2E7D32");
    }

    [Theory]
    [InlineData("verde")]
    [InlineData("2E7D32")]    // sem #
    [InlineData("#2E7D3")]    // cinco dígitos
    [InlineData("#2E7D322")]  // sete dígitos
    [InlineData("#GGGGGG")]   // fora do hexadecimal
    public void DeveRejeitarCorInvalida(string cor)
    {
        var acao = () => Criar(cor: cor);

        acao.Should()
            .Throw<RegraDeNegocioVioladaException>()
            .WithMessage("*#RRGGBB*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void DeveExigirACor(string cor)
    {
        // Mensagem diferente da cor malformada: aqui o campo simplesmente não foi preenchido.
        var acao = () => Criar(cor: cor);

        acao.Should()
            .Throw<RegraDeNegocioVioladaException>()
            .WithMessage("*obrigatória*");
    }

    [Fact]
    public void DeveAceitarCorAbreviadaSomenteNoFormatoCompleto()
    {
        var acao = () => Criar(cor: "#2E7");

        acao.Should().Throw<RegraDeNegocioVioladaException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("x")]
    public void DeveRejeitarNomeCurto(string nome)
    {
        var acao = () => Criar(nome: nome);

        acao.Should().Throw<RegraDeNegocioVioladaException>();
    }

    [Fact]
    public void DeveRejeitarNomeAcimaDoTamanhoMaximo()
    {
        var acao = () => Criar(nome: new string('a', CategoriaDeGasto.TamanhoMaximoDoNome + 1));

        acao.Should().Throw<RegraDeNegocioVioladaException>();
    }

    [Fact]
    public void DeveRecortarEspacosDoNome()
    {
        Criar(nome: "  Gastos Fixos  ").Nome.Should().Be("Gastos Fixos");
    }

    [Fact]
    public void DeveRejeitarTipoDesconhecido()
    {
        var acao = () => Criar(tipo: (TipoDeCategoria)99);

        acao.Should().Throw<RegraDeNegocioVioladaException>();
    }

    [Fact]
    public void DesativarEReativarDevemAlternarASituacao()
    {
        var categoria = Criar();

        categoria.Desativar();
        categoria.Ativa.Should().BeFalse();

        categoria.Reativar();
        categoria.Ativa.Should().BeTrue();
    }

    [Fact]
    public void RestaurarDeveManterOsDadosComoEstavamNoBanco()
    {
        var id = Guid.NewGuid();

        var categoria = CategoriaDeGasto.Restaurar(
            id,
            Usuario,
            "Mercado",
            TipoDeCategoria.Mercado,
            "#2E7D32",
            ativa: false);

        categoria.Id.Should().Be(id);
        categoria.Ativa.Should().BeFalse();
    }
}
