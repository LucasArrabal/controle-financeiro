using ControleFinanceiro.Aplicacao.Contratos.Repositorios;
using ControleFinanceiro.Dominio.ObjetosDeValor;
using ControleFinanceiro.Infraestrutura.Dados.Persistencia.ComandosSql;
using ControleFinanceiro.Infraestrutura.Dados.Persistencia.Conexao;
using Dapper;

namespace ControleFinanceiro.Infraestrutura.Dados.Persistencia.Consultas;

public sealed class ConsultaDeResumoMensal : IConsultaDeResumoMensal
{
    private readonly IFabricaDeConexaoPostgres _fabricaDeConexao;

    public ConsultaDeResumoMensal(IFabricaDeConexaoPostgres fabricaDeConexao) =>
        _fabricaDeConexao = fabricaDeConexao;

    public async Task<ResumoMensalAgregado> ObterAsync(
        Guid usuarioId,
        CompetenciaMensal competencia,
        CancellationToken cancelamento)
    {
        await using var conexao = await _fabricaDeConexao.AbrirConexaoAsync(cancelamento);

        var linhas = (await conexao.QueryAsync<RegistroDoResumoMensal>(
            new CommandDefinition(
                ComandosSqlDoResumoMensal.ObterPorCompetencia,
                new
                {
                    UsuarioId = usuarioId,
                    Competencia = competencia.ToString(),
                    PrimeiroDia = competencia.PrimeiroDia,
                    UltimoDia = competencia.UltimoDia,
                },
                cancellationToken: cancelamento))).ToList();

        // A consulta sempre devolve ao menos uma linha, mesmo num mês sem categoria alguma.
        if (linhas.Count == 0)
        {
            return new ResumoMensalAgregado(0m, 0m, []);
        }

        var gastos = linhas
            .Where(linha => linha.CategoriaId is not null)
            .Select(linha => new GastoDeCategoriaAgregado(
                linha.CategoriaId!.Value,
                linha.NomeDaCategoria!,
                linha.CorHexadecimal!,
                linha.ValorGasto ?? 0m,
                linha.TetoDefinido))
            .ToList();

        return new ResumoMensalAgregado(
            linhas[0].TotalDeReceitas,
            linhas[0].TotalDeDespesas,
            gastos);
    }

    /// <summary>
    /// Linha crua do resumo. As colunas da categoria são anuláveis porque a consulta responde
    /// os totais mesmo quando não há categoria nenhuma para listar.
    /// </summary>
    private sealed record RegistroDoResumoMensal(
        decimal TotalDeReceitas,
        decimal TotalDeDespesas,
        Guid? CategoriaId,
        string? NomeDaCategoria,
        string? CorHexadecimal,
        decimal? ValorGasto,
        decimal? TetoDefinido);
}
