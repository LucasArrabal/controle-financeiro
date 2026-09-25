using ControleFinanceiro.Aplicacao.Dtos.Respostas;
using ControleFinanceiro.Dominio.Entidades;

namespace ControleFinanceiro.Aplicacao.CasosDeUso.Categorias;

/// <summary>Ponto único de tradução de <see cref="CategoriaDeGasto"/> para o DTO de saída.</summary>
internal static class ConversorDeCategoriaParaResposta
{
    public static RespostaDeCategoria Converter(CategoriaDeGasto categoria) =>
        new(
            categoria.Id,
            categoria.Nome,
            categoria.Tipo.ToString(),
            categoria.CorHexadecimal,
            categoria.Ativa);
}
