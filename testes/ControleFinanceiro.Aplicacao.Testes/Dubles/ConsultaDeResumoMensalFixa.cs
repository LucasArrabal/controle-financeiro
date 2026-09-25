using ControleFinanceiro.Aplicacao.Contratos.Repositorios;
using ControleFinanceiro.Dominio.ObjetosDeValor;

namespace ControleFinanceiro.Aplicacao.Testes.Dubles;

/// <summary>Devolve sempre o mesmo agregado, para testar só a camada de cálculo do painel.</summary>
internal sealed class ConsultaDeResumoMensalFixa : IConsultaDeResumoMensal
{
    private readonly ResumoMensalAgregado _agregado;

    public ConsultaDeResumoMensalFixa(ResumoMensalAgregado agregado) => _agregado = agregado;

    public Task<ResumoMensalAgregado> ObterAsync(
        Guid usuarioId,
        CompetenciaMensal competencia,
        CancellationToken cancelamento) =>
        Task.FromResult(_agregado);
}
