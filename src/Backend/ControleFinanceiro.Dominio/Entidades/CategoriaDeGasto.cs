using System.Text.RegularExpressions;
using ControleFinanceiro.Dominio.Enumeradores;
using ControleFinanceiro.Dominio.Excecoes;

namespace ControleFinanceiro.Dominio.Entidades;

/// <summary>
/// Agrupador de despesas. Nunca é excluída de verdade: quando tem lançamentos, só é desativada.
/// </summary>
public sealed partial class CategoriaDeGasto
{
    public const int TamanhoMinimoDoNome = 2;
    public const int TamanhoMaximoDoNome = 60;

    private CategoriaDeGasto(
        Guid id,
        Guid usuarioId,
        string nome,
        TipoDeCategoria tipo,
        string corHexadecimal,
        bool ativa)
    {
        Id = id;
        UsuarioId = usuarioId;
        Nome = nome;
        Tipo = tipo;
        CorHexadecimal = corHexadecimal;
        Ativa = ativa;
    }

    public Guid Id { get; }

    public Guid UsuarioId { get; }

    public string Nome { get; private set; }

    public TipoDeCategoria Tipo { get; private set; }

    /// <summary>Cor usada no gráfico do painel, sempre em <c>#RRGGBB</c> maiúsculo.</summary>
    public string CorHexadecimal { get; private set; }

    public bool Ativa { get; private set; }

    public static CategoriaDeGasto Criar(
        Guid usuarioId,
        string nome,
        TipoDeCategoria tipo,
        string corHexadecimal) =>
        new(
            Guid.NewGuid(),
            usuarioId,
            NormalizarNome(nome),
            ValidarTipo(tipo),
            NormalizarCor(corHexadecimal),
            ativa: true);

    /// <summary>Reidrata uma categoria já persistida, sem reexecutar as regras de criação.</summary>
    public static CategoriaDeGasto Restaurar(
        Guid id,
        Guid usuarioId,
        string nome,
        TipoDeCategoria tipo,
        string corHexadecimal,
        bool ativa) =>
        new(id, usuarioId, nome, tipo, corHexadecimal, ativa);

    public void Renomear(string novoNome) => Nome = NormalizarNome(novoNome);

    public void AlterarTipo(TipoDeCategoria novoTipo) => Tipo = ValidarTipo(novoTipo);

    public void AlterarCor(string novaCorHexadecimal) => CorHexadecimal = NormalizarCor(novaCorHexadecimal);

    public void Desativar() => Ativa = false;

    public void Reativar() => Ativa = true;

    private static string NormalizarNome(string nome)
    {
        RegraDeNegocioVioladaException.LancarSe(
            string.IsNullOrWhiteSpace(nome),
            "Nome da categoria é obrigatório.");

        var recortado = nome.Trim();

        RegraDeNegocioVioladaException.LancarSe(
            recortado.Length is < TamanhoMinimoDoNome or > TamanhoMaximoDoNome,
            $"Nome da categoria deve ter entre {TamanhoMinimoDoNome} e {TamanhoMaximoDoNome} caracteres.");

        return recortado;
    }

    private static TipoDeCategoria ValidarTipo(TipoDeCategoria tipo)
    {
        RegraDeNegocioVioladaException.LancarSe(
            !Enum.IsDefined(tipo),
            $"Tipo de categoria '{tipo}' não é reconhecido.");

        return tipo;
    }

    private static string NormalizarCor(string corHexadecimal)
    {
        RegraDeNegocioVioladaException.LancarSe(
            string.IsNullOrWhiteSpace(corHexadecimal),
            "Cor da categoria é obrigatória.");

        var recortada = corHexadecimal.Trim().ToUpperInvariant();

        RegraDeNegocioVioladaException.LancarSe(
            !PadraoDeCorHexadecimal().IsMatch(recortada),
            "Cor da categoria deve estar no formato #RRGGBB (ex.: #2E7D32).");

        return recortada;
    }

    [GeneratedRegex("^#[0-9A-F]{6}$")]
    private static partial Regex PadraoDeCorHexadecimal();
}
