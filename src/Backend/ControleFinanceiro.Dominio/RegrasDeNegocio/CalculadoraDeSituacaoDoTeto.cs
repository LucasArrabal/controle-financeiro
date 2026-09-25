using ControleFinanceiro.Dominio.Enumeradores;

namespace ControleFinanceiro.Dominio.RegrasDeNegocio;

/// <summary>
/// Traduz "quanto já gastei do meu teto" na cor da barra do painel.
/// Até 80% verde, acima de 80% e até 100% amarelo, acima de 100% vermelho.
/// </summary>
public static class CalculadoraDeSituacaoDoTeto
{
    public const decimal PercentualDeAtencao = 80m;
    public const decimal PercentualDeEstouro = 100m;

    /// <summary>
    /// Situação a partir dos valores brutos. Teto zero significa "sem gasto permitido":
    /// qualquer gasto já estoura; gasto zero continua dentro do limite.
    /// </summary>
    public static SituacaoDoTeto Calcular(decimal valorGasto, decimal valorDoTeto)
    {
        if (valorDoTeto <= 0m)
        {
            return valorGasto > 0m ? SituacaoDoTeto.Estourado : SituacaoDoTeto.DentroDoLimite;
        }

        // Compara os valores direto, sem passar pelo percentual arredondado: R$ 1.000,01 de um
        // teto de R$ 1.000 dá 100,001%, que arredondado para 100,00% seria classificado como
        // "em atenção" — dizendo que está no limite uma categoria que já estourou.
        if (valorGasto > valorDoTeto)
        {
            return SituacaoDoTeto.Estourado;
        }

        var limiteDeAtencao = valorDoTeto * (PercentualDeAtencao / 100m);

        return valorGasto > limiteDeAtencao
            ? SituacaoDoTeto.EmAtencao
            : SituacaoDoTeto.DentroDoLimite;
    }

    /// <summary>
    /// Situação a partir do percentual do teto já consumido. Use <see cref="Calcular"/> quando
    /// os valores estiverem à mão: este caminho herda o arredondamento de quem calculou o percentual.
    /// </summary>
    public static SituacaoDoTeto CalcularPeloPercentualConsumido(decimal percentualConsumido) =>
        percentualConsumido switch
        {
            <= PercentualDeAtencao => SituacaoDoTeto.DentroDoLimite,
            <= PercentualDeEstouro => SituacaoDoTeto.EmAtencao,
            _ => SituacaoDoTeto.Estourado
        };
}
