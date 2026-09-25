using ControleFinanceiro.Aplicacao.Contratos;
using ControleFinanceiro.Aplicacao.Contratos.Repositorios;
using ControleFinanceiro.Aplicacao.Dtos.Respostas;
using ControleFinanceiro.Dominio.Entidades;
using ControleFinanceiro.Dominio.Excecoes;
using ControleFinanceiro.Dominio.ObjetosDeValor;
using FluentValidation;

namespace ControleFinanceiro.Aplicacao.CasosDeUso.TetosDeGasto.DefinirTetosDoMes;

public sealed class DefinirTetosDoMesManipulador
{
    private readonly IRepositorioDeTetosDeGasto _repositorioDeTetos;
    private readonly IRepositorioDeCategorias _repositorioDeCategorias;
    private readonly IProvedorDoUsuarioAtual _provedorDoUsuarioAtual;
    private readonly IValidator<DefinirTetosDoMesComando> _validador;

    public DefinirTetosDoMesManipulador(
        IRepositorioDeTetosDeGasto repositorioDeTetos,
        IRepositorioDeCategorias repositorioDeCategorias,
        IProvedorDoUsuarioAtual provedorDoUsuarioAtual,
        IValidator<DefinirTetosDoMesComando> validador)
    {
        _repositorioDeTetos = repositorioDeTetos;
        _repositorioDeCategorias = repositorioDeCategorias;
        _provedorDoUsuarioAtual = provedorDoUsuarioAtual;
        _validador = validador;
    }

    public async Task<IReadOnlyList<RespostaDeTetoDeGasto>> ManipularAsync(
        DefinirTetosDoMesComando comando,
        CancellationToken cancelamento)
    {
        await _validador.ValidateAndThrowAsync(comando, cancelamento);

        var competencia = CompetenciaMensal.Analisar(comando.Competencia);
        var usuarioId = _provedorDoUsuarioAtual.UsuarioId;

        var categoriasAtivas = await _repositorioDeCategorias.ListarAsync(
            usuarioId,
            apenasAtivas: true,
            cancelamento);

        var idsAtivos = categoriasAtivas.Select(categoria => categoria.Id).ToHashSet();

        var categoriaDesconhecida = comando.Limites
            .Select(limite => limite.CategoriaId)
            .FirstOrDefault(id => !idsAtivos.Contains(id));

        RegraDeNegocioVioladaException.LancarSe(
            categoriaDesconhecida != Guid.Empty,
            $"A categoria {categoriaDesconhecida} não existe ou está desativada.");

        var tetos = comando.Limites
            .Where(limite => limite.ValorLimite.HasValue)
            .Select(limite => TetoDeGastoMensal.Definir(
                usuarioId,
                limite.CategoriaId,
                competencia,
                limite.ValorLimite!.Value))
            .ToList();

        var categoriasSemTeto = comando.Limites
            .Where(limite => !limite.ValorLimite.HasValue)
            .Select(limite => limite.CategoriaId)
            .ToList();

        await _repositorioDeTetos.SalvarEmLoteAsync(
            usuarioId,
            competencia,
            tetos,
            categoriasSemTeto,
            cancelamento);

        var atualizados = await _repositorioDeTetos.ListarPorCompetenciaAsync(
            usuarioId,
            competencia,
            cancelamento);

        return MontadorDeRespostaDeTetos.Montar(categoriasAtivas, atualizados);
    }
}
