using ControleFinanceiro.Aplicacao.Contratos;
using ControleFinanceiro.Aplicacao.Contratos.Repositorios;
using ControleFinanceiro.Aplicacao.Dtos.Respostas;
using ControleFinanceiro.Aplicacao.Excecoes;
using ControleFinanceiro.Dominio.Entidades;
using ControleFinanceiro.Dominio.Excecoes;
using ControleFinanceiro.Dominio.ObjetosDeValor;
using FluentValidation;

namespace ControleFinanceiro.Aplicacao.CasosDeUso.Receitas.ReplicarReceitaNosMesesSeguintes;

public sealed class ReplicarReceitaNosMesesSeguintesManipulador
{
    private readonly IRepositorioDeReceitas _repositorioDeReceitas;
    private readonly IProvedorDoUsuarioAtual _provedorDoUsuarioAtual;
    private readonly IProvedorDeDataHora _provedorDeDataHora;
    private readonly IValidator<ReplicarReceitaNosMesesSeguintesComando> _validador;

    public ReplicarReceitaNosMesesSeguintesManipulador(
        IRepositorioDeReceitas repositorioDeReceitas,
        IProvedorDoUsuarioAtual provedorDoUsuarioAtual,
        IProvedorDeDataHora provedorDeDataHora,
        IValidator<ReplicarReceitaNosMesesSeguintesComando> validador)
    {
        _repositorioDeReceitas = repositorioDeReceitas;
        _provedorDoUsuarioAtual = provedorDoUsuarioAtual;
        _provedorDeDataHora = provedorDeDataHora;
        _validador = validador;
    }

    public async Task<RespostaDaReplicacaoDeReceita> ManipularAsync(
        ReplicarReceitaNosMesesSeguintesComando comando,
        CancellationToken cancelamento)
    {
        await _validador.ValidateAndThrowAsync(comando, cancelamento);

        var usuarioId = _provedorDoUsuarioAtual.UsuarioId;

        var receita = await _repositorioDeReceitas.ObterPorIdAsync(usuarioId, comando.Id, cancelamento)
                      ?? throw new RecursoNaoEncontradoException("Receita", comando.Id);

        RegraDeNegocioVioladaException.LancarSe(
            !receita.Recorrente,
            "Só receitas marcadas como recorrentes podem ser replicadas.");

        var competenciasAlvo = MontarCompetenciasSeguintes(receita.Competencia, comando.QuantidadeDeMeses);

        // Replicar duas vezes o mesmo salário é o erro mais provável aqui, então os meses que
        // já têm um lançamento de mesma descrição são pulados em vez de duplicados.
        var jaOcupadas = await _repositorioDeReceitas.ListarCompetenciasComDescricaoAsync(
            usuarioId,
            receita.Descricao,
            competenciasAlvo,
            cancelamento);

        var ocupadas = jaOcupadas.ToHashSet();
        var aCriar = competenciasAlvo.Where(competencia => !ocupadas.Contains(competencia)).ToList();

        var novasReceitas = aCriar
            .Select(competencia => receita.ReplicarPara(competencia, _provedorDeDataHora.AgoraEmUtc))
            .ToList();

        await _repositorioDeReceitas.InserirVariasAsync(novasReceitas, cancelamento);

        return new RespostaDaReplicacaoDeReceita(
            aCriar.Select(competencia => competencia.ToString()).ToList(),
            competenciasAlvo
                .Where(competencia => ocupadas.Contains(competencia))
                .Select(competencia => competencia.ToString())
                .ToList());
    }

    private static List<CompetenciaMensal> MontarCompetenciasSeguintes(
        CompetenciaMensal origem,
        int quantidadeDeMeses) =>
        Enumerable
            .Range(1, quantidadeDeMeses)
            .Aggregate(
                new List<CompetenciaMensal>(quantidadeDeMeses),
                (competencias, _) =>
                {
                    var anterior = competencias.Count == 0 ? origem : competencias[^1];
                    competencias.Add(anterior.Proxima());
                    return competencias;
                });
}
