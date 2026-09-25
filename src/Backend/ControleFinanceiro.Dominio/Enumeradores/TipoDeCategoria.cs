namespace ControleFinanceiro.Dominio.Enumeradores;

/// <summary>
/// Natureza do gasto agrupado por uma categoria. Persistido como texto na coluna
/// <c>categorias_de_gasto.tipo</c> para manter o banco legível.
/// </summary>
public enum TipoDeCategoria
{
    GastoFixo = 1,
    Mercado = 2,
    Essencial = 3,
    Lazer = 4
}
