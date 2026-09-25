namespace ControleFinanceiro.Infraestrutura.Dados.Persistencia.Repositorios.Registros;

/// <summary>Linha crua de <c>tetos_de_gasto_mensal</c>, como o Dapper a materializa.</summary>
internal sealed record RegistroDeTetoDeGasto(
    Guid Id,
    Guid UsuarioId,
    Guid CategoriaId,
    string Competencia,
    decimal ValorLimite);
