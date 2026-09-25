using ControleFinanceiro.Aplicacao.Contratos;
using ControleFinanceiro.Aplicacao.Contratos.Repositorios;
using ControleFinanceiro.Aplicacao.Excecoes;

namespace ControleFinanceiro.Aplicacao.CasosDeUso.Receitas.ExcluirReceita;

public sealed class ExcluirReceitaManipulador
{
    private readonly IRepositorioDeReceitas _repositorioDeReceitas;
    private readonly IProvedorDoUsuarioAtual _provedorDoUsuarioAtual;

    public ExcluirReceitaManipulador(
        IRepositorioDeReceitas repositorioDeReceitas,
        IProvedorDoUsuarioAtual provedorDoUsuarioAtual)
    {
        _repositorioDeReceitas = repositorioDeReceitas;
        _provedorDoUsuarioAtual = provedorDoUsuarioAtual;
    }

    public async Task ManipularAsync(ExcluirReceitaComando comando, CancellationToken cancelamento)
    {
        var usuarioId = _provedorDoUsuarioAtual.UsuarioId;

        var receita = await _repositorioDeReceitas.ObterPorIdAsync(usuarioId, comando.Id, cancelamento)
                      ?? throw new RecursoNaoEncontradoException("Receita", comando.Id);

        await _repositorioDeReceitas.ExcluirAsync(usuarioId, receita.Id, cancelamento);
    }
}
