using ControleFinanceiro.Aplicacao.Contratos.Repositorios;
using ControleFinanceiro.Dominio.Entidades;
using ControleFinanceiro.Dominio.ObjetosDeValor;

namespace ControleFinanceiro.Aplicacao.Testes.Dubles;

internal sealed class RepositorioDeTetosDeGastoEmMemoria : IRepositorioDeTetosDeGasto
{
    private readonly List<TetoDeGastoMensal> _tetos = [];

    public IReadOnlyList<TetoDeGastoMensal> Gravados => _tetos;

    public void Adicionar(Guid categoriaId, CompetenciaMensal competencia, decimal valorLimite) =>
        _tetos.Add(TetoDeGastoMensal.Definir(
            ProvedorDoUsuarioDeTeste.Id,
            categoriaId,
            competencia,
            valorLimite));

    public Task<IReadOnlyList<TetoDeGastoMensal>> ListarPorCompetenciaAsync(
        Guid usuarioId,
        CompetenciaMensal competencia,
        CancellationToken cancelamento) =>
        Task.FromResult<IReadOnlyList<TetoDeGastoMensal>>(
            _tetos
                .Where(teto => teto.UsuarioId == usuarioId)
                .Where(teto => teto.Competencia.Equals(competencia))
                .ToList());

    public Task SalvarEmLoteAsync(
        Guid usuarioId,
        CompetenciaMensal competencia,
        IReadOnlyCollection<TetoDeGastoMensal> tetos,
        IReadOnlyCollection<Guid> categoriasSemTeto,
        CancellationToken cancelamento)
    {
        _tetos.RemoveAll(teto =>
            teto.UsuarioId == usuarioId
            && teto.Competencia.Equals(competencia)
            && categoriasSemTeto.Contains(teto.CategoriaId));

        foreach (var teto in tetos)
        {
            // Mesmo efeito do `on conflict do update` do banco.
            _tetos.RemoveAll(existente =>
                existente.CategoriaId == teto.CategoriaId
                && existente.Competencia.Equals(teto.Competencia));

            _tetos.Add(teto);
        }

        return Task.CompletedTask;
    }
}
