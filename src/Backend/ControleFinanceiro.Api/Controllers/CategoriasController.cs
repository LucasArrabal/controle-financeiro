using ControleFinanceiro.Aplicacao.CasosDeUso.Categorias.CriarCategoria;
using ControleFinanceiro.Aplicacao.CasosDeUso.Categorias.DesativarCategoria;
using ControleFinanceiro.Aplicacao.CasosDeUso.Categorias.EditarCategoria;
using ControleFinanceiro.Aplicacao.CasosDeUso.Categorias.ListarCategorias;
using ControleFinanceiro.Aplicacao.Dtos.Requisicoes;
using ControleFinanceiro.Aplicacao.Dtos.Respostas;
using Microsoft.AspNetCore.Mvc;

namespace ControleFinanceiro.Api.Controllers;

[ApiController]
[Route("api/v1/categorias")]
[Produces("application/json")]
public sealed class CategoriasController : ControllerBase
{
    /// <summary>Lista as categorias. Por padrão traz só as ativas.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<RespostaDeCategoria>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<RespostaDeCategoria>>> Listar(
        [FromServices] ListarCategoriasManipulador manipulador,
        [FromQuery] bool incluirInativas,
        CancellationToken cancelamento) =>
        Ok(await manipulador.ManipularAsync(new ListarCategoriasConsulta(incluirInativas), cancelamento));

    [HttpPost]
    [ProducesResponseType<RespostaDeCategoria>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<RespostaDeCategoria>> Criar(
        [FromServices] CriarCategoriaManipulador manipulador,
        [FromBody] RequisicaoDeCategoria requisicao,
        CancellationToken cancelamento)
    {
        var comando = new CriarCategoriaComando(
            requisicao.Nome,
            requisicao.Tipo,
            requisicao.CorHexadecimal);

        var categoria = await manipulador.ManipularAsync(comando, cancelamento);

        return CreatedAtAction(nameof(Listar), new { }, categoria);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<RespostaDeCategoria>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<RespostaDeCategoria>> Editar(
        [FromServices] EditarCategoriaManipulador manipulador,
        Guid id,
        [FromBody] RequisicaoDeEdicaoDeCategoria requisicao,
        CancellationToken cancelamento)
    {
        var comando = new EditarCategoriaComando(
            id,
            requisicao.Nome,
            requisicao.Tipo,
            requisicao.CorHexadecimal,
            requisicao.Ativa);

        return Ok(await manipulador.ManipularAsync(comando, cancelamento));
    }

    /// <summary>Desativa a categoria (soft delete). Os gastos já lançados nela continuam no painel.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Desativar(
        [FromServices] DesativarCategoriaManipulador manipulador,
        Guid id,
        CancellationToken cancelamento)
    {
        await manipulador.ManipularAsync(new DesativarCategoriaComando(id), cancelamento);

        return NoContent();
    }
}
