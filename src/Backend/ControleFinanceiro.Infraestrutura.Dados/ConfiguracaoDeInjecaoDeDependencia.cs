using ControleFinanceiro.Aplicacao.Contratos;
using ControleFinanceiro.Aplicacao.Contratos.Repositorios;
using ControleFinanceiro.Infraestrutura.Dados.Identidade;
using ControleFinanceiro.Infraestrutura.Dados.Persistencia.Conexao;
using ControleFinanceiro.Infraestrutura.Dados.Persistencia.Consultas;
using ControleFinanceiro.Infraestrutura.Dados.Persistencia.Conversores;
using ControleFinanceiro.Infraestrutura.Dados.Persistencia.Diagnostico;
using ControleFinanceiro.Infraestrutura.Dados.Persistencia.Repositorios;
using ControleFinanceiro.Infraestrutura.Dados.Relogio;
using Dapper;
using Microsoft.Extensions.DependencyInjection;

namespace ControleFinanceiro.Infraestrutura.Dados;

/// <summary>
/// Único ponto em que a API toma conhecimento de que a persistência é PostgreSQL + Dapper.
/// </summary>
public static class ConfiguracaoDeInjecaoDeDependencia
{
    /// <summary>Nome da verificação exposta em <c>/health</c>.</summary>
    public const string NomeDaVerificacaoDeSaudeDoBanco = "postgres";

    public static IServiceCollection AdicionarPersistenciaPostgres(
        this IServiceCollection servicos,
        string stringDeConexao)
    {
        ConfigurarDapper();

        servicos.AddSingleton<IFabricaDeConexaoPostgres>(
            _ => new FabricaDeConexaoPostgres(stringDeConexao));

        servicos.AddScoped<IRepositorioDeCategorias, RepositorioDeCategorias>();
        servicos.AddScoped<IRepositorioDeDespesas, RepositorioDeDespesas>();
        servicos.AddScoped<IRepositorioDeReceitas, RepositorioDeReceitas>();
        servicos.AddScoped<IRepositorioDeTetosDeGasto, RepositorioDeTetosDeGasto>();
        servicos.AddScoped<IConsultaDeResumoMensal, ConsultaDeResumoMensal>();

        servicos.AddSingleton<IProvedorDeDataHora, ProvedorDeDataHoraDoSistema>();
        servicos.AddSingleton<IProvedorDoUsuarioAtual, ProvedorDoUsuarioPadrao>();

        servicos.AddHealthChecks()
            .AddCheck<VerificacaoDeSaudeDoBanco>(NomeDaVerificacaoDeSaudeDoBanco);

        return servicos;
    }

    private static void ConfigurarDapper()
    {
        // As colunas usam snake_case e as propriedades PascalCase; sem isso cada select
        // precisaria de "as" em toda coluna composta.
        DefaultTypeMap.MatchNamesWithUnderscores = true;

        SqlMapper.AddTypeHandler(new ConversorDapperDeDateOnly());
    }
}
