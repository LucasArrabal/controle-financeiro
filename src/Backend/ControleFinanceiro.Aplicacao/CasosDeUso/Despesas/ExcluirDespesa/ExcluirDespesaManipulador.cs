using ControleFinanceiro.Aplicacao.Contratos;
using ControleFinanceiro.Aplicacao.Contratos.Repositorios;
using ControleFinanceiro.Aplicacao.Excecoes;

namespace ControleFinanceiro.Aplicacao.CasosDeUso.Despesas.ExcluirDespesa;

public sealed class ExcluirDespesaManipulador
{
    private readonly IRepositorioDeDespesas _repositorioDeDespesas;
    private readonly IProvedorDoUsuarioAtual _provedorDoUsuarioAtual;

    public ExcluirDespesaManipulador(
        IRepositorioDeDespesas repositorioDeDespesas,
        IProvedorDoUsuarioAtual provedorDoUsuarioAtual)
    {
        _repositorioDeDespesas = repositorioDeDespesas;
        _provedorDoUsuarioAtual = provedorDoUsuarioAtual;
    }

    public async Task ManipularAsync(ExcluirDespesaComando comando, CancellationToken cancelamento)
    {
        var usuarioId = _provedorDoUsuarioAtual.UsuarioId;

        var despesa = await _repositorioDeDespesas.ObterPorIdAsync(usuarioId, comando.Id, cancelamento)
                      ?? throw new RecursoNaoEncontradoException("Despesa", comando.Id);

        await _repositorioDeDespesas.ExcluirAsync(usuarioId, despesa.Id, cancelamento);
    }
}
