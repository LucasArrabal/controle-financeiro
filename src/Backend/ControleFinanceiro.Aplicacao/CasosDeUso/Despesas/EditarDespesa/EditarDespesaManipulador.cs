using ControleFinanceiro.Aplicacao.Contratos;
using ControleFinanceiro.Aplicacao.Contratos.Repositorios;
using ControleFinanceiro.Aplicacao.Dtos.Respostas;
using ControleFinanceiro.Aplicacao.Excecoes;
using ControleFinanceiro.Dominio.Excecoes;
using FluentValidation;

namespace ControleFinanceiro.Aplicacao.CasosDeUso.Despesas.EditarDespesa;

public sealed class EditarDespesaManipulador
{
    private readonly IRepositorioDeDespesas _repositorioDeDespesas;
    private readonly IRepositorioDeCategorias _repositorioDeCategorias;
    private readonly IProvedorDoUsuarioAtual _provedorDoUsuarioAtual;
    private readonly IProvedorDeDataHora _provedorDeDataHora;
    private readonly IValidator<EditarDespesaComando> _validador;

    public EditarDespesaManipulador(
        IRepositorioDeDespesas repositorioDeDespesas,
        IRepositorioDeCategorias repositorioDeCategorias,
        IProvedorDoUsuarioAtual provedorDoUsuarioAtual,
        IProvedorDeDataHora provedorDeDataHora,
        IValidator<EditarDespesaComando> validador)
    {
        _repositorioDeDespesas = repositorioDeDespesas;
        _repositorioDeCategorias = repositorioDeCategorias;
        _provedorDoUsuarioAtual = provedorDoUsuarioAtual;
        _provedorDeDataHora = provedorDeDataHora;
        _validador = validador;
    }

    public async Task<RespostaDeDespesa> ManipularAsync(
        EditarDespesaComando comando,
        CancellationToken cancelamento)
    {
        await _validador.ValidateAndThrowAsync(comando, cancelamento);

        var usuarioId = _provedorDoUsuarioAtual.UsuarioId;

        var despesa = await _repositorioDeDespesas.ObterPorIdAsync(usuarioId, comando.Id, cancelamento)
                      ?? throw new RecursoNaoEncontradoException("Despesa", comando.Id);

        var categoria = await _repositorioDeCategorias.ObterPorIdAsync(usuarioId, comando.CategoriaId, cancelamento)
                        ?? throw new RecursoNaoEncontradoException("Categoria", comando.CategoriaId);

        // Trocar para uma categoria desativada é bloqueado; manter a que já estava, não.
        RegraDeNegocioVioladaException.LancarSe(
            !categoria.Ativa && categoria.Id != despesa.CategoriaId,
            $"A categoria '{categoria.Nome}' está desativada e não aceita novos lançamentos.");

        despesa.Alterar(
            comando.Descricao,
            comando.Valor,
            comando.DataDoGasto,
            comando.CategoriaId,
            comando.FormaDePagamento,
            comando.Observacao,
            _provedorDeDataHora.HojeNoFusoDoUsuario);

        await _repositorioDeDespesas.AtualizarAsync(despesa, cancelamento);

        return ConversorDeDespesaParaResposta.Converter(despesa, categoria);
    }
}
