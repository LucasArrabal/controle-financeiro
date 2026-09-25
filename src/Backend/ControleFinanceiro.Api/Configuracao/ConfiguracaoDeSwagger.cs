namespace ControleFinanceiro.Api.Configuracao;

/// <summary>
/// Documento OpenAPI gerado pelo <c>Microsoft.AspNetCore.OpenApi</c> e servido na interface
/// do Swagger UI. Só é exposto fora de produção.
/// </summary>
public static class ConfiguracaoDeSwagger
{
    public const string NomeDoDocumento = "v1";

    private const string CaminhoDoDocumento = "/openapi/v1.json";

    public static IServiceCollection AdicionarDocumentacaoOpenApi(this IServiceCollection servicos)
    {
        servicos.AddOpenApi(NomeDoDocumento);
        return servicos;
    }

    public static WebApplication UsarSwaggerEmDesenvolvimento(this WebApplication aplicacao)
    {
        if (aplicacao.Environment.IsProduction())
        {
            return aplicacao;
        }

        aplicacao.MapOpenApi();

        aplicacao.UseSwaggerUI(opcoes =>
        {
            opcoes.SwaggerEndpoint(CaminhoDoDocumento, "Controle Financeiro — v1");
            opcoes.RoutePrefix = "swagger";
            opcoes.DocumentTitle = "Controle Financeiro — API";
        });

        return aplicacao;
    }
}
