namespace ControleFinanceiro.Infraestrutura.Dados.Persistencia.ComandosSql;

/// <summary>
/// SQL de <c>receitas</c>. Como em despesas, o filtro do mês é por intervalo de datas
/// para usar o índice de <c>data_do_recebimento</c>.
/// </summary>
internal static class ComandosSqlDeReceita
{
    public const string ListarPorCompetencia =
        """
        select id,
               usuario_id,
               descricao,
               valor,
               data_do_recebimento,
               recorrente,
               criado_em
          from receitas
         where usuario_id = @UsuarioId
           and data_do_recebimento >= @PrimeiroDia
           and data_do_recebimento <= @UltimoDia
         order by data_do_recebimento desc, criado_em desc;
        """;

    public const string ObterPorId =
        """
        select id,
               usuario_id,
               descricao,
               valor,
               data_do_recebimento,
               recorrente,
               criado_em
          from receitas
         where usuario_id = @UsuarioId
           and id = @Id;
        """;

    public const string Inserir =
        """
        insert into receitas (
            id, usuario_id, descricao, valor, data_do_recebimento, recorrente, criado_em)
        values (
            @Id, @UsuarioId, @Descricao, @Valor, @DataDoRecebimento, @Recorrente, @CriadoEm);
        """;

    public const string Atualizar =
        """
        update receitas
           set descricao = @Descricao,
               valor = @Valor,
               data_do_recebimento = @DataDoRecebimento,
               recorrente = @Recorrente
         where usuario_id = @UsuarioId
           and id = @Id;
        """;

    public const string Excluir =
        """
        delete from receitas
         where usuario_id = @UsuarioId
           and id = @Id;
        """;

    /// <summary>
    /// Meses, dentro do intervalo informado, que já têm receita com a mesma descrição.
    /// Devolve a competência pronta em texto, para a replicação só comparar strings.
    /// </summary>
    public const string ListarCompetenciasComDescricao =
        """
        select distinct to_char(data_do_recebimento, 'YYYY-MM') as competencia
          from receitas
         where usuario_id = @UsuarioId
           and lower(descricao) = lower(@Descricao)
           and data_do_recebimento >= @PrimeiroDia
           and data_do_recebimento <= @UltimoDia;
        """;
}
