using ControleFinanceiro.Aplicacao.CasosDeUso.Despesas.RegistrarDespesa;
using ControleFinanceiro.Aplicacao.Testes.Dubles;
using FluentAssertions;
using FluentValidation.Results;

namespace ControleFinanceiro.Aplicacao.Testes.CasosDeUso.Despesas;

public sealed class RegistrarDespesaValidadorTeste
{
    private static readonly DateOnly Hoje = ProvedorDeDataHoraFixo.HojeDeReferencia;
    private static readonly Guid Categoria = Guid.Parse("a1000000-0000-4000-8000-000000000002");

    private readonly RegistrarDespesaValidador _validador = new(new ProvedorDeDataHoraFixo());

    private ValidationResult Validar(
        string descricao = "Mercado",
        decimal valor = 100m,
        DateOnly? dataDoGasto = null,
        Guid? categoriaId = null,
        string? formaDePagamento = null) =>
        _validador.Validate(new RegistrarDespesaComando(
            descricao,
            valor,
            dataDoGasto ?? Hoje,
            categoriaId ?? Categoria,
            formaDePagamento,
            null));

    [Fact]
    public void ComandoCompletoDeveSerValido()
    {
        Validar().IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("x")]
    public void DeveRecusarDescricaoCurta(string descricao)
    {
        Validar(descricao: descricao)
            .Errors.Should()
            .Contain(erro => erro.PropertyName == nameof(RegistrarDespesaComando.Descricao));
    }

    [Fact]
    public void DeveRecusarDescricaoAcimaDeCentoECinquentaCaracteres()
    {
        Validar(descricao: new string('a', 151))
            .Errors.Should()
            .Contain(erro => erro.PropertyName == nameof(RegistrarDespesaComando.Descricao));
    }

    [Fact]
    public void DeveAceitarDescricaoComExatamenteCentoECinquentaCaracteres()
    {
        Validar(descricao: new string('a', 150)).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.01)]
    public void DeveRecusarValorNaoPositivo(decimal valor)
    {
        Validar(valor: valor)
            .Errors.Should()
            .Contain(erro => erro.ErrorMessage.Contains("maior que zero"));
    }

    [Fact]
    public void DeveRecusarDataMaisDeUmAnoNoFuturo()
    {
        Validar(dataDoGasto: Hoje.AddMonths(12).AddDays(1))
            .Errors.Should()
            .Contain(erro => erro.ErrorMessage.Contains("1 ano no futuro"));
    }

    [Fact]
    public void DeveAceitarDataExatamenteUmAnoNoFuturo()
    {
        Validar(dataDoGasto: Hoje.AddMonths(12)).IsValid.Should().BeTrue();
    }

    [Fact]
    public void DeveRecusarCategoriaVazia()
    {
        Validar(categoriaId: Guid.Empty)
            .Errors.Should()
            .Contain(erro => erro.PropertyName == nameof(RegistrarDespesaComando.CategoriaId));
    }

    [Fact]
    public void DeveRecusarFormaDePagamentoAcimaDeTrintaCaracteres()
    {
        Validar(formaDePagamento: new string('a', 31))
            .Errors.Should()
            .Contain(erro => erro.PropertyName == nameof(RegistrarDespesaComando.FormaDePagamento));
    }

    [Fact]
    public void FormaDePagamentoNulaDeveSerAceita()
    {
        Validar(formaDePagamento: null).IsValid.Should().BeTrue();
    }

    [Fact]
    public void DeveAcumularTodosOsErrosDeUmaVez()
    {
        // O formulário mostra tudo o que está errado de uma vez, não um erro por tentativa.
        var resultado = Validar(descricao: "x", valor: 0, categoriaId: Guid.Empty);

        resultado.Errors.Select(erro => erro.PropertyName).Distinct().Should().HaveCount(3);
    }
}
