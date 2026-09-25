namespace ControleFinanceiro.Infraestrutura.Dados.Persistencia.ComandosSql;

/// <summary>
/// SQL do painel mensal. Uma consulta só, e toda a soma acontece no banco: nenhuma despesa
/// individual sobe para o C#.
/// </summary>
internal static class ComandosSqlDoResumoMensal
{
    /// <summary>
    /// Devolve uma linha por categoria, com os dois totais do mês repetidos em cada linha.
    ///
    /// O <c>left join linhas on true</c> no fim é de propósito: <c>totais</c> tem exatamente
    /// uma linha, então a consulta responde os totais mesmo num mês sem nenhuma categoria
    /// para listar — e nesse caso <c>categoria_id</c> volta nulo.
    ///
    /// O filtro <c>(c.ativa or g.valor is not null)</c> mantém no gráfico a categoria que foi
    /// desativada mas ainda tem gasto no mês; sem isso as fatias não somariam 100%.
    /// </summary>
    public const string ObterPorCompetencia =
        """
        with intervalo as (
            select @PrimeiroDia::date as primeiro_dia,
                   @UltimoDia::date   as ultimo_dia
        ),
        gasto_por_categoria as (
            select d.categoria_id,
                   sum(d.valor) as valor
              from despesas d, intervalo i
             where d.usuario_id = @UsuarioId
               and d.data_do_gasto between i.primeiro_dia and i.ultimo_dia
             group by d.categoria_id
        ),
        totais as (
            select (
                       select coalesce(sum(r.valor), 0)
                         from receitas r, intervalo i
                        where r.usuario_id = @UsuarioId
                          and r.data_do_recebimento between i.primeiro_dia and i.ultimo_dia
                   ) as total_de_receitas,
                   (
                       select coalesce(sum(valor), 0)
                         from gasto_por_categoria
                   ) as total_de_despesas
        ),
        linhas as (
            select c.id              as categoria_id,
                   c.nome            as nome_da_categoria,
                   c.cor_hexadecimal as cor_hexadecimal,
                   coalesce(g.valor, 0) as valor_gasto,
                   t.valor_limite    as teto_definido
              from categorias_de_gasto c
              left join gasto_por_categoria g
                     on g.categoria_id = c.id
              left join tetos_de_gasto_mensal t
                     on t.categoria_id = c.id
                    and t.usuario_id = @UsuarioId
                    and t.competencia = @Competencia
             where c.usuario_id = @UsuarioId
               and (c.ativa or g.valor is not null)
        )
        select totais.total_de_receitas,
               totais.total_de_despesas,
               linhas.categoria_id,
               linhas.nome_da_categoria,
               linhas.cor_hexadecimal,
               linhas.valor_gasto,
               linhas.teto_definido
          from totais
          left join linhas on true
         order by linhas.valor_gasto desc nulls last, linhas.nome_da_categoria;
        """;
}
