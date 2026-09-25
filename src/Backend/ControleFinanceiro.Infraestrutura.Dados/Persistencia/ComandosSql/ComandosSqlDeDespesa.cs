namespace ControleFinanceiro.Infraestrutura.Dados.Persistencia.ComandosSql;

/// <summary>
/// SQL de <c>despesas</c>. O filtro do mês é feito por intervalo de datas (e não por
/// <c>extract</c> ou <c>to_char</c>) justamente para usar o índice de <c>data_do_gasto</c>.
/// </summary>
internal static class ComandosSqlDeDespesa
{
    public const string ListarPorCompetencia =
        """
        select id,
               usuario_id,
               descricao,
               valor,
               data_do_gasto,
               categoria_id,
               forma_de_pagamento,
               observacao,
               criado_em
          from despesas
         where usuario_id = @UsuarioId
           and data_do_gasto >= @PrimeiroDia
           and data_do_gasto <= @UltimoDia
         order by data_do_gasto desc, criado_em desc;
        """;

    public const string ObterPorId =
        """
        select id,
               usuario_id,
               descricao,
               valor,
               data_do_gasto,
               categoria_id,
               forma_de_pagamento,
               observacao,
               criado_em
          from despesas
         where usuario_id = @UsuarioId
           and id = @Id;
        """;

    public const string Inserir =
        """
        insert into despesas (
            id, usuario_id, descricao, valor, data_do_gasto,
            categoria_id, forma_de_pagamento, observacao, criado_em)
        values (
            @Id, @UsuarioId, @Descricao, @Valor, @DataDoGasto,
            @CategoriaId, @FormaDePagamento, @Observacao, @CriadoEm);
        """;

    public const string Atualizar =
        """
        update despesas
           set descricao = @Descricao,
               valor = @Valor,
               data_do_gasto = @DataDoGasto,
               categoria_id = @CategoriaId,
               forma_de_pagamento = @FormaDePagamento,
               observacao = @Observacao
         where usuario_id = @UsuarioId
           and id = @Id;
        """;

    public const string Excluir =
        """
        delete from despesas
         where usuario_id = @UsuarioId
           and id = @Id;
        """;
}
