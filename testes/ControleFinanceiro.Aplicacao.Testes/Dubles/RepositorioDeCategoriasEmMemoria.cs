using ControleFinanceiro.Aplicacao.Contratos.Repositorios;
using ControleFinanceiro.Dominio.Entidades;
using ControleFinanceiro.Dominio.Enumeradores;

namespace ControleFinanceiro.Aplicacao.Testes.Dubles;

/// <summary>
/// Repositório de categorias guardado numa lista. Escrito à mão em vez de gerado por
/// biblioteca de dublês para os testes lerem como cenário, não como configuração de mock.
/// </summary>
internal sealed class RepositorioDeCategoriasEmMemoria : IRepositorioDeCategorias
{
    private readonly List<CategoriaDeGasto> _categorias = [];

    public int QuantidadeDeInsercoes { get; private set; }

    public int QuantidadeDeAtualizacoes { get; private set; }

    public CategoriaDeGasto Adicionar(
        string nome = "Mercado",
        TipoDeCategoria tipo = TipoDeCategoria.Mercado,
        string cor = "#2E7D32",
        bool ativa = true)
    {
        var categoria = CategoriaDeGasto.Criar(ProvedorDoUsuarioDeTeste.Id, nome, tipo, cor);

        if (!ativa)
        {
            categoria.Desativar();
        }

        _categorias.Add(categoria);
        return categoria;
    }

    public Task<IReadOnlyList<CategoriaDeGasto>> ListarAsync(
        Guid usuarioId,
        bool apenasAtivas,
        CancellationToken cancelamento) =>
        Task.FromResult<IReadOnlyList<CategoriaDeGasto>>(
            _categorias
                .Where(categoria => categoria.UsuarioId == usuarioId)
                .Where(categoria => !apenasAtivas || categoria.Ativa)
                .OrderBy(categoria => categoria.Nome)
                .ToList());

    public Task<CategoriaDeGasto?> ObterPorIdAsync(
        Guid usuarioId,
        Guid categoriaId,
        CancellationToken cancelamento) =>
        Task.FromResult(
            _categorias.SingleOrDefault(
                categoria => categoria.UsuarioId == usuarioId && categoria.Id == categoriaId));

    public Task<bool> ExisteComMesmoNomeAsync(
        Guid usuarioId,
        string nome,
        Guid? idIgnorado,
        CancellationToken cancelamento) =>
        Task.FromResult(
            _categorias.Any(categoria =>
                categoria.UsuarioId == usuarioId
                && categoria.Id != idIgnorado
                && string.Equals(categoria.Nome, nome, StringComparison.OrdinalIgnoreCase)));

    public Task InserirAsync(CategoriaDeGasto categoria, CancellationToken cancelamento)
    {
        _categorias.Add(categoria);
        QuantidadeDeInsercoes++;
        return Task.CompletedTask;
    }

    public Task AtualizarAsync(CategoriaDeGasto categoria, CancellationToken cancelamento)
    {
        QuantidadeDeAtualizacoes++;
        return Task.CompletedTask;
    }
}
