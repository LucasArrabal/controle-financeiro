using ControleFinanceiro.Dominio.Excecoes;

namespace ControleFinanceiro.Dominio.ObjetosDeValor;

/// <summary>
/// Quantia em reais. Sempre <see cref="decimal"/> com duas casas — nunca ponto flutuante,
/// para bater exatamente com <c>numeric(14,2)</c> no PostgreSQL.
/// </summary>
public readonly record struct ValorMonetario : IComparable<ValorMonetario>
{
    public const int CasasDecimais = 2;

    /// <summary>Maior quantia representável em <c>numeric(14,2)</c>.</summary>
    public const decimal LimiteMaximo = 999_999_999_999.99m;

    public static readonly ValorMonetario Zero = new(0m);

    private ValorMonetario(decimal quantia) => Quantia = quantia;

    public decimal Quantia { get; }

    public bool EhZero => Quantia == 0m;

    public bool EhPositivo => Quantia > 0m;

    /// <summary>Aceita zero; rejeita valores negativos e acima do limite da coluna.</summary>
    public static ValorMonetario Criar(decimal quantia)
    {
        RegraDeNegocioVioladaException.LancarSe(
            quantia < 0m,
            "Valor monetário não pode ser negativo.");

        RegraDeNegocioVioladaException.LancarSe(
            quantia > LimiteMaximo,
            $"Valor monetário não pode ultrapassar {LimiteMaximo:N2}.");

        return new ValorMonetario(Arredondar(quantia));
    }

    /// <summary>Exige quantia estritamente maior que zero — usado por despesas e receitas.</summary>
    public static ValorMonetario CriarPositivo(decimal quantia)
    {
        RegraDeNegocioVioladaException.LancarSe(
            quantia <= 0m,
            "Valor deve ser maior que zero.");

        return Criar(quantia);
    }

    private static decimal Arredondar(decimal quantia) =>
        Math.Round(quantia, CasasDecimais, MidpointRounding.AwayFromZero);

    public int CompareTo(ValorMonetario outro) => Quantia.CompareTo(outro.Quantia);

    public static ValorMonetario operator +(ValorMonetario esquerda, ValorMonetario direita) =>
        Criar(esquerda.Quantia + direita.Quantia);

    public static bool operator >(ValorMonetario esquerda, ValorMonetario direita) =>
        esquerda.Quantia > direita.Quantia;

    public static bool operator <(ValorMonetario esquerda, ValorMonetario direita) =>
        esquerda.Quantia < direita.Quantia;

    public static bool operator >=(ValorMonetario esquerda, ValorMonetario direita) =>
        esquerda.Quantia >= direita.Quantia;

    public static bool operator <=(ValorMonetario esquerda, ValorMonetario direita) =>
        esquerda.Quantia <= direita.Quantia;

    public override string ToString() => Quantia.ToString("0.00");
}
