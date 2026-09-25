using ControleFinanceiro.Aplicacao.Contratos;

namespace ControleFinanceiro.Infraestrutura.Dados.Identidade;

/// <summary>
/// Dono fixo de todos os lançamentos enquanto não existe login. As tabelas já têm
/// <c>usuario_id</c>, então ligar autenticação depois é trocar esta implementação por uma
/// que leia o usuário do token — nenhum SQL e nenhum caso de uso muda.
/// </summary>
public sealed class ProvedorDoUsuarioPadrao : IProvedorDoUsuarioAtual
{
    /// <summary>Mesmo GUID usado nos scripts de migração — não altere sem migrar os dados.</summary>
    public static readonly Guid IdDoUsuarioPadrao =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    public Guid UsuarioId => IdDoUsuarioPadrao;
}
