using ControleFinanceiro.Aplicacao.Contratos;

namespace ControleFinanceiro.Aplicacao.Testes.Dubles;

internal sealed class ProvedorDoUsuarioDeTeste : IProvedorDoUsuarioAtual
{
    public static readonly Guid Id = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public Guid UsuarioId => Id;
}
