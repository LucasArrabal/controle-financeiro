using ControleFinanceiro.Aplicacao.Contratos.Repositorios;
using ControleFinanceiro.Dominio.Entidades;
using ControleFinanceiro.Dominio.ObjetosDeValor;
using ControleFinanceiro.Infraestrutura.Dados.Persistencia.ComandosSql;
using ControleFinanceiro.Infraestrutura.Dados.Persistencia.Conexao;
using ControleFinanceiro.Infraestrutura.Dados.Persistencia.Repositorios.Registros;
using Dapper;

namespace ControleFinanceiro.Infraestrutura.Dados.Persistencia.Repositorios;

public sealed class RepositorioDeReceitas : IRepositorioDeReceitas
{
    private readonly IFabricaDeConexaoPostgres _fabricaDeConexao;

    public RepositorioDeReceitas(IFabricaDeConexaoPostgres fabricaDeConexao) =>
        _fabricaDeConexao = fabricaDeConexao;

    public async Task<IReadOnlyList<Receita>> ListarPorCompetenciaAsync(
        Guid usuarioId,
        CompetenciaMensal competencia,
        CancellationToken cancelamento)
    {
        await using var conexao = await _fabricaDeConexao.AbrirConexaoAsync(cancelamento);

        var registros = await conexao.QueryAsync<RegistroDeReceita>(
            new CommandDefinition(
                ComandosSqlDeReceita.ListarPorCompetencia,
                new
                {
                    UsuarioId = usuarioId,
                    PrimeiroDia = competencia.PrimeiroDia,
                    UltimoDia = competencia.UltimoDia,
                },
                cancellationToken: cancelamento));

        return registros.Select(Reidratar).ToList();
    }

    public async Task<Receita?> ObterPorIdAsync(
        Guid usuarioId,
        Guid receitaId,
        CancellationToken cancelamento)
    {
        await using var conexao = await _fabricaDeConexao.AbrirConexaoAsync(cancelamento);

        var registro = await conexao.QuerySingleOrDefaultAsync<RegistroDeReceita>(
            new CommandDefinition(
                ComandosSqlDeReceita.ObterPorId,
                new { UsuarioId = usuarioId, Id = receitaId },
                cancellationToken: cancelamento));

        return registro is null ? null : Reidratar(registro);
    }

    public async Task InserirAsync(Receita receita, CancellationToken cancelamento)
    {
        await using var conexao = await _fabricaDeConexao.AbrirConexaoAsync(cancelamento);

        await conexao.ExecuteAsync(
            new CommandDefinition(
                ComandosSqlDeReceita.Inserir,
                MontarParametrosDeInsercao(receita),
                cancellationToken: cancelamento));
    }

    public async Task InserirVariasAsync(
        IReadOnlyCollection<Receita> receitas,
        CancellationToken cancelamento)
    {
        if (receitas.Count == 0)
        {
            return;
        }

        await using var conexao = await _fabricaDeConexao.AbrirConexaoAsync(cancelamento);
        await using var transacao = await conexao.BeginTransactionAsync(cancelamento);

        // Ou entram todos os meses replicados, ou nenhum.
        await conexao.ExecuteAsync(
            new CommandDefinition(
                ComandosSqlDeReceita.Inserir,
                receitas.Select(MontarParametrosDeInsercao).ToList(),
                transaction: transacao,
                cancellationToken: cancelamento));

        await transacao.CommitAsync(cancelamento);
    }

    public async Task AtualizarAsync(Receita receita, CancellationToken cancelamento)
    {
        await using var conexao = await _fabricaDeConexao.AbrirConexaoAsync(cancelamento);

        await conexao.ExecuteAsync(
            new CommandDefinition(
                ComandosSqlDeReceita.Atualizar,
                new
                {
                    receita.Id,
                    receita.UsuarioId,
                    receita.Descricao,
                    Valor = receita.Valor.Quantia,
                    receita.DataDoRecebimento,
                    receita.Recorrente,
                },
                cancellationToken: cancelamento));
    }

    public async Task ExcluirAsync(Guid usuarioId, Guid receitaId, CancellationToken cancelamento)
    {
        await using var conexao = await _fabricaDeConexao.AbrirConexaoAsync(cancelamento);

        await conexao.ExecuteAsync(
            new CommandDefinition(
                ComandosSqlDeReceita.Excluir,
                new { UsuarioId = usuarioId, Id = receitaId },
                cancellationToken: cancelamento));
    }

    public async Task<IReadOnlyList<CompetenciaMensal>> ListarCompetenciasComDescricaoAsync(
        Guid usuarioId,
        string descricao,
        IReadOnlyCollection<CompetenciaMensal> competencias,
        CancellationToken cancelamento)
    {
        if (competencias.Count == 0)
        {
            return [];
        }

        var ordenadas = competencias.OrderBy(competencia => competencia).ToList();

        await using var conexao = await _fabricaDeConexao.AbrirConexaoAsync(cancelamento);

        var encontradas = await conexao.QueryAsync<string>(
            new CommandDefinition(
                ComandosSqlDeReceita.ListarCompetenciasComDescricao,
                new
                {
                    UsuarioId = usuarioId,
                    Descricao = descricao,
                    PrimeiroDia = ordenadas[0].PrimeiroDia,
                    UltimoDia = ordenadas[^1].UltimoDia,
                },
                cancellationToken: cancelamento));

        return encontradas.Select(CompetenciaMensal.Analisar).ToList();
    }

    private static object MontarParametrosDeInsercao(Receita receita) =>
        new
        {
            receita.Id,
            receita.UsuarioId,
            receita.Descricao,
            Valor = receita.Valor.Quantia,
            receita.DataDoRecebimento,
            receita.Recorrente,
            CriadoEm = receita.CriadoEm.UtcDateTime,
        };

    private static Receita Reidratar(RegistroDeReceita registro) =>
        Receita.Restaurar(
            registro.Id,
            registro.UsuarioId,
            registro.Descricao,
            registro.Valor,
            registro.DataDoRecebimento,
            registro.Recorrente,
            new DateTimeOffset(DateTime.SpecifyKind(registro.CriadoEm, DateTimeKind.Utc)));
}
