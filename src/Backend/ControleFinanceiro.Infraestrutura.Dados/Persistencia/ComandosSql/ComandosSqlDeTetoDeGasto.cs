namespace ControleFinanceiro.Infraestrutura.Dados.Persistencia.ComandosSql;

/// <summary>
/// SQL de <c>tetos_de_gasto_mensal</c>. A gravação é um upsert apoiado no índice único
/// (categoria_id, competencia), então salvar a tela duas vezes não cria linha duplicada.
/// </summary>
internal static class ComandosSqlDeTetoDeGasto
{
    public const string ListarPorCompetencia =
        """
        select id,
               usuario_id,
               categoria_id,
               competencia,
               valor_limite
          from tetos_de_gasto_mensal
         where usuario_id = @UsuarioId
           and competencia = @Competencia;
        """;

    public const string Salvar =
        """
        insert into tetos_de_gasto_mensal (id, usuario_id, categoria_id, competencia, valor_limite)
        values (@Id, @UsuarioId, @CategoriaId, @Competencia, @ValorLimite)
        on conflict (categoria_id, competencia)
        do update set valor_limite = excluded.valor_limite;
        """;

    public const string RemoverDaCategoria =
        """
        delete from tetos_de_gasto_mensal
         where usuario_id = @UsuarioId
           and competencia = @Competencia
           and categoria_id = any(@CategoriaIds);
        """;
}
