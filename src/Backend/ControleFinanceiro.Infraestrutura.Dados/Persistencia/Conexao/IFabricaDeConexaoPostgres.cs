using Npgsql;

namespace ControleFinanceiro.Infraestrutura.Dados.Persistencia.Conexao;

/// <summary>
/// Fonte das conexões dos repositórios. Fica na infraestrutura de propósito:
/// a camada de Aplicação não sabe que existe PostgreSQL.
/// </summary>
public interface IFabricaDeConexaoPostgres
{
    Task<NpgsqlConnection> AbrirConexaoAsync(CancellationToken cancelamento);
}
