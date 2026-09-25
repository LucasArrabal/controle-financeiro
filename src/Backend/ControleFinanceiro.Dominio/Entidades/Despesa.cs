using ControleFinanceiro.Dominio.Excecoes;
using ControleFinanceiro.Dominio.ObjetosDeValor;

namespace ControleFinanceiro.Dominio.Entidades;

/// <summary>
/// Um gasto lançado pelo usuário. <see cref="DataDoGasto"/> é data pura (sem fuso):
/// o dia em que o dinheiro saiu, do ponto de vista de quem gastou.
/// </summary>
public sealed class Despesa
{
    public const int TamanhoMinimoDaDescricao = 2;
    public const int TamanhoMaximoDaDescricao = 150;
    public const int TamanhoMaximoDaFormaDePagamento = 30;

    /// <summary>Quanto tempo no futuro um gasto pode ser lançado.</summary>
    public static readonly int MesesDeToleranciaNoFuturo = 12;

    private Despesa(
        Guid id,
        Guid usuarioId,
        string descricao,
        ValorMonetario valor,
        DateOnly dataDoGasto,
        Guid categoriaId,
        string? formaDePagamento,
        string? observacao,
        DateTimeOffset criadoEm)
    {
        Id = id;
        UsuarioId = usuarioId;
        Descricao = descricao;
        Valor = valor;
        DataDoGasto = dataDoGasto;
        CategoriaId = categoriaId;
        FormaDePagamento = formaDePagamento;
        Observacao = observacao;
        CriadoEm = criadoEm;
    }

    public Guid Id { get; }

    public Guid UsuarioId { get; }

    public string Descricao { get; private set; }

    public ValorMonetario Valor { get; private set; }

    public DateOnly DataDoGasto { get; private set; }

    public Guid CategoriaId { get; private set; }

    public string? FormaDePagamento { get; private set; }

    public string? Observacao { get; private set; }

    /// <summary>Instante da criação do registro, sempre em UTC.</summary>
    public DateTimeOffset CriadoEm { get; }

    public CompetenciaMensal Competencia => CompetenciaMensal.DeData(DataDoGasto);

    public static Despesa Registrar(
        Guid usuarioId,
        string descricao,
        decimal valor,
        DateOnly dataDoGasto,
        Guid categoriaId,
        string? formaDePagamento,
        string? observacao,
        DateOnly hoje,
        DateTimeOffset criadoEm) =>
        new(
            Guid.NewGuid(),
            usuarioId,
            NormalizarDescricao(descricao),
            ValorMonetario.CriarPositivo(valor),
            ValidarDataDoGasto(dataDoGasto, hoje),
            ValidarCategoria(categoriaId),
            NormalizarFormaDePagamento(formaDePagamento),
            NormalizarObservacao(observacao),
            criadoEm.ToUniversalTime());

    /// <summary>Reidrata uma despesa já persistida, sem reexecutar as regras de criação.</summary>
    public static Despesa Restaurar(
        Guid id,
        Guid usuarioId,
        string descricao,
        decimal valor,
        DateOnly dataDoGasto,
        Guid categoriaId,
        string? formaDePagamento,
        string? observacao,
        DateTimeOffset criadoEm) =>
        new(
            id,
            usuarioId,
            descricao,
            ValorMonetario.Criar(valor),
            dataDoGasto,
            categoriaId,
            formaDePagamento,
            observacao,
            criadoEm);

    public void Alterar(
        string descricao,
        decimal valor,
        DateOnly dataDoGasto,
        Guid categoriaId,
        string? formaDePagamento,
        string? observacao,
        DateOnly hoje)
    {
        Descricao = NormalizarDescricao(descricao);
        Valor = ValorMonetario.CriarPositivo(valor);
        DataDoGasto = ValidarDataDoGasto(dataDoGasto, hoje);
        CategoriaId = ValidarCategoria(categoriaId);
        FormaDePagamento = NormalizarFormaDePagamento(formaDePagamento);
        Observacao = NormalizarObservacao(observacao);
    }

    private static string NormalizarDescricao(string descricao)
    {
        RegraDeNegocioVioladaException.LancarSe(
            string.IsNullOrWhiteSpace(descricao),
            "Descrição da despesa é obrigatória.");

        var recortada = descricao.Trim();

        RegraDeNegocioVioladaException.LancarSe(
            recortada.Length is < TamanhoMinimoDaDescricao or > TamanhoMaximoDaDescricao,
            $"Descrição da despesa deve ter entre {TamanhoMinimoDaDescricao} e {TamanhoMaximoDaDescricao} caracteres.");

        return recortada;
    }

    private static DateOnly ValidarDataDoGasto(DateOnly dataDoGasto, DateOnly hoje)
    {
        RegraDeNegocioVioladaException.LancarSe(
            dataDoGasto > hoje.AddMonths(MesesDeToleranciaNoFuturo),
            "Data do gasto não pode estar mais de 1 ano no futuro.");

        return dataDoGasto;
    }

    private static Guid ValidarCategoria(Guid categoriaId)
    {
        RegraDeNegocioVioladaException.LancarSe(
            categoriaId == Guid.Empty,
            "Categoria da despesa é obrigatória.");

        return categoriaId;
    }

    private static string? NormalizarFormaDePagamento(string? formaDePagamento)
    {
        if (string.IsNullOrWhiteSpace(formaDePagamento))
        {
            return null;
        }

        var recortada = formaDePagamento.Trim();

        RegraDeNegocioVioladaException.LancarSe(
            recortada.Length > TamanhoMaximoDaFormaDePagamento,
            $"Forma de pagamento deve ter no máximo {TamanhoMaximoDaFormaDePagamento} caracteres.");

        return recortada;
    }

    private static string? NormalizarObservacao(string? observacao) =>
        string.IsNullOrWhiteSpace(observacao) ? null : observacao.Trim();
}
