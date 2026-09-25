using System.Globalization;
using ControleFinanceiro.Dominio.Excecoes;

namespace ControleFinanceiro.Dominio.ObjetosDeValor;

/// <summary>
/// Mês de referência de um lançamento, no formato <c>aaaa-MM</c> (ex.: <c>2026-09</c>).
/// É a chave de filtro de todas as telas e a coluna <c>char(7)</c> de tetos_de_gasto_mensal.
/// </summary>
public readonly record struct CompetenciaMensal : IComparable<CompetenciaMensal>
{
    public const string Formato = "yyyy-MM";
    public const int AnoMinimo = 1900;
    public const int AnoMaximo = 2999;

    private CompetenciaMensal(int ano, int mes)
    {
        Ano = ano;
        Mes = mes;
    }

    public int Ano { get; }

    public int Mes { get; }

    public static CompetenciaMensal Criar(int ano, int mes)
    {
        RegraDeNegocioVioladaException.LancarSe(
            ano is < AnoMinimo or > AnoMaximo,
            $"Ano da competência deve estar entre {AnoMinimo} e {AnoMaximo}.");

        RegraDeNegocioVioladaException.LancarSe(
            mes is < 1 or > 12,
            "Mês da competência deve estar entre 1 e 12.");

        return new CompetenciaMensal(ano, mes);
    }

    /// <summary>Converte <c>"2026-09"</c> em competência. Lança se o texto não bater com o formato.</summary>
    public static CompetenciaMensal Analisar(string texto)
    {
        if (!TentarAnalisar(texto, out var competencia))
        {
            throw new RegraDeNegocioVioladaException(
                $"Competência '{texto}' é inválida. Use o formato {Formato} (ex.: 2026-09).");
        }

        return competencia;
    }

    public static bool TentarAnalisar(string? texto, out CompetenciaMensal competencia)
    {
        competencia = default;

        if (string.IsNullOrWhiteSpace(texto))
        {
            return false;
        }

        var recortado = texto.Trim();

        if (!DateTime.TryParseExact(
                recortado,
                Formato,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var data))
        {
            return false;
        }

        if (data.Year is < AnoMinimo or > AnoMaximo)
        {
            return false;
        }

        competencia = new CompetenciaMensal(data.Year, data.Month);
        return true;
    }

    public static CompetenciaMensal DeData(DateOnly data) => new(data.Year, data.Month);

    public DateOnly PrimeiroDia => new(Ano, Mes, 1);

    public DateOnly UltimoDia => new(Ano, Mes, DateTime.DaysInMonth(Ano, Mes));

    public CompetenciaMensal Anterior()
    {
        var primeiroDiaDoMesAnterior = PrimeiroDia.AddMonths(-1);
        return new CompetenciaMensal(primeiroDiaDoMesAnterior.Year, primeiroDiaDoMesAnterior.Month);
    }

    public CompetenciaMensal Proxima()
    {
        var primeiroDiaDoProximoMes = PrimeiroDia.AddMonths(1);
        return new CompetenciaMensal(primeiroDiaDoProximoMes.Year, primeiroDiaDoProximoMes.Month);
    }

    public int CompareTo(CompetenciaMensal outra)
    {
        var comparacaoDeAno = Ano.CompareTo(outra.Ano);
        return comparacaoDeAno != 0 ? comparacaoDeAno : Mes.CompareTo(outra.Mes);
    }

    public override string ToString() => $"{Ano:D4}-{Mes:D2}";
}
