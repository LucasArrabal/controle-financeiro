namespace ControleFinanceiro.Dominio.RegrasDeNegocio;

/// <summary>
/// Percentuais do painel. Toda divisão por zero devolve 0 — o painel precisa renderizar
/// mesmo num mês sem nenhum lançamento.
/// </summary>
public static class CalculadoraDePercentual
{
    public const int CasasDecimais = 2;

    /// <summary>
    /// Quanto <paramref name="parte"/> representa de <paramref name="total"/>, em pontos percentuais.
    /// Devolve 0 quando o total é zero ou negativo.
    /// </summary>
    public static decimal DeParticipacao(decimal parte, decimal total)
    {
        if (total <= 0m)
        {
            return 0m;
        }

        return Math.Round(parte / total * 100m, CasasDecimais, MidpointRounding.AwayFromZero);
    }
}
