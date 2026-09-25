using ControleFinanceiro.Aplicacao.Dtos.Respostas;
using ControleFinanceiro.Dominio.Entidades;

namespace ControleFinanceiro.Aplicacao.CasosDeUso.Despesas;

/// <summary>Ponto único de tradução de <see cref="Despesa"/> para o DTO de saída.</summary>
internal static class ConversorDeDespesaParaResposta
{
    public static RespostaDeDespesa Converter(Despesa despesa, CategoriaDeGasto categoria) =>
        new(
            despesa.Id,
            despesa.Descricao,
            despesa.Valor.Quantia,
            despesa.DataDoGasto,
            despesa.CategoriaId,
            categoria.Nome,
            categoria.CorHexadecimal,
            despesa.FormaDePagamento,
            despesa.Observacao,
            despesa.CriadoEm);
}
