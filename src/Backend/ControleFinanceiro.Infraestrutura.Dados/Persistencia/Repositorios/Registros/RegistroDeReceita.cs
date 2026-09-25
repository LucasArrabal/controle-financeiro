namespace ControleFinanceiro.Infraestrutura.Dados.Persistencia.Repositorios.Registros;

/// <summary>Linha crua de <c>receitas</c>, como o Dapper a materializa.</summary>
internal sealed record RegistroDeReceita(
    Guid Id,
    Guid UsuarioId,
    string Descricao,
    decimal Valor,
    DateOnly DataDoRecebimento,
    bool Recorrente,
    DateTime CriadoEm);
