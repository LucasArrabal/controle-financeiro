using ControleFinanceiro.Aplicacao.CasosDeUso.Categorias.CriarCategoria;
using FluentAssertions;
using FluentValidation.Results;

namespace ControleFinanceiro.Aplicacao.Testes.CasosDeUso.Categorias;

public sealed class CriarCategoriaValidadorTeste
{
    private readonly CriarCategoriaValidador _validador = new();

    private ValidationResult Validar(
        string nome = "Mercado",
        string tipo = "Mercado",
        string cor = "#2E7D32") =>
        _validador.Validate(new CriarCategoriaComando(nome, tipo, cor));

    [Fact]
    public void ComandoCompletoDeveSerValido()
    {
        Validar().IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("GastoFixo")]
    [InlineData("Mercado")]
    [InlineData("Essencial")]
    [InlineData("Lazer")]
    public void DeveAceitarOsTiposConhecidos(string tipo)
    {
        Validar(tipo: tipo).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("gastofixo")]
    [InlineData("MERCADO")]
    public void DeveAceitarTipoEmQualquerCaixa(string tipo)
    {
        Validar(tipo: tipo).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("Investimento")]
    [InlineData("")]
    [InlineData("99")]
    public void DeveRecusarTipoDesconhecido(string tipo)
    {
        Validar(tipo: tipo)
            .Errors.Should()
            .Contain(erro => erro.PropertyName == nameof(CriarCategoriaComando.Tipo));
    }

    [Theory]
    [InlineData("#2E7D32")]
    [InlineData("#2e7d32")]
    public void DeveAceitarCorEmQualquerCaixa(string cor)
    {
        Validar(cor: cor).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("verde")]
    [InlineData("2E7D32")]
    [InlineData("#2E7D3")]
    [InlineData("")]
    public void DeveRecusarCorInvalida(string cor)
    {
        Validar(cor: cor)
            .Errors.Should()
            .Contain(erro => erro.PropertyName == nameof(CriarCategoriaComando.CorHexadecimal));
    }

    [Theory]
    [InlineData("")]
    [InlineData("x")]
    public void DeveRecusarNomeCurto(string nome)
    {
        Validar(nome: nome).IsValid.Should().BeFalse();
    }

    [Fact]
    public void DeveRecusarNomeAcimaDeSessentaCaracteres()
    {
        Validar(nome: new string('a', 61)).IsValid.Should().BeFalse();
    }
}
