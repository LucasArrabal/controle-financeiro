using ControleFinanceiro.Aplicacao.Contratos;
using ControleFinanceiro.Aplicacao.Contratos.Repositorios;
using ControleFinanceiro.Aplicacao.Dtos.Respostas;
using ControleFinanceiro.Aplicacao.Excecoes;
using FluentValidation;

namespace ControleFinanceiro.Aplicacao.CasosDeUso.Receitas.EditarReceita;

public sealed class EditarReceitaManipulador
{
    private readonly IRepositorioDeReceitas _repositorioDeReceitas;
    private readonly IProvedorDoUsuarioAtual _provedorDoUsuarioAtual;
    private readonly IValidator<EditarReceitaComando> _validador;

    public EditarReceitaManipulador(
        IRepositorioDeReceitas repositorioDeReceitas,
        IProvedorDoUsuarioAtual provedorDoUsuarioAtual,
        IValidator<EditarReceitaComando> validador)
    {
        _repositorioDeReceitas = repositorioDeReceitas;
        _provedorDoUsuarioAtual = provedorDoUsuarioAtual;
        _validador = validador;
    }

    public async Task<RespostaDeReceita> ManipularAsync(
        EditarReceitaComando comando,
        CancellationToken cancelamento)
    {
        await _validador.ValidateAndThrowAsync(comando, cancelamento);

        var usuarioId = _provedorDoUsuarioAtual.UsuarioId;

        var receita = await _repositorioDeReceitas.ObterPorIdAsync(usuarioId, comando.Id, cancelamento)
                      ?? throw new RecursoNaoEncontradoException("Receita", comando.Id);

        receita.Alterar(
            comando.Descricao,
            comando.Valor,
            comando.DataDoRecebimento,
            comando.Recorrente);

        await _repositorioDeReceitas.AtualizarAsync(receita, cancelamento);

        return ConversorDeReceitaParaResposta.Converter(receita);
    }
}
