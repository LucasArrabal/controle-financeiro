using ControleFinanceiro.Aplicacao.Contratos;
using ControleFinanceiro.Aplicacao.Contratos.Repositorios;
using ControleFinanceiro.Aplicacao.Excecoes;

namespace ControleFinanceiro.Aplicacao.CasosDeUso.Categorias.DesativarCategoria;

public sealed class DesativarCategoriaManipulador
{
    private readonly IRepositorioDeCategorias _repositorioDeCategorias;
    private readonly IProvedorDoUsuarioAtual _provedorDoUsuarioAtual;

    public DesativarCategoriaManipulador(
        IRepositorioDeCategorias repositorioDeCategorias,
        IProvedorDoUsuarioAtual provedorDoUsuarioAtual)
    {
        _repositorioDeCategorias = repositorioDeCategorias;
        _provedorDoUsuarioAtual = provedorDoUsuarioAtual;
    }

    public async Task ManipularAsync(DesativarCategoriaComando comando, CancellationToken cancelamento)
    {
        var usuarioId = _provedorDoUsuarioAtual.UsuarioId;

        var categoria = await _repositorioDeCategorias.ObterPorIdAsync(usuarioId, comando.Id, cancelamento)
                        ?? throw new RecursoNaoEncontradoException("Categoria", comando.Id);

        if (!categoria.Ativa)
        {
            return;
        }

        categoria.Desativar();

        await _repositorioDeCategorias.AtualizarAsync(categoria, cancelamento);
    }
}
