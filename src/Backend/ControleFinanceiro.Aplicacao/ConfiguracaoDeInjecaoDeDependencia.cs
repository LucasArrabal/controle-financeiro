using ControleFinanceiro.Aplicacao.CasosDeUso.Categorias.CriarCategoria;
using ControleFinanceiro.Aplicacao.CasosDeUso.Categorias.DesativarCategoria;
using ControleFinanceiro.Aplicacao.CasosDeUso.Categorias.EditarCategoria;
using ControleFinanceiro.Aplicacao.CasosDeUso.Categorias.ListarCategorias;
using ControleFinanceiro.Aplicacao.CasosDeUso.Despesas.EditarDespesa;
using ControleFinanceiro.Aplicacao.CasosDeUso.Despesas.ExcluirDespesa;
using ControleFinanceiro.Aplicacao.CasosDeUso.Despesas.ListarDespesasDoMes;
using ControleFinanceiro.Aplicacao.CasosDeUso.Despesas.RegistrarDespesa;
using ControleFinanceiro.Aplicacao.CasosDeUso.PainelResumoMensal;
using ControleFinanceiro.Aplicacao.CasosDeUso.Receitas.EditarReceita;
using ControleFinanceiro.Aplicacao.CasosDeUso.Receitas.ExcluirReceita;
using ControleFinanceiro.Aplicacao.CasosDeUso.Receitas.ListarReceitasDoMes;
using ControleFinanceiro.Aplicacao.CasosDeUso.Receitas.RegistrarReceita;
using ControleFinanceiro.Aplicacao.CasosDeUso.Receitas.ReplicarReceitaNosMesesSeguintes;
using ControleFinanceiro.Aplicacao.CasosDeUso.TetosDeGasto.CopiarTetosDoMesAnterior;
using ControleFinanceiro.Aplicacao.CasosDeUso.TetosDeGasto.DefinirTetosDoMes;
using ControleFinanceiro.Aplicacao.CasosDeUso.TetosDeGasto.ListarTetosDoMes;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace ControleFinanceiro.Aplicacao;

/// <summary>
/// Registra os casos de uso e seus validadores. Fica aqui, e não no Program da API,
/// para que um caso de uso novo seja adicionado no mesmo projeto em que foi escrito.
/// </summary>
public static class ConfiguracaoDeInjecaoDeDependencia
{
    public static IServiceCollection AdicionarCasosDeUso(this IServiceCollection servicos)
    {
        servicos.AddValidatorsFromAssemblyContaining<CriarCategoriaValidador>(
            lifetime: ServiceLifetime.Scoped,
            includeInternalTypes: false);

        servicos.AddScoped<ListarCategoriasManipulador>();
        servicos.AddScoped<CriarCategoriaManipulador>();
        servicos.AddScoped<EditarCategoriaManipulador>();
        servicos.AddScoped<DesativarCategoriaManipulador>();

        servicos.AddScoped<ListarDespesasDoMesManipulador>();
        servicos.AddScoped<RegistrarDespesaManipulador>();
        servicos.AddScoped<EditarDespesaManipulador>();
        servicos.AddScoped<ExcluirDespesaManipulador>();

        servicos.AddScoped<ListarReceitasDoMesManipulador>();
        servicos.AddScoped<RegistrarReceitaManipulador>();
        servicos.AddScoped<EditarReceitaManipulador>();
        servicos.AddScoped<ExcluirReceitaManipulador>();
        servicos.AddScoped<ReplicarReceitaNosMesesSeguintesManipulador>();

        servicos.AddScoped<ListarTetosDoMesManipulador>();
        servicos.AddScoped<DefinirTetosDoMesManipulador>();
        servicos.AddScoped<CopiarTetosDoMesAnteriorManipulador>();

        servicos.AddScoped<ObterResumoMensalManipulador>();

        return servicos;
    }
}
