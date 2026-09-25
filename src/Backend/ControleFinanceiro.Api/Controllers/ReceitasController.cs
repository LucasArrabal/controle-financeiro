using ControleFinanceiro.Aplicacao.CasosDeUso.Receitas.EditarReceita;
using ControleFinanceiro.Aplicacao.CasosDeUso.Receitas.ExcluirReceita;
using ControleFinanceiro.Aplicacao.CasosDeUso.Receitas.ListarReceitasDoMes;
using ControleFinanceiro.Aplicacao.CasosDeUso.Receitas.RegistrarReceita;
using ControleFinanceiro.Aplicacao.CasosDeUso.Receitas.ReplicarReceitaNosMesesSeguintes;
using ControleFinanceiro.Aplicacao.Dtos.Requisicoes;
using ControleFinanceiro.Aplicacao.Dtos.Respostas;
using Microsoft.AspNetCore.Mvc;

namespace ControleFinanceiro.Api.Controllers;

[ApiController]
[Route("api/v1/receitas")]
[Produces("application/json")]
public sealed class ReceitasController : ControllerBase
{
    /// <summary>Receitas da competência informada, no formato <c>aaaa-MM</c> (ex.: <c>2026-09</c>).</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<RespostaDeReceita>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<IReadOnlyList<RespostaDeReceita>>> ListarDoMes(
        [FromServices] ListarReceitasDoMesManipulador manipulador,
        [FromQuery] string competencia,
        CancellationToken cancelamento) =>
        Ok(await manipulador.ManipularAsync(new ListarReceitasDoMesConsulta(competencia), cancelamento));

    [HttpPost]
    [ProducesResponseType<RespostaDeReceita>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<RespostaDeReceita>> Registrar(
        [FromServices] RegistrarReceitaManipulador manipulador,
        [FromBody] RequisicaoDeReceita requisicao,
        CancellationToken cancelamento)
    {
        var comando = new RegistrarReceitaComando(
            requisicao.Descricao,
            requisicao.Valor,
            requisicao.DataDoRecebimento,
            requisicao.Recorrente);

        var receita = await manipulador.ManipularAsync(comando, cancelamento);

        return CreatedAtAction(
            nameof(ListarDoMes),
            new { competencia = $"{receita.DataDoRecebimento:yyyy-MM}" },
            receita);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<RespostaDeReceita>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<RespostaDeReceita>> Editar(
        [FromServices] EditarReceitaManipulador manipulador,
        Guid id,
        [FromBody] RequisicaoDeReceita requisicao,
        CancellationToken cancelamento)
    {
        var comando = new EditarReceitaComando(
            id,
            requisicao.Descricao,
            requisicao.Valor,
            requisicao.DataDoRecebimento,
            requisicao.Recorrente);

        return Ok(await manipulador.ManipularAsync(comando, cancelamento));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Excluir(
        [FromServices] ExcluirReceitaManipulador manipulador,
        Guid id,
        CancellationToken cancelamento)
    {
        await manipulador.ManipularAsync(new ExcluirReceitaComando(id), cancelamento);

        return NoContent();
    }

    /// <summary>
    /// Copia uma receita recorrente para os próximos meses. Meses que já têm um lançamento
    /// de mesma descrição são pulados, e a resposta diz quais foram criados e quais ignorados.
    /// </summary>
    [HttpPost("{id:guid}/replicar")]
    [ProducesResponseType<RespostaDaReplicacaoDeReceita>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<RespostaDaReplicacaoDeReceita>> Replicar(
        [FromServices] ReplicarReceitaNosMesesSeguintesManipulador manipulador,
        Guid id,
        [FromQuery] int meses,
        CancellationToken cancelamento) =>
        Ok(await manipulador.ManipularAsync(
            new ReplicarReceitaNosMesesSeguintesComando(id, meses),
            cancelamento));
}
