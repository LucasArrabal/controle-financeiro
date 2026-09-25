using ControleFinanceiro.Aplicacao.Contratos;
using ControleFinanceiro.Aplicacao.Contratos.Repositorios;
using ControleFinanceiro.Aplicacao.Dtos.Respostas;
using ControleFinanceiro.Dominio.ObjetosDeValor;
using ControleFinanceiro.Dominio.RegrasDeNegocio;

namespace ControleFinanceiro.Aplicacao.CasosDeUso.PainelResumoMensal;

/// <summary>
/// Monta o painel: o banco entrega os totais somados e este caso de uso aplica as regras de
/// percentual e de situação do teto, que são do domínio.
/// </summary>
public sealed class ObterResumoMensalManipulador
{
    private readonly IConsultaDeResumoMensal _consultaDeResumoMensal;
    private readonly IProvedorDoUsuarioAtual _provedorDoUsuarioAtual;

    public ObterResumoMensalManipulador(
        IConsultaDeResumoMensal consultaDeResumoMensal,
        IProvedorDoUsuarioAtual provedorDoUsuarioAtual)
    {
        _consultaDeResumoMensal = consultaDeResumoMensal;
        _provedorDoUsuarioAtual = provedorDoUsuarioAtual;
    }

    public async Task<RespostaDoResumoMensal> ManipularAsync(
        ObterResumoMensalConsulta consulta,
        CancellationToken cancelamento)
    {
        var competencia = CompetenciaMensal.Analisar(consulta.Competencia);

        var agregado = await _consultaDeResumoMensal.ObterAsync(
            _provedorDoUsuarioAtual.UsuarioId,
            competencia,
            cancelamento);

        var gastos = agregado.GastosPorCategoria
            .Select(gasto => ConverterGasto(gasto, agregado.TotalDeDespesas))
            .ToList();

        return new RespostaDoResumoMensal(
            competencia.ToString(),
            agregado.TotalDeReceitas,
            agregado.TotalDeDespesas,
            agregado.TotalDeReceitas - agregado.TotalDeDespesas,
            // Mês sem receita não tem renda para comprometer: devolve 0 em vez de dividir por zero.
            CalculadoraDePercentual.DeParticipacao(
                agregado.TotalDeDespesas,
                agregado.TotalDeReceitas),
            gastos);
    }

    private static RespostaDeGastoPorCategoria ConverterGasto(
        GastoDeCategoriaAgregado gasto,
        decimal totalDeDespesas)
    {
        var temTeto = gasto.TetoDefinido.HasValue;

        return new RespostaDeGastoPorCategoria(
            gasto.CategoriaId,
            gasto.NomeDaCategoria,
            gasto.CorHexadecimal,
            gasto.ValorGasto,
            CalculadoraDePercentual.DeParticipacao(gasto.ValorGasto, totalDeDespesas),
            gasto.TetoDefinido,
            // Sem teto não há percentual consumido nem situação — a tela mostra a linha sem barra.
            temTeto
                ? CalculadoraDePercentual.DeParticipacao(gasto.ValorGasto, gasto.TetoDefinido!.Value)
                : null,
            temTeto
                ? CalculadoraDeSituacaoDoTeto
                    .Calcular(gasto.ValorGasto, gasto.TetoDefinido!.Value)
                    .ToString()
                : null);
    }
}
