using ControleFinanceiro.Aplicacao.Contratos;

namespace ControleFinanceiro.Aplicacao.Testes.Dubles;

/// <summary>Relógio parado, para as regras que dependem de "hoje" não mudarem de resultado amanhã.</summary>
internal sealed class ProvedorDeDataHoraFixo : IProvedorDeDataHora
{
    public static readonly DateOnly HojeDeReferencia = new(2026, 9, 21);

    public DateTimeOffset AgoraEmUtc { get; init; } =
        new(2026, 9, 21, 15, 0, 0, TimeSpan.Zero);

    public DateOnly HojeNoFusoDoUsuario { get; init; } = HojeDeReferencia;
}
