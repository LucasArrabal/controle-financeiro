using ControleFinanceiro.Dominio.Excecoes;
using ControleFinanceiro.Dominio.ObjetosDeValor;

namespace ControleFinanceiro.Dominio.Entidades;

/// <summary>
/// Uma entrada de dinheiro (salário, freela, reembolso). <see cref="Recorrente"/> marca
/// os lançamentos que se repetem todo mês e podem ser replicados nas competências seguintes.
/// </summary>
public sealed class Receita
{
    public const int TamanhoMinimoDaDescricao = 2;
    public const int TamanhoMaximoDaDescricao = 150;

    private Receita(
        Guid id,
        Guid usuarioId,
        string descricao,
        ValorMonetario valor,
        DateOnly dataDoRecebimento,
        bool recorrente,
        DateTimeOffset criadoEm)
    {
        Id = id;
        UsuarioId = usuarioId;
        Descricao = descricao;
        Valor = valor;
        DataDoRecebimento = dataDoRecebimento;
        Recorrente = recorrente;
        CriadoEm = criadoEm;
    }

    public Guid Id { get; }

    public Guid UsuarioId { get; }

    public string Descricao { get; private set; }

    public ValorMonetario Valor { get; private set; }

    public DateOnly DataDoRecebimento { get; private set; }

    public bool Recorrente { get; private set; }

    /// <summary>Instante da criação do registro, sempre em UTC.</summary>
    public DateTimeOffset CriadoEm { get; }

    public CompetenciaMensal Competencia => CompetenciaMensal.DeData(DataDoRecebimento);

    public static Receita Registrar(
        Guid usuarioId,
        string descricao,
        decimal valor,
        DateOnly dataDoRecebimento,
        bool recorrente,
        DateTimeOffset criadoEm) =>
        new(
            Guid.NewGuid(),
            usuarioId,
            NormalizarDescricao(descricao),
            ValorMonetario.CriarPositivo(valor),
            dataDoRecebimento,
            recorrente,
            criadoEm.ToUniversalTime());

    /// <summary>Reidrata uma receita já persistida, sem reexecutar as regras de criação.</summary>
    public static Receita Restaurar(
        Guid id,
        Guid usuarioId,
        string descricao,
        decimal valor,
        DateOnly dataDoRecebimento,
        bool recorrente,
        DateTimeOffset criadoEm) =>
        new(
            id,
            usuarioId,
            descricao,
            ValorMonetario.Criar(valor),
            dataDoRecebimento,
            recorrente,
            criadoEm);

    public void Alterar(string descricao, decimal valor, DateOnly dataDoRecebimento, bool recorrente)
    {
        Descricao = NormalizarDescricao(descricao);
        Valor = ValorMonetario.CriarPositivo(valor);
        DataDoRecebimento = dataDoRecebimento;
        Recorrente = recorrente;
    }

    /// <summary>Gera a cópia desta receita na competência informada, mantendo o dia do recebimento.</summary>
    public Receita ReplicarPara(CompetenciaMensal competencia, DateTimeOffset criadoEm)
    {
        var ultimoDiaDoMes = competencia.UltimoDia.Day;
        var dia = Math.Min(DataDoRecebimento.Day, ultimoDiaDoMes);

        return new Receita(
            Guid.NewGuid(),
            UsuarioId,
            Descricao,
            Valor,
            new DateOnly(competencia.Ano, competencia.Mes, dia),
            Recorrente,
            criadoEm.ToUniversalTime());
    }

    private static string NormalizarDescricao(string descricao)
    {
        RegraDeNegocioVioladaException.LancarSe(
            string.IsNullOrWhiteSpace(descricao),
            "Descrição da receita é obrigatória.");

        var recortada = descricao.Trim();

        RegraDeNegocioVioladaException.LancarSe(
            recortada.Length is < TamanhoMinimoDaDescricao or > TamanhoMaximoDaDescricao,
            $"Descrição da receita deve ter entre {TamanhoMinimoDaDescricao} e {TamanhoMaximoDaDescricao} caracteres.");

        return recortada;
    }
}
