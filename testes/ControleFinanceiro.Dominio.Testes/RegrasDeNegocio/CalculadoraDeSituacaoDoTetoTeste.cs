using ControleFinanceiro.Dominio.Enumeradores;
using ControleFinanceiro.Dominio.RegrasDeNegocio;
using FluentAssertions;

namespace ControleFinanceiro.Dominio.Testes.RegrasDeNegocio;

/// <summary>
/// A regra é: até 80% do teto verde, acima de 80% e até 100% amarelo, acima de 100% vermelho.
/// Os limites exatos (80 e 100) são o que mais importa aqui — é onde um erro de <c>&lt;</c> por
/// <c>&lt;=</c> passaria despercebido.
/// </summary>
public sealed class CalculadoraDeSituacaoDoTetoTeste
{
    [Theory]
    [InlineData(0, 1000)]      // nada gasto
    [InlineData(500, 1000)]    // metade
    [InlineData(799.99, 1000)] // um centavo antes do limite
    [InlineData(800, 1000)]    // exatamente 80%: ainda dentro do limite
    public void DeveFicarDentroDoLimiteAteOitentaPorCento(decimal valorGasto, decimal teto)
    {
        CalculadoraDeSituacaoDoTeto
            .Calcular(valorGasto, teto)
            .Should()
            .Be(SituacaoDoTeto.DentroDoLimite);
    }

    [Theory]
    [InlineData(800.01, 1000)] // um centavo depois de 80%
    [InlineData(900, 1000)]
    [InlineData(1000, 1000)]   // exatamente no teto: atenção, não estouro
    public void DeveEntrarEmAtencaoEntreOitentaECemPorCento(decimal valorGasto, decimal teto)
    {
        CalculadoraDeSituacaoDoTeto
            .Calcular(valorGasto, teto)
            .Should()
            .Be(SituacaoDoTeto.EmAtencao);
    }

    [Theory]
    [InlineData(1000.01, 1000)] // um centavo acima do teto
    [InlineData(1500, 1000)]
    public void DeveEstourarAcimaDeCemPorCento(decimal valorGasto, decimal teto)
    {
        CalculadoraDeSituacaoDoTeto
            .Calcular(valorGasto, teto)
            .Should()
            .Be(SituacaoDoTeto.Estourado);
    }

    [Fact]
    public void TetoZeroComGastoDeveEstourar()
    {
        // Teto zero significa "nenhum gasto permitido": qualquer centavo já estoura.
        CalculadoraDeSituacaoDoTeto
            .Calcular(valorGasto: 0.01m, valorDoTeto: 0m)
            .Should()
            .Be(SituacaoDoTeto.Estourado);
    }

    [Fact]
    public void TetoZeroSemGastoDeveFicarDentroDoLimite()
    {
        CalculadoraDeSituacaoDoTeto
            .Calcular(valorGasto: 0m, valorDoTeto: 0m)
            .Should()
            .Be(SituacaoDoTeto.DentroDoLimite);
    }

    [Fact]
    public void TetoNegativoDeveSerTratadoComoTetoZero()
    {
        // O domínio não deixa gravar teto negativo, mas a calculadora é pura e não pode
        // devolver lixo se receber um.
        CalculadoraDeSituacaoDoTeto
            .Calcular(valorGasto: 10m, valorDoTeto: -100m)
            .Should()
            .Be(SituacaoDoTeto.Estourado);
    }

    [Theory]
    [InlineData(0, SituacaoDoTeto.DentroDoLimite)]
    [InlineData(80, SituacaoDoTeto.DentroDoLimite)]
    [InlineData(80.01, SituacaoDoTeto.EmAtencao)]
    [InlineData(100, SituacaoDoTeto.EmAtencao)]
    [InlineData(100.01, SituacaoDoTeto.Estourado)]
    public void DeveClassificarPeloPercentualConsumido(
        decimal percentualConsumido,
        SituacaoDoTeto esperada)
    {
        CalculadoraDeSituacaoDoTeto
            .CalcularPeloPercentualConsumido(percentualConsumido)
            .Should()
            .Be(esperada);
    }
}
