using ControleFinanceiro.Aplicacao.Contratos;
using ControleFinanceiro.Aplicacao.Contratos.Repositorios;
using ControleFinanceiro.Aplicacao.Dtos.Respostas;
using ControleFinanceiro.Dominio.Entidades;
using ControleFinanceiro.Dominio.Enumeradores;
using ControleFinanceiro.Dominio.Excecoes;
using FluentValidation;

namespace ControleFinanceiro.Aplicacao.CasosDeUso.Categorias.CriarCategoria;

public sealed class CriarCategoriaManipulador
{
    private readonly IRepositorioDeCategorias _repositorioDeCategorias;
    private readonly IProvedorDoUsuarioAtual _provedorDoUsuarioAtual;
    private readonly IValidator<CriarCategoriaComando> _validador;

    public CriarCategoriaManipulador(
        IRepositorioDeCategorias repositorioDeCategorias,
        IProvedorDoUsuarioAtual provedorDoUsuarioAtual,
        IValidator<CriarCategoriaComando> validador)
    {
        _repositorioDeCategorias = repositorioDeCategorias;
        _provedorDoUsuarioAtual = provedorDoUsuarioAtual;
        _validador = validador;
    }

    public async Task<RespostaDeCategoria> ManipularAsync(
        CriarCategoriaComando comando,
        CancellationToken cancelamento)
    {
        await _validador.ValidateAndThrowAsync(comando, cancelamento);

        var usuarioId = _provedorDoUsuarioAtual.UsuarioId;

        var nomeJaUsado = await _repositorioDeCategorias.ExisteComMesmoNomeAsync(
            usuarioId,
            comando.Nome.Trim(),
            idIgnorado: null,
            cancelamento);

        RegraDeNegocioVioladaException.LancarSe(
            nomeJaUsado,
            $"Já existe uma categoria chamada '{comando.Nome.Trim()}'.");

        var categoria = CategoriaDeGasto.Criar(
            usuarioId,
            comando.Nome,
            Enum.Parse<TipoDeCategoria>(comando.Tipo, ignoreCase: true),
            comando.CorHexadecimal);

        await _repositorioDeCategorias.InserirAsync(categoria, cancelamento);

        return ConversorDeCategoriaParaResposta.Converter(categoria);
    }
}
