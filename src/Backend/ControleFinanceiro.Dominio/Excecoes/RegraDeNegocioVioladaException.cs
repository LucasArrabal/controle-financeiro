namespace ControleFinanceiro.Dominio.Excecoes;

/// <summary>
/// Lançada quando uma invariante do domínio é violada. A API traduz esta exceção
/// para HTTP 422 em <c>TratamentoGlobalDeErrosMiddleware</c>.
/// </summary>
public sealed class RegraDeNegocioVioladaException : Exception
{
    public RegraDeNegocioVioladaException(string mensagem) : base(mensagem)
    {
    }

    public static void LancarSe(bool condicao, string mensagem)
    {
        if (condicao)
        {
            throw new RegraDeNegocioVioladaException(mensagem);
        }
    }
}
