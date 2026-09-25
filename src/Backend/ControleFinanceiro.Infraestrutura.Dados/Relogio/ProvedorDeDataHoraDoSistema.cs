using ControleFinanceiro.Aplicacao.Contratos;

namespace ControleFinanceiro.Infraestrutura.Dados.Relogio;

/// <summary>
/// Relógio real. Grava sempre em UTC e só converte para America/Sao_Paulo para responder
/// "que dia é hoje" — a pergunta que decide se um gasto está longe demais no futuro.
/// </summary>
public sealed class ProvedorDeDataHoraDoSistema : IProvedorDeDataHora
{
    private const string FusoIana = "America/Sao_Paulo";
    private const string FusoWindows = "E. South America Standard Time";

    private readonly TimeZoneInfo _fusoDoUsuario = ResolverFusoDoUsuario();

    public DateTimeOffset AgoraEmUtc => DateTimeOffset.UtcNow;

    public DateOnly HojeNoFusoDoUsuario =>
        DateOnly.FromDateTime(
            TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, _fusoDoUsuario).DateTime);

    private static TimeZoneInfo ResolverFusoDoUsuario()
    {
        // .NET no Windows entende o id IANA quando o ICU está disponível; o fallback cobre
        // o modo de globalização invariante, em que só o id do Windows é reconhecido.
        foreach (var identificador in new[] { FusoIana, FusoWindows })
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(identificador);
            }
            catch (TimeZoneNotFoundException)
            {
                // Tenta o próximo identificador.
            }
            catch (InvalidTimeZoneException)
            {
                // Tenta o próximo identificador.
            }
        }

        return TimeZoneInfo.Utc;
    }
}
