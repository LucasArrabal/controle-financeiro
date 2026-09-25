using ControleFinanceiro.Aplicacao.Contratos;
using ControleFinanceiro.Aplicacao.Contratos.Repositorios;
using ControleFinanceiro.Aplicacao.Dtos.Respostas;
using ControleFinanceiro.Dominio.ObjetosDeValor;

namespace ControleFinanceiro.Aplicacao.CasosDeUso.Receitas.ListarReceitasDoMes;

public sealed class ListarReceitasDoMesManipulador
{
    private readonly IRepositorioDeReceitas _repositorioDeReceitas;
    private readonly IProvedorDoUsuarioAtual _provedorDoUsuarioAtual;

    public ListarReceitasDoMesManipulador(
        IRepositorioDeReceitas repositorioDeReceitas,
        IProvedorDoUsuarioAtual provedorDoUsuarioAtual)
    {
        _repositorioDeReceitas = repositorioDeReceitas;
        _provedorDoUsuarioAtual = provedorDoUsuarioAtual;
    }

    public async Task<IReadOnlyList<RespostaDeReceita>> ManipularAsync(
        ListarReceitasDoMesConsulta consulta,
        CancellationToken cancelamento)
    {
        var competencia = CompetenciaMensal.Analisar(consulta.Competencia);

        var receitas = await _repositorioDeReceitas.ListarPorCompetenciaAsync(
            _provedorDoUsuarioAtual.UsuarioId,
            competencia,
            cancelamento);

        return receitas.Select(ConversorDeReceitaParaResposta.Converter).ToList();
    }
}
