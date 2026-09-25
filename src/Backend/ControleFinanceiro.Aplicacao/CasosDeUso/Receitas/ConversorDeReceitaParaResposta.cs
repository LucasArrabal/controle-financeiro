using ControleFinanceiro.Aplicacao.Dtos.Respostas;
using ControleFinanceiro.Dominio.Entidades;

namespace ControleFinanceiro.Aplicacao.CasosDeUso.Receitas;

/// <summary>Ponto único de tradução de <see cref="Receita"/> para o DTO de saída.</summary>
internal static class ConversorDeReceitaParaResposta
{
    public static RespostaDeReceita Converter(Receita receita) =>
        new(
            receita.Id,
            receita.Descricao,
            receita.Valor.Quantia,
            receita.DataDoRecebimento,
            receita.Recorrente,
            receita.CriadoEm);
}
