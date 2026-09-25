namespace ControleFinanceiro.Aplicacao.Contratos;

/// <summary>
/// Dono dos lançamentos que estão sendo lidos ou gravados. Enquanto não existe login,
/// a implementação devolve um GUID fixo — quando a autenticação entrar, só essa
/// implementação muda, nenhum caso de uso precisa ser tocado.
/// </summary>
public interface IProvedorDoUsuarioAtual
{
    Guid UsuarioId { get; }
}
