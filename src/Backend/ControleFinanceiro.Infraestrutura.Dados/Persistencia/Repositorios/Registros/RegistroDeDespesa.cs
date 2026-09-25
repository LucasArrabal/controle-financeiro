namespace ControleFinanceiro.Infraestrutura.Dados.Persistencia.Repositorios.Registros;

/// <summary>Linha crua de <c>despesas</c>, como o Dapper a materializa.</summary>
internal sealed record RegistroDeDespesa(
    Guid Id,
    Guid UsuarioId,
    string Descricao,
    decimal Valor,
    DateOnly DataDoGasto,
    Guid CategoriaId,
    string? FormaDePagamento,
    string? Observacao,
    DateTime CriadoEm);
