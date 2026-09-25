using ControleFinanceiro.Aplicacao.Contratos.Repositorios;
using ControleFinanceiro.Dominio.Entidades;
using ControleFinanceiro.Dominio.Enumeradores;
using ControleFinanceiro.Infraestrutura.Dados.Persistencia.ComandosSql;
using ControleFinanceiro.Infraestrutura.Dados.Persistencia.Conexao;
using ControleFinanceiro.Infraestrutura.Dados.Persistencia.Repositorios.Registros;
using Dapper;

namespace ControleFinanceiro.Infraestrutura.Dados.Persistencia.Repositorios;

public sealed class RepositorioDeCategorias : IRepositorioDeCategorias
{
    private readonly IFabricaDeConexaoPostgres _fabricaDeConexao;

    public RepositorioDeCategorias(IFabricaDeConexaoPostgres fabricaDeConexao) =>
        _fabricaDeConexao = fabricaDeConexao;

    public async Task<IReadOnlyList<CategoriaDeGasto>> ListarAsync(
        Guid usuarioId,
        bool apenasAtivas,
        CancellationToken cancelamento)
    {
        await using var conexao = await _fabricaDeConexao.AbrirConexaoAsync(cancelamento);

        var registros = await conexao.QueryAsync<RegistroDeCategoria>(
            new CommandDefinition(
                ComandosSqlDeCategoria.Listar,
                new { UsuarioId = usuarioId, ApenasAtivas = apenasAtivas },
                cancellationToken: cancelamento));

        return registros.Select(Reidratar).ToList();
    }

    public async Task<CategoriaDeGasto?> ObterPorIdAsync(
        Guid usuarioId,
        Guid categoriaId,
        CancellationToken cancelamento)
    {
        await using var conexao = await _fabricaDeConexao.AbrirConexaoAsync(cancelamento);

        var registro = await conexao.QuerySingleOrDefaultAsync<RegistroDeCategoria>(
            new CommandDefinition(
                ComandosSqlDeCategoria.ObterPorId,
                new { UsuarioId = usuarioId, Id = categoriaId },
                cancellationToken: cancelamento));

        return registro is null ? null : Reidratar(registro);
    }

    public async Task<bool> ExisteComMesmoNomeAsync(
        Guid usuarioId,
        string nome,
        Guid? idIgnorado,
        CancellationToken cancelamento)
    {
        await using var conexao = await _fabricaDeConexao.AbrirConexaoAsync(cancelamento);

        return await conexao.ExecuteScalarAsync<bool>(
            new CommandDefinition(
                ComandosSqlDeCategoria.ExisteComMesmoNome,
                new { UsuarioId = usuarioId, Nome = nome, IdIgnorado = idIgnorado },
                cancellationToken: cancelamento));
    }

    public async Task InserirAsync(CategoriaDeGasto categoria, CancellationToken cancelamento)
    {
        await using var conexao = await _fabricaDeConexao.AbrirConexaoAsync(cancelamento);

        await conexao.ExecuteAsync(
            new CommandDefinition(
                ComandosSqlDeCategoria.Inserir,
                MontarParametros(categoria),
                cancellationToken: cancelamento));
    }

    public async Task AtualizarAsync(CategoriaDeGasto categoria, CancellationToken cancelamento)
    {
        await using var conexao = await _fabricaDeConexao.AbrirConexaoAsync(cancelamento);

        await conexao.ExecuteAsync(
            new CommandDefinition(
                ComandosSqlDeCategoria.Atualizar,
                MontarParametros(categoria),
                cancellationToken: cancelamento));
    }

    private static object MontarParametros(CategoriaDeGasto categoria) =>
        new
        {
            categoria.Id,
            categoria.UsuarioId,
            categoria.Nome,
            Tipo = categoria.Tipo.ToString(),
            categoria.CorHexadecimal,
            categoria.Ativa
        };

    private static CategoriaDeGasto Reidratar(RegistroDeCategoria registro) =>
        CategoriaDeGasto.Restaurar(
            registro.Id,
            registro.UsuarioId,
            registro.Nome,
            Enum.Parse<TipoDeCategoria>(registro.Tipo, ignoreCase: true),
            registro.CorHexadecimal,
            registro.Ativa);
}
