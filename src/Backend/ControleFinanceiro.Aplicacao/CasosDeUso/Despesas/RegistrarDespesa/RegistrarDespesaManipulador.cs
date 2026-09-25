using ControleFinanceiro.Aplicacao.Contratos;
using ControleFinanceiro.Aplicacao.Contratos.Repositorios;
using ControleFinanceiro.Aplicacao.Dtos.Respostas;
using ControleFinanceiro.Aplicacao.Excecoes;
using ControleFinanceiro.Dominio.Entidades;
using ControleFinanceiro.Dominio.Excecoes;
using FluentValidation;

namespace ControleFinanceiro.Aplicacao.CasosDeUso.Despesas.RegistrarDespesa;

public sealed class RegistrarDespesaManipulador
{
    private readonly IRepositorioDeDespesas _repositorioDeDespesas;
    private readonly IRepositorioDeCategorias _repositorioDeCategorias;
    private readonly IProvedorDoUsuarioAtual _provedorDoUsuarioAtual;
    private readonly IProvedorDeDataHora _provedorDeDataHora;
    private readonly IValidator<RegistrarDespesaComando> _validador;

    public RegistrarDespesaManipulador(
        IRepositorioDeDespesas repositorioDeDespesas,
        IRepositorioDeCategorias repositorioDeCategorias,
        IProvedorDoUsuarioAtual provedorDoUsuarioAtual,
        IProvedorDeDataHora provedorDeDataHora,
        IValidator<RegistrarDespesaComando> validador)
    {
        _repositorioDeDespesas = repositorioDeDespesas;
        _repositorioDeCategorias = repositorioDeCategorias;
        _provedorDoUsuarioAtual = provedorDoUsuarioAtual;
        _provedorDeDataHora = provedorDeDataHora;
        _validador = validador;
    }

    public async Task<RespostaDeDespesa> ManipularAsync(
        RegistrarDespesaComando comando,
        CancellationToken cancelamento)
    {
        await _validador.ValidateAndThrowAsync(comando, cancelamento);

        var usuarioId = _provedorDoUsuarioAtual.UsuarioId;

        var categoria = await _repositorioDeCategorias.ObterPorIdAsync(usuarioId, comando.CategoriaId, cancelamento)
                        ?? throw new RecursoNaoEncontradoException("Categoria", comando.CategoriaId);

        RegraDeNegocioVioladaException.LancarSe(
            !categoria.Ativa,
            $"A categoria '{categoria.Nome}' está desativada e não aceita novos lançamentos.");

        var despesa = Despesa.Registrar(
            usuarioId,
            comando.Descricao,
            comando.Valor,
            comando.DataDoGasto,
            comando.CategoriaId,
            comando.FormaDePagamento,
            comando.Observacao,
            _provedorDeDataHora.HojeNoFusoDoUsuario,
            _provedorDeDataHora.AgoraEmUtc);

        await _repositorioDeDespesas.InserirAsync(despesa, cancelamento);

        return ConversorDeDespesaParaResposta.Converter(despesa, categoria);
    }
}
