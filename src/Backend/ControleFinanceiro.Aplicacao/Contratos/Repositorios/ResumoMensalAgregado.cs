namespace ControleFinanceiro.Aplicacao.Contratos.Repositorios;

/// <summary>
/// Números crus do mês, já somados pelo banco. Os percentuais e a situação de cada teto
/// não vêm daqui — são regra de negócio e ficam no domínio.
/// </summary>
public sealed record ResumoMensalAgregado(
    decimal TotalDeReceitas,
    decimal TotalDeDespesas,
    IReadOnlyList<GastoDeCategoriaAgregado> GastosPorCategoria);

/// <summary>
/// Gasto somado de uma categoria no mês. <see cref="TetoDefinido"/> nulo é "sem teto";
/// zero é um teto de verdade.
/// </summary>
public sealed record GastoDeCategoriaAgregado(
    Guid CategoriaId,
    string NomeDaCategoria,
    string CorHexadecimal,
    decimal ValorGasto,
    decimal? TetoDefinido);
