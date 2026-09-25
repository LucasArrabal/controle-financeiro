using ControleFinanceiro.Aplicacao.CasosDeUso.PainelResumoMensal;
using ControleFinanceiro.Aplicacao.Dtos.Respostas;
using Microsoft.AspNetCore.Mvc;

namespace ControleFinanceiro.Api.Controllers;

[ApiController]
[Route("api/v1/painel")]
[Produces("application/json")]
public sealed class PainelResumoMensalController : ControllerBase
{
    /// <summary>
    /// Totais do mês e distribuição dos gastos por categoria, com o consumo de cada teto.
    /// Tudo vem de uma única consulta agregada no banco.
    /// </summary>
    [HttpGet("resumo-mensal")]
    [ProducesResponseType<RespostaDoResumoMensal>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<RespostaDoResumoMensal>> ObterResumoMensal(
        [FromServices] ObterResumoMensalManipulador manipulador,
        [FromQuery] string competencia,
        CancellationToken cancelamento) =>
        Ok(await manipulador.ManipularAsync(new ObterResumoMensalConsulta(competencia), cancelamento));
}
