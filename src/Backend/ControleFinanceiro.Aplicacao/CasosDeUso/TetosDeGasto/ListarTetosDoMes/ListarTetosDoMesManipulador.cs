using ControleFinanceiro.Aplicacao.Contratos;
using ControleFinanceiro.Aplicacao.Contratos.Repositorios;
using ControleFinanceiro.Aplicacao.Dtos.Respostas;
using ControleFinanceiro.Dominio.ObjetosDeValor;

namespace ControleFinanceiro.Aplicacao.CasosDeUso.TetosDeGasto.ListarTetosDoMes;

public sealed class ListarTetosDoMesManipulador
{
    private readonly IRepositorioDeTetosDeGasto _repositorioDeTetos;
    private readonly IRepositorioDeCategorias _repositorioDeCategorias;
    private readonly IProvedorDoUsuarioAtual _provedorDoUsuarioAtual;

    public ListarTetosDoMesManipulador(
        IRepositorioDeTetosDeGasto repositorioDeTetos,
        IRepositorioDeCategorias repositorioDeCategorias,
        IProvedorDoUsuarioAtual provedorDoUsuarioAtual)
    {
        _repositorioDeTetos = repositorioDeTetos;
        _repositorioDeCategorias = repositorioDeCategorias;
        _provedorDoUsuarioAtual = provedorDoUsuarioAtual;
    }

    public async Task<IReadOnlyList<RespostaDeTetoDeGasto>> ManipularAsync(
        ListarTetosDoMesConsulta consulta,
        CancellationToken cancelamento)
    {
        var competencia = CompetenciaMensal.Analisar(consulta.Competencia);
        var usuarioId = _provedorDoUsuarioAtual.UsuarioId;

        var categorias = await _repositorioDeCategorias.ListarAsync(
            usuarioId,
            apenasAtivas: true,
            cancelamento);

        var tetos = await _repositorioDeTetos.ListarPorCompetenciaAsync(
            usuarioId,
            competencia,
            cancelamento);

        return MontadorDeRespostaDeTetos.Montar(categorias, tetos);
    }
}
