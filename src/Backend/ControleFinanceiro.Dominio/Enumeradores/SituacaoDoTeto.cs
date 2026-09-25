namespace ControleFinanceiro.Dominio.Enumeradores;

/// <summary>
/// Quão perto do teto mensal uma categoria está. Define a cor da barra de consumo no painel.
/// </summary>
public enum SituacaoDoTeto
{
    /// <summary>Consumo até 80% do teto.</summary>
    DentroDoLimite = 1,

    /// <summary>Consumo acima de 80% e até 100% do teto.</summary>
    EmAtencao = 2,

    /// <summary>Consumo acima de 100% do teto.</summary>
    Estourado = 3
}
