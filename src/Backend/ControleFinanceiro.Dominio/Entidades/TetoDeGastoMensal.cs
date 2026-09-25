using ControleFinanceiro.Dominio.Excecoes;
using ControleFinanceiro.Dominio.ObjetosDeValor;

namespace ControleFinanceiro.Dominio.Entidades;

/// <summary>
/// Limite de gasto de uma categoria numa competência. Zero é um teto válido e significa
/// "nenhum gasto permitido" — diferente de não ter teto definido, que é a ausência do registro.
/// </summary>
public sealed class TetoDeGastoMensal
{
    private TetoDeGastoMensal(
        Guid id,
        Guid usuarioId,
        Guid categoriaId,
        CompetenciaMensal competencia,
        ValorMonetario valorLimite)
    {
        Id = id;
        UsuarioId = usuarioId;
        CategoriaId = categoriaId;
        Competencia = competencia;
        ValorLimite = valorLimite;
    }

    public Guid Id { get; }

    public Guid UsuarioId { get; }

    public Guid CategoriaId { get; }

    public CompetenciaMensal Competencia { get; }

    public ValorMonetario ValorLimite { get; private set; }

    public static TetoDeGastoMensal Definir(
        Guid usuarioId,
        Guid categoriaId,
        CompetenciaMensal competencia,
        decimal valorLimite)
    {
        RegraDeNegocioVioladaException.LancarSe(
            categoriaId == Guid.Empty,
            "Categoria do teto de gasto é obrigatória.");

        return new TetoDeGastoMensal(
            Guid.NewGuid(),
            usuarioId,
            categoriaId,
            competencia,
            ValorMonetario.Criar(valorLimite));
    }

    /// <summary>Reidrata um teto já persistido, sem reexecutar as regras de criação.</summary>
    public static TetoDeGastoMensal Restaurar(
        Guid id,
        Guid usuarioId,
        Guid categoriaId,
        CompetenciaMensal competencia,
        decimal valorLimite) =>
        new(id, usuarioId, categoriaId, competencia, ValorMonetario.Criar(valorLimite));

    public void AlterarLimite(decimal novoValorLimite) =>
        ValorLimite = ValorMonetario.Criar(novoValorLimite);

    /// <summary>Gera a cópia deste teto na competência informada, mantendo categoria e limite.</summary>
    public TetoDeGastoMensal CopiarPara(CompetenciaMensal competencia) =>
        new(Guid.NewGuid(), UsuarioId, CategoriaId, competencia, ValorLimite);
}
