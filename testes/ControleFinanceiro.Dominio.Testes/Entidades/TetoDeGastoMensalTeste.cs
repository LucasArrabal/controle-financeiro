using ControleFinanceiro.Dominio.Entidades;
using ControleFinanceiro.Dominio.Excecoes;
using ControleFinanceiro.Dominio.ObjetosDeValor;
using FluentAssertions;

namespace ControleFinanceiro.Dominio.Testes.Entidades;

public sealed class TetoDeGastoMensalTeste
{
    private static readonly Guid Usuario = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid Categoria = Guid.Parse("a1000000-0000-4000-8000-000000000004");
    private static readonly CompetenciaMensal Setembro = CompetenciaMensal.Criar(2026, 9);

    [Fact]
    public void DeveAceitarTetoZero()
    {
        // Zero é um limite legítimo: "nenhum gasto permitido nesta categoria".
        TetoDeGastoMensal
            .Definir(Usuario, Categoria, Setembro, 0m)
            .ValorLimite.Quantia
            .Should()
            .Be(0m);
    }

    [Fact]
    public void DeveRejeitarTetoNegativo()
    {
        var acao = () => TetoDeGastoMensal.Definir(Usuario, Categoria, Setembro, -1m);

        acao.Should()
            .Throw<RegraDeNegocioVioladaException>()
            .WithMessage("*não pode ser negativo*");
    }

    [Fact]
    public void DeveRejeitarCategoriaVazia()
    {
        var acao = () => TetoDeGastoMensal.Definir(Usuario, Guid.Empty, Setembro, 100m);

        acao.Should().Throw<RegraDeNegocioVioladaException>();
    }

    [Fact]
    public void AlterarLimiteDeveAplicarAsMesmasRegras()
    {
        var teto = TetoDeGastoMensal.Definir(Usuario, Categoria, Setembro, 400m);

        var acao = () => teto.AlterarLimite(-5m);

        acao.Should().Throw<RegraDeNegocioVioladaException>();
    }

    [Fact]
    public void CopiarParaDeveManterCategoriaELimiteETrocarACompetencia()
    {
        var original = TetoDeGastoMensal.Definir(Usuario, Categoria, Setembro, 400m);

        var copia = original.CopiarPara(CompetenciaMensal.Criar(2026, 10));

        copia.CategoriaId.Should().Be(Categoria);
        copia.ValorLimite.Should().Be(original.ValorLimite);
        copia.Competencia.ToString().Should().Be("2026-10");
        copia.Id.Should().NotBe(original.Id);
    }
}
