using ControleFinanceiro.Aplicacao.Dtos.Respostas;
using ControleFinanceiro.Dominio.Entidades;

namespace ControleFinanceiro.Aplicacao.CasosDeUso.TetosDeGasto;

/// <summary>
/// Monta a tela de tetos: uma linha por categoria ativa, com o limite quando existir.
/// Categoria sem teto continua aparecendo — é assim que o usuário define o primeiro limite dela.
/// </summary>
internal static class MontadorDeRespostaDeTetos
{
    public static IReadOnlyList<RespostaDeTetoDeGasto> Montar(
        IReadOnlyList<CategoriaDeGasto> categoriasAtivas,
        IReadOnlyList<TetoDeGastoMensal> tetos)
    {
        var limitePorCategoria = tetos.ToDictionary(
            teto => teto.CategoriaId,
            teto => teto.ValorLimite.Quantia);

        return categoriasAtivas
            .Select(categoria => new RespostaDeTetoDeGasto(
                categoria.Id,
                categoria.Nome,
                categoria.CorHexadecimal,
                limitePorCategoria.TryGetValue(categoria.Id, out var limite) ? limite : null))
            .ToList();
    }
}
