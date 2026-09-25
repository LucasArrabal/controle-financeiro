using ControleFinanceiro.Aplicacao.Contratos;
using ControleFinanceiro.Aplicacao.Contratos.Repositorios;
using ControleFinanceiro.Aplicacao.Dtos.Respostas;
using ControleFinanceiro.Dominio.ObjetosDeValor;

namespace ControleFinanceiro.Aplicacao.CasosDeUso.Despesas.ListarDespesasDoMes;

public sealed class ListarDespesasDoMesManipulador
{
    private readonly IRepositorioDeDespesas _repositorioDeDespesas;
    private readonly IRepositorioDeCategorias _repositorioDeCategorias;
    private readonly IProvedorDoUsuarioAtual _provedorDoUsuarioAtual;

    public ListarDespesasDoMesManipulador(
        IRepositorioDeDespesas repositorioDeDespesas,
        IRepositorioDeCategorias repositorioDeCategorias,
        IProvedorDoUsuarioAtual provedorDoUsuarioAtual)
    {
        _repositorioDeDespesas = repositorioDeDespesas;
        _repositorioDeCategorias = repositorioDeCategorias;
        _provedorDoUsuarioAtual = provedorDoUsuarioAtual;
    }

    public async Task<IReadOnlyList<RespostaDeDespesa>> ManipularAsync(
        ListarDespesasDoMesConsulta consulta,
        CancellationToken cancelamento)
    {
        var competencia = CompetenciaMensal.Analisar(consulta.Competencia);
        var usuarioId = _provedorDoUsuarioAtual.UsuarioId;

        var despesas = await _repositorioDeDespesas.ListarPorCompetenciaAsync(
            usuarioId,
            competencia,
            cancelamento);

        if (despesas.Count == 0)
        {
            return [];
        }

        // Inclui as inativas: um gasto antigo continua apontando para a categoria desativada
        // e a tabela do mês precisa exibir o nome e a cor dela.
        var categorias = await _repositorioDeCategorias.ListarAsync(
            usuarioId,
            apenasAtivas: false,
            cancelamento);

        var categoriasPorId = categorias.ToDictionary(categoria => categoria.Id);

        return despesas
            .Select(despesa => ConversorDeDespesaParaResposta.Converter(
                despesa,
                categoriasPorId[despesa.CategoriaId]))
            .ToList();
    }
}
