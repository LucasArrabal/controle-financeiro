using ControleFinanceiro.Dominio.Entidades;
using ControleFinanceiro.Dominio.Excecoes;
using ControleFinanceiro.Dominio.ObjetosDeValor;
using FluentAssertions;

namespace ControleFinanceiro.Dominio.Testes.Entidades;

public sealed class ReceitaTeste
{
    private static readonly DateTimeOffset Agora = new(2026, 9, 21, 12, 0, 0, TimeSpan.Zero);
    private static readonly Guid Usuario = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private static Receita Registrar(
        string descricao = "Salário",
        decimal valor = 7800m,
        DateOnly? dataDoRecebimento = null,
        bool recorrente = true) =>
        Receita.Registrar(
            Usuario,
            descricao,
            valor,
            dataDoRecebimento ?? new DateOnly(2026, 9, 5),
            recorrente,
            Agora);

    [Fact]
    public void DeveRegistrarComOsDadosInformados()
    {
        var receita = Registrar();

        receita.Descricao.Should().Be("Salário");
        receita.Valor.Quantia.Should().Be(7800m);
        receita.Recorrente.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void DeveRejeitarValorNaoPositivo(decimal valor)
    {
        var acao = () => Registrar(valor: valor);

        acao.Should().Throw<RegraDeNegocioVioladaException>();
    }

    [Fact]
    public void DeveAceitarDataNoFuturo()
    {
        // Receita não tem a travar de "1 ano no futuro" da despesa: é o que permite
        // replicar o salário para os meses seguintes.
        var acao = () => Registrar(dataDoRecebimento: new DateOnly(2028, 1, 5));

        acao.Should().NotThrow();
    }

    [Fact]
    public void ReplicarDeveManterDescricaoValorEDia()
    {
        var original = Registrar(dataDoRecebimento: new DateOnly(2026, 9, 5));

        var copia = original.ReplicarPara(CompetenciaMensal.Criar(2026, 10), Agora);

        copia.Descricao.Should().Be(original.Descricao);
        copia.Valor.Should().Be(original.Valor);
        copia.Recorrente.Should().BeTrue();
        copia.DataDoRecebimento.Should().Be(new DateOnly(2026, 10, 5));
    }

    [Fact]
    public void ReplicarDeveGerarIdNovo()
    {
        var original = Registrar();

        original.ReplicarPara(CompetenciaMensal.Criar(2026, 10), Agora)
            .Id.Should().NotBe(original.Id);
    }

    [Fact]
    public void ReplicarDiaTrintaEUmParaMesDeTrintaDiasDeveCairNoUltimoDia()
    {
        // 31 de janeiro replicado para abril não existe: tem de virar 30 de abril,
        // não estourar nem pular para maio.
        var original = Registrar(dataDoRecebimento: new DateOnly(2026, 1, 31));

        original.ReplicarPara(CompetenciaMensal.Criar(2026, 4), Agora)
            .DataDoRecebimento.Should().Be(new DateOnly(2026, 4, 30));
    }

    [Fact]
    public void ReplicarDiaTrintaEUmParaFevereiroDeveCairNoUltimoDia()
    {
        var original = Registrar(dataDoRecebimento: new DateOnly(2026, 1, 31));

        original.ReplicarPara(CompetenciaMensal.Criar(2026, 2), Agora)
            .DataDoRecebimento.Should().Be(new DateOnly(2026, 2, 28));
    }

    [Fact]
    public void CompetenciaDeveSairDaDataDoRecebimento()
    {
        Registrar(dataDoRecebimento: new DateOnly(2026, 12, 20))
            .Competencia.ToString().Should().Be("2026-12");
    }

    [Fact]
    public void AlterarDeveTrocarOsDados()
    {
        var receita = Registrar();

        receita.Alterar("Freela", 1200m, new DateOnly(2026, 9, 12), recorrente: false);

        receita.Descricao.Should().Be("Freela");
        receita.Valor.Quantia.Should().Be(1200m);
        receita.Recorrente.Should().BeFalse();
    }
}
