using ControleFinanceiro.Dominio.ObjetosDeValor;

namespace ControleFinanceiro.Aplicacao.Contratos.Repositorios;

/// <summary>
/// Leitura do painel. É uma consulta, não um repositório: não devolve entidades, devolve
/// os totais do mês já agregados pelo banco numa única ida.
/// </summary>
public interface IConsultaDeResumoMensal
{
    Task<ResumoMensalAgregado> ObterAsync(
        Guid usuarioId,
        CompetenciaMensal competencia,
        CancellationToken cancelamento);
}
