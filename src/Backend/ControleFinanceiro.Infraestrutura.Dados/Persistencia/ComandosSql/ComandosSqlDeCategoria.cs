namespace ControleFinanceiro.Infraestrutura.Dados.Persistencia.ComandosSql;

/// <summary>
/// SQL de <c>categorias_de_gasto</c>. Fica tudo aqui, em constante nomeada, para o repositório
/// mostrar só a orquestração e para as consultas serem revisáveis num arquivo só.
/// </summary>
internal static class ComandosSqlDeCategoria
{
    public const string Listar =
        """
        select id,
               usuario_id,
               nome,
               tipo,
               cor_hexadecimal,
               ativa
          from categorias_de_gasto
         where usuario_id = @UsuarioId
           and (@ApenasAtivas = false or ativa = true)
         order by nome;
        """;

    public const string ObterPorId =
        """
        select id,
               usuario_id,
               nome,
               tipo,
               cor_hexadecimal,
               ativa
          from categorias_de_gasto
         where usuario_id = @UsuarioId
           and id = @Id;
        """;

    public const string ExisteComMesmoNome =
        """
        select exists (
            select 1
              from categorias_de_gasto
             where usuario_id = @UsuarioId
               and lower(nome) = lower(@Nome)
               and (@IdIgnorado::uuid is null or id <> @IdIgnorado::uuid)
        );
        """;

    public const string Inserir =
        """
        insert into categorias_de_gasto (id, usuario_id, nome, tipo, cor_hexadecimal, ativa)
        values (@Id, @UsuarioId, @Nome, @Tipo, @CorHexadecimal, @Ativa);
        """;

    public const string Atualizar =
        """
        update categorias_de_gasto
           set nome = @Nome,
               tipo = @Tipo,
               cor_hexadecimal = @CorHexadecimal,
               ativa = @Ativa
         where usuario_id = @UsuarioId
           and id = @Id;
        """;
}
