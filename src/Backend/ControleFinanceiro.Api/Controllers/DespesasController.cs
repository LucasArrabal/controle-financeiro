using ControleFinanceiro.Aplicacao.CasosDeUso.Despesas.EditarDespesa;
using ControleFinanceiro.Aplicacao.CasosDeUso.Despesas.ExcluirDespesa;
using ControleFinanceiro.Aplicacao.CasosDeUso.Despesas.ListarDespesasDoMes;
using ControleFinanceiro.Aplicacao.CasosDeUso.Despesas.RegistrarDespesa;
using ControleFinanceiro.Aplicacao.Dtos.Requisicoes;
using ControleFinanceiro.Aplicacao.Dtos.Respostas;
using Microsoft.AspNetCore.Mvc;

namespace ControleFinanceiro.Api.Controllers;

[ApiController]
[Route("api/v1/despesas")]
[Produces("application/json")]
public sealed class DespesasController : ControllerBase
{
    /// <summary>Despesas da competência informada, no formato <c>aaaa-MM</c> (ex.: <c>2026-09</c>).</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<RespostaDeDespesa>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<IReadOnlyList<RespostaDeDespesa>>> ListarDoMes(
        [FromServices] ListarDespesasDoMesManipulador manipulador,
        [FromQuery] string competencia,
        CancellationToken cancelamento) =>
        Ok(await manipulador.ManipularAsync(new ListarDespesasDoMesConsulta(competencia), cancelamento));

    [HttpPost]
    [ProducesResponseType<RespostaDeDespesa>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<RespostaDeDespesa>> Registrar(
        [FromServices] RegistrarDespesaManipulador manipulador,
        [FromBody] RequisicaoDeDespesa requisicao,
        CancellationToken cancelamento)
    {
        var comando = new RegistrarDespesaComando(
            requisicao.Descricao,
            requisicao.Valor,
            requisicao.DataDoGasto,
            requisicao.CategoriaId,
            requisicao.FormaDePagamento,
            requisicao.Observacao);

        var despesa = await manipulador.ManipularAsync(comando, cancelamento);

        return CreatedAtAction(
            nameof(ListarDoMes),
            new { competencia = $"{despesa.DataDoGasto:yyyy-MM}" },
            despesa);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<RespostaDeDespesa>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<RespostaDeDespesa>> Editar(
        [FromServices] EditarDespesaManipulador manipulador,
        Guid id,
        [FromBody] RequisicaoDeDespesa requisicao,
        CancellationToken cancelamento)
    {
        var comando = new EditarDespesaComando(
            id,
            requisicao.Descricao,
            requisicao.Valor,
            requisicao.DataDoGasto,
            requisicao.CategoriaId,
            requisicao.FormaDePagamento,
            requisicao.Observacao);

        return Ok(await manipulador.ManipularAsync(comando, cancelamento));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Excluir(
        [FromServices] ExcluirDespesaManipulador manipulador,
        Guid id,
        CancellationToken cancelamento)
    {
        await manipulador.ManipularAsync(new ExcluirDespesaComando(id), cancelamento);

        return NoContent();
    }
}
