namespace ControleFinanceiro.Aplicacao.Contratos;

/// <summary>
/// Fonte de tempo dos casos de uso. Existe para que as regras que dependem de "hoje"
/// sejam testáveis sem congelar o relógio do sistema.
/// </summary>
public interface IProvedorDeDataHora
{
    /// <summary>Instante atual em UTC — é o que vai para as colunas <c>timestamptz</c>.</summary>
    DateTimeOffset AgoraEmUtc { get; }

    /// <summary>Data de hoje no fuso do usuário (America/Sao_Paulo), sem conversão de fuso depois.</summary>
    DateOnly HojeNoFusoDoUsuario { get; }
}
