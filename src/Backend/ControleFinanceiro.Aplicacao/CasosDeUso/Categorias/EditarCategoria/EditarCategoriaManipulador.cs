using ControleFinanceiro.Aplicacao.Contratos;
using ControleFinanceiro.Aplicacao.Contratos.Repositorios;
using ControleFinanceiro.Aplicacao.Dtos.Respostas;
using ControleFinanceiro.Aplicacao.Excecoes;
using ControleFinanceiro.Dominio.Enumeradores;
using ControleFinanceiro.Dominio.Excecoes;
using FluentValidation;

namespace ControleFinanceiro.Aplicacao.CasosDeUso.Categorias.EditarCategoria;

public sealed class EditarCategoriaManipulador
{
    private readonly IRepositorioDeCategorias _repositorioDeCategorias;
    private readonly IProvedorDoUsuarioAtual _provedorDoUsuarioAtual;
    private readonly IValidator<EditarCategoriaComando> _validador;

    public EditarCategoriaManipulador(
        IRepositorioDeCategorias repositorioDeCategorias,
        IProvedorDoUsuarioAtual provedorDoUsuarioAtual,
        IValidator<EditarCategoriaComando> validador)
    {
        _repositorioDeCategorias = repositorioDeCategorias;
        _provedorDoUsuarioAtual = provedorDoUsuarioAtual;
        _validador = validador;
    }

    public async Task<RespostaDeCategoria> ManipularAsync(
        EditarCategoriaComando comando,
        CancellationToken cancelamento)
    {
        await _validador.ValidateAndThrowAsync(comando, cancelamento);

        var usuarioId = _provedorDoUsuarioAtual.UsuarioId;

        var categoria = await _repositorioDeCategorias.ObterPorIdAsync(usuarioId, comando.Id, cancelamento)
                        ?? throw new RecursoNaoEncontradoException("Categoria", comando.Id);

        var nomeJaUsado = await _repositorioDeCategorias.ExisteComMesmoNomeAsync(
            usuarioId,
            comando.Nome.Trim(),
            idIgnorado: comando.Id,
            cancelamento);

        RegraDeNegocioVioladaException.LancarSe(
            nomeJaUsado,
            $"Já existe uma categoria chamada '{comando.Nome.Trim()}'.");

        categoria.Renomear(comando.Nome);
        categoria.AlterarTipo(Enum.Parse<TipoDeCategoria>(comando.Tipo, ignoreCase: true));
        categoria.AlterarCor(comando.CorHexadecimal);

        if (comando.Ativa)
        {
            categoria.Reativar();
        }
        else
        {
            categoria.Desativar();
        }

        await _repositorioDeCategorias.AtualizarAsync(categoria, cancelamento);

        return ConversorDeCategoriaParaResposta.Converter(categoria);
    }
}
