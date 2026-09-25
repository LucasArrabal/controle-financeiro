using System.Text.Json;
using ControleFinanceiro.Aplicacao.Excecoes;
using ControleFinanceiro.Dominio.Excecoes;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace ControleFinanceiro.Api.Middlewares;

/// <summary>
/// Converte toda exceção que escapa dos casos de uso em ProblemDetails (RFC 7807).
/// Detalhe técnico só aparece fora de produção; o cliente recebe sempre o mesmo formato.
/// </summary>
public sealed class TratamentoGlobalDeErrosMiddleware
{
    private const string TipoDeConteudoProblemDetails = "application/problem+json";

    private readonly RequestDelegate _proximo;
    private readonly ILogger<TratamentoGlobalDeErrosMiddleware> _registrador;
    private readonly IHostEnvironment _ambiente;

    public TratamentoGlobalDeErrosMiddleware(
        RequestDelegate proximo,
        ILogger<TratamentoGlobalDeErrosMiddleware> registrador,
        IHostEnvironment ambiente)
    {
        _proximo = proximo;
        _registrador = registrador;
        _ambiente = ambiente;
    }

    public async Task InvokeAsync(HttpContext contexto)
    {
        try
        {
            await _proximo(contexto);
        }
        catch (Exception excecao)
        {
            await ResponderComProblemDetailsAsync(contexto, excecao);
        }
    }

    private async Task ResponderComProblemDetailsAsync(HttpContext contexto, Exception excecao)
    {
        if (contexto.Response.HasStarted)
        {
            _registrador.LogError(excecao, "Erro depois de a resposta já ter começado a ser enviada.");
            return;
        }

        var problema = Traduzir(excecao);

        problema.Instance = contexto.Request.Path;
        problema.Extensions["traceId"] = contexto.TraceIdentifier;

        if (problema.Status == StatusCodes.Status500InternalServerError)
        {
            _registrador.LogError(excecao, "Erro não tratado em {Caminho}.", contexto.Request.Path);

            if (!_ambiente.IsProduction())
            {
                problema.Detail = excecao.ToString();
            }
        }
        else
        {
            _registrador.LogWarning(
                "Requisição rejeitada em {Caminho}: {Mensagem}",
                contexto.Request.Path,
                excecao.Message);
        }

        contexto.Response.Clear();
        contexto.Response.StatusCode = problema.Status!.Value;
        contexto.Response.ContentType = TipoDeConteudoProblemDetails;

        // Serializa pelo tipo concreto: ValidationProblemDetails só leva o "errors" junto assim.
        await contexto.Response.WriteAsync(
            JsonSerializer.Serialize(problema, problema.GetType(), OpcoesDeSerializacao));
    }

    private static ProblemDetails Traduzir(Exception excecao) => excecao switch
    {
        ValidationException erroDeValidacao => MontarProblemaDeValidacao(erroDeValidacao),

        RegraDeNegocioVioladaException regraViolada => new ProblemDetails
        {
            Status = StatusCodes.Status422UnprocessableEntity,
            Title = "Regra de negócio violada.",
            Detail = regraViolada.Message,
            Type = "https://tools.ietf.org/html/rfc4918#section-11.2"
        },

        RecursoNaoEncontradoException naoEncontrado => new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Title = "Recurso não encontrado.",
            Detail = naoEncontrado.Message,
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.5"
        },

        OperationCanceledException => new ProblemDetails
        {
            Status = StatusCodes.Status499ClientClosedRequest,
            Title = "Requisição cancelada pelo cliente."
        },

        _ => new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Erro inesperado ao processar a requisição.",
            Type = "https://tools.ietf.org/html/rfc9110#section-15.6.1"
        }
    };

    private static ProblemDetails MontarProblemaDeValidacao(ValidationException erroDeValidacao)
    {
        // As chaves saem em camelCase para bater com os nomes dos campos no corpo da requisição.
        var errosPorCampo = erroDeValidacao.Errors
            .GroupBy(erro => EmCamelCase(erro.PropertyName))
            .ToDictionary(
                grupo => grupo.Key,
                grupo => grupo.Select(erro => erro.ErrorMessage).ToArray());

        return new ValidationProblemDetails(errosPorCampo)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Um ou mais campos são inválidos.",
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1"
        };
    }

    private static string EmCamelCase(string nomeDaPropriedade) =>
        string.IsNullOrEmpty(nomeDaPropriedade)
            ? nomeDaPropriedade
            : char.ToLowerInvariant(nomeDaPropriedade[0]) + nomeDaPropriedade[1..];

    private static readonly JsonSerializerOptions OpcoesDeSerializacao = new(JsonSerializerDefaults.Web);
}
