using ControleFinanceiro.Aplicacao.Contratos.Repositorios;
using ControleFinanceiro.Dominio.Entidades;
using ControleFinanceiro.Dominio.ObjetosDeValor;

namespace ControleFinanceiro.Aplicacao.Testes.Dubles;

internal sealed class RepositorioDeReceitasEmMemoria : IRepositorioDeReceitas
{
    private readonly List<Receita> _receitas = [];

    public IReadOnlyList<Receita> Gravadas => _receitas;

    public Receita Adicionar(
        string descricao = "Salário",
        decimal valor = 7800m,
        DateOnly? dataDoRecebimento = null,
        bool recorrente = true)
    {
        var receita = Receita.Registrar(
            ProvedorDoUsuarioDeTeste.Id,
            descricao,
            valor,
            dataDoRecebimento ?? new DateOnly(2026, 9, 5),
            recorrente,
            new DateTimeOffset(2026, 9, 5, 12, 0, 0, TimeSpan.Zero));

        _receitas.Add(receita);
        return receita;
    }

    public Task<IReadOnlyList<Receita>> ListarPorCompetenciaAsync(
        Guid usuarioId,
        CompetenciaMensal competencia,
        CancellationToken cancelamento) =>
        Task.FromResult<IReadOnlyList<Receita>>(
            _receitas
                .Where(receita => receita.UsuarioId == usuarioId)
                .Where(receita => receita.Competencia.Equals(competencia))
                .ToList());

    public Task<Receita?> ObterPorIdAsync(
        Guid usuarioId,
        Guid receitaId,
        CancellationToken cancelamento) =>
        Task.FromResult(
            _receitas.SingleOrDefault(
                receita => receita.UsuarioId == usuarioId && receita.Id == receitaId));

    public Task InserirAsync(Receita receita, CancellationToken cancelamento)
    {
        _receitas.Add(receita);
        return Task.CompletedTask;
    }

    public Task InserirVariasAsync(
        IReadOnlyCollection<Receita> receitas,
        CancellationToken cancelamento)
    {
        _receitas.AddRange(receitas);
        return Task.CompletedTask;
    }

    public Task AtualizarAsync(Receita receita, CancellationToken cancelamento) =>
        Task.CompletedTask;

    public Task ExcluirAsync(Guid usuarioId, Guid receitaId, CancellationToken cancelamento)
    {
        _receitas.RemoveAll(receita => receita.UsuarioId == usuarioId && receita.Id == receitaId);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<CompetenciaMensal>> ListarCompetenciasComDescricaoAsync(
        Guid usuarioId,
        string descricao,
        IReadOnlyCollection<CompetenciaMensal> competencias,
        CancellationToken cancelamento) =>
        Task.FromResult<IReadOnlyList<CompetenciaMensal>>(
            _receitas
                .Where(receita => receita.UsuarioId == usuarioId)
                .Where(receita => string.Equals(
                    receita.Descricao,
                    descricao,
                    StringComparison.OrdinalIgnoreCase))
                .Select(receita => receita.Competencia)
                .Where(competencias.Contains)
                .Distinct()
                .ToList());
}
