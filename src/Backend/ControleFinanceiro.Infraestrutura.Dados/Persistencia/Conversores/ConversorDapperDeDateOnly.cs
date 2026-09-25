using System.Data;
using System.Globalization;
using Dapper;

namespace ControleFinanceiro.Infraestrutura.Dados.Persistencia.Conversores;

/// <summary>
/// Amarra <see cref="DateOnly"/> à coluna <c>date</c> do PostgreSQL. Sem isso o parâmetro
/// sairia como <c>timestamp</c> e o filtro do mês deixaria de usar o índice de data.
/// </summary>
public sealed class ConversorDapperDeDateOnly : SqlMapper.TypeHandler<DateOnly>
{
    public override void SetValue(IDbDataParameter parametro, DateOnly valor)
    {
        parametro.DbType = DbType.Date;
        parametro.Value = valor.ToDateTime(TimeOnly.MinValue);
    }

    public override DateOnly Parse(object valor) => valor switch
    {
        DateOnly data => data,
        DateTime dataHora => DateOnly.FromDateTime(dataHora),
        string texto => DateOnly.Parse(texto, CultureInfo.InvariantCulture),
        _ => throw new InvalidCastException(
            $"Não foi possível converter '{valor.GetType().Name}' para DateOnly.")
    };
}
