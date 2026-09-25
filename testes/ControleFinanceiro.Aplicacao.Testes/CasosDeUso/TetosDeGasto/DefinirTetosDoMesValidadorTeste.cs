using ControleFinanceiro.Aplicacao.CasosDeUso.TetosDeGasto.DefinirTetosDoMes;
using FluentAssertions;
using FluentValidation.Results;

namespace ControleFinanceiro.Aplicacao.Testes.CasosDeUso.TetosDeGasto;

public sealed class DefinirTetosDoMesValidadorTeste
{
    private static readonly Guid Mercado = Guid.Parse("a1000000-0000-4000-8000-000000000002");
    private static readonly Guid Lazer = Guid.Parse("a1000000-0000-4000-8000-000000000004");

    private readonly DefinirTetosDoMesValidador _validador = new();

    private ValidationResult Validar(string competencia, params LimiteDeCategoria[] limites) =>
        _validador.Validate(new DefinirTetosDoMesComando(competencia, limites));

    [Fact]
    public void ComandoCompletoDeveSerValido()
    {
        Validar("2026-09", new LimiteDeCategoria(Mercado, 1500m)).IsValid.Should().BeTrue();
    }

    [Fact]
    public void TetoZeroDeveSerAceito()
    {
        // Zero é limite legítimo: "nenhum gasto permitido".
        Validar("2026-09", new LimiteDeCategoria(Mercado, 0m)).IsValid.Should().BeTrue();
    }

    [Fact]
    public void TetoNuloDeveSerAceitoPorqueRemoveOLimite()
    {
        Validar("2026-09", new LimiteDeCategoria(Mercado, null)).IsValid.Should().BeTrue();
    }

    [Fact]
    public void DeveRecusarTetoNegativo()
    {
        Validar("2026-09", new LimiteDeCategoria(Mercado, -1m))
            .Errors.Should()
            .Contain(erro => erro.ErrorMessage.Contains("não pode ser negativo"));
    }

    [Theory]
    [InlineData("setembro")]
    [InlineData("2026-13")]
    [InlineData("2026/09")]
    [InlineData("")]
    public void DeveRecusarCompetenciaInvalida(string competencia)
    {
        Validar(competencia, new LimiteDeCategoria(Mercado, 100m))
            .Errors.Should()
            .Contain(erro => erro.PropertyName == nameof(DefinirTetosDoMesComando.Competencia));
    }

    [Fact]
    public void DeveRecusarCategoriaVazia()
    {
        Validar("2026-09", new LimiteDeCategoria(Guid.Empty, 100m)).IsValid.Should().BeFalse();
    }

    [Fact]
    public void DeveRecusarAMesmaCategoriaDuasVezes()
    {
        // Dois limites para a mesma categoria tornariam o resultado dependente da ordem.
        Validar(
                "2026-09",
                new LimiteDeCategoria(Mercado, 1500m),
                new LimiteDeCategoria(Mercado, 900m))
            .Errors.Should()
            .Contain(erro => erro.ErrorMessage.Contains("mais de uma vez"));
    }

    [Fact]
    public void DeveAceitarCategoriasDiferentes()
    {
        Validar(
                "2026-09",
                new LimiteDeCategoria(Mercado, 1500m),
                new LimiteDeCategoria(Lazer, 400m))
            .IsValid.Should().BeTrue();
    }

    [Fact]
    public void ListaVaziaDeveSerAceita()
    {
        // Salvar a tela sem nenhuma categoria não é erro; é só um mês sem categoria ativa.
        Validar("2026-09").IsValid.Should().BeTrue();
    }
}
