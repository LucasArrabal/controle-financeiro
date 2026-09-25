using ControleFinanceiro.Aplicacao.Dtos.Respostas;

namespace ControleFinanceiro.Aplicacao.Contratos;

/// <summary>
/// Destino externo para onde o resumo de um mês pode ser publicado (Google Sheets, por exemplo).
/// Previsto na arquitetura de propósito e ainda sem implementação — o Sheets continua
/// fora do caminho de leitura e gravação do sistema, que é sempre o PostgreSQL.
/// </summary>
public interface IExportadorDeResumoMensal
{
    /// <summary>Publica o resumo e devolve o endereço do documento gerado.</summary>
    Task<Uri> ExportarAsync(RespostaDoResumoMensal resumo, CancellationToken cancelamento);
}
