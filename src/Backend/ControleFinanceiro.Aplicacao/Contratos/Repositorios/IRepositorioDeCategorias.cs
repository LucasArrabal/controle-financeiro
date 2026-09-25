using ControleFinanceiro.Dominio.Entidades;

namespace ControleFinanceiro.Aplicacao.Contratos.Repositorios;

public interface IRepositorioDeCategorias
{
    Task<IReadOnlyList<CategoriaDeGasto>> ListarAsync(
        Guid usuarioId,
        bool apenasAtivas,
        CancellationToken cancelamento);

    Task<CategoriaDeGasto?> ObterPorIdAsync(
        Guid usuarioId,
        Guid categoriaId,
        CancellationToken cancelamento);

    /// <summary>
    /// Verifica a unicidade do nome ignorando maiúsculas/minúsculas.
    /// <paramref name="idIgnorado"/> permite editar a própria categoria sem colidir consigo mesma.
    /// </summary>
    Task<bool> ExisteComMesmoNomeAsync(
        Guid usuarioId,
        string nome,
        Guid? idIgnorado,
        CancellationToken cancelamento);

    Task InserirAsync(CategoriaDeGasto categoria, CancellationToken cancelamento);

    /// <summary>Grava nome, tipo, cor e a situação de ativa — é por aqui que o soft delete acontece.</summary>
    Task AtualizarAsync(CategoriaDeGasto categoria, CancellationToken cancelamento);
}
