namespace ControleFinanceiro.Aplicacao.Excecoes;

/// <summary>
/// Lançada quando um id informado pelo cliente não existe para o usuário atual.
/// A API traduz esta exceção para HTTP 404.
/// </summary>
public sealed class RecursoNaoEncontradoException : Exception
{
    public RecursoNaoEncontradoException(string recurso, Guid id)
        : base($"{recurso} {id} não foi encontrado.")
    {
    }

    public RecursoNaoEncontradoException(string mensagem) : base(mensagem)
    {
    }
}
