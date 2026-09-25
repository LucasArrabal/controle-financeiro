using Npgsql;

namespace ControleFinanceiro.Infraestrutura.Dados.Persistencia.Conexao;

/// <summary>
/// Entrega conexões a partir de um <see cref="NpgsqlDataSource"/> único, que é quem
/// mantém o pool. Registrada como singleton; cada chamada devolve uma conexão já aberta
/// que o chamador descarta.
/// </summary>
public sealed class FabricaDeConexaoPostgres : IFabricaDeConexaoPostgres, IAsyncDisposable
{
    private readonly NpgsqlDataSource _fonteDeDados;

    public FabricaDeConexaoPostgres(string stringDeConexao)
    {
        if (string.IsNullOrWhiteSpace(stringDeConexao))
        {
            throw new InvalidOperationException(
                "String de conexão do PostgreSQL não foi informada. " +
                "Defina a variável de ambiente CONTROLEFINANCEIRO_STRING_DE_CONEXAO.");
        }

        var construtor = new NpgsqlDataSourceBuilder(stringDeConexao);
        _fonteDeDados = construtor.Build();
    }

    public async Task<NpgsqlConnection> AbrirConexaoAsync(CancellationToken cancelamento) =>
        await _fonteDeDados.OpenConnectionAsync(cancelamento);

    public ValueTask DisposeAsync() => _fonteDeDados.DisposeAsync();
}
