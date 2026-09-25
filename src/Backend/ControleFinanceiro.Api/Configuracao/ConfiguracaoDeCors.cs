namespace ControleFinanceiro.Api.Configuracao;

/// <summary>
/// CORS liberado apenas para as origens configuradas. Nada de AllowAnyOrigin:
/// a lista vem de <c>CONTROLEFINANCEIRO_ORIGENS_PERMITIDAS</c>, separada por vírgula.
/// </summary>
public static class ConfiguracaoDeCors
{
    public const string NomeDaPolitica = "OrigensDoFrontend";

    public const string ChaveDeConfiguracao = "CONTROLEFINANCEIRO_ORIGENS_PERMITIDAS";

    private const string OrigemPadraoDeDesenvolvimento = "http://localhost:4200";

    public static IServiceCollection AdicionarCorsDoFrontend(
        this IServiceCollection servicos,
        IConfiguration configuracao)
    {
        var origensPermitidas = LerOrigensPermitidas(configuracao);

        servicos.AddCors(opcoes => opcoes.AddPolicy(
            NomeDaPolitica,
            politica => politica
                .WithOrigins(origensPermitidas)
                .AllowAnyHeader()
                .AllowAnyMethod()));

        return servicos;
    }

    public static string[] LerOrigensPermitidas(IConfiguration configuracao)
    {
        var configurado = configuracao[ChaveDeConfiguracao];

        if (string.IsNullOrWhiteSpace(configurado))
        {
            return [OrigemPadraoDeDesenvolvimento];
        }

        return configurado
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToArray();
    }
}
