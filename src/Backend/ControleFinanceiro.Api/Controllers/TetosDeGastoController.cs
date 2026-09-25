using ControleFinanceiro.Aplicacao.CasosDeUso.TetosDeGasto.CopiarTetosDoMesAnterior;
using ControleFinanceiro.Aplicacao.CasosDeUso.TetosDeGasto.DefinirTetosDoMes;
using ControleFinanceiro.Aplicacao.CasosDeUso.TetosDeGasto.ListarTetosDoMes;
using ControleFinanceiro.Aplicacao.Dtos.Requisicoes;
using ControleFinanceiro.Aplicacao.Dtos.Respostas;
using Microsoft.AspNetCore.Mvc;

namespace ControleFinanceiro.Api.Controllers;

[ApiController]
[Route("api/v1/tetos-de-gasto")]
[Produces("application/json")]
public sealed class TetosDeGastoController : ControllerBase
{
    /// <summary>
    /// Uma linha por categoria ativa. <c>valorLimite</c> nulo é "sem teto"; zero é
    /// "nenhum gasto permitido".
    /// </summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<RespostaDeTetoDeGasto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<IReadOnlyList<RespostaDeTetoDeGasto>>> ListarDoMes(
        [FromServices] ListarTetosDoMesManipulador manipulador,
        [FromQuery] string competencia,
        CancellationToken cancelamento) =>
        Ok(await manipulador.ManipularAsync(new ListarTetosDoMesConsulta(competencia), cancelamento));

    /// <summary>Upsert em lote: grava de uma vez o estado da tela de tetos do mês.</summary>
    [HttpPut]
    [ProducesResponseType<IReadOnlyList<RespostaDeTetoDeGasto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<IReadOnlyList<RespostaDeTetoDeGasto>>> Definir(
        [FromServices] DefinirTetosDoMesManipulador manipulador,
        [FromBody] RequisicaoDeTetosDoMes requisicao,
        CancellationToken cancelamento)
    {
        var comando = new DefinirTetosDoMesComando(
            requisicao.Competencia,
            requisicao.Tetos
                .Select(teto => new LimiteDeCategoria(teto.CategoriaId, teto.ValorLimite))
                .ToList());

        return Ok(await manipulador.ManipularAsync(comando, cancelamento));
    }

    [HttpPost("copiar-do-mes-anterior")]
    [ProducesResponseType<IReadOnlyList<RespostaDeTetoDeGasto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<IReadOnlyList<RespostaDeTetoDeGasto>>> CopiarDoMesAnterior(
        [FromServices] CopiarTetosDoMesAnteriorManipulador manipulador,
        [FromQuery] string competencia,
        CancellationToken cancelamento) =>
        Ok(await manipulador.ManipularAsync(
            new CopiarTetosDoMesAnteriorComando(competencia),
            cancelamento));
}
