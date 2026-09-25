namespace ControleFinanceiro.Infraestrutura.Dados.Persistencia.Repositorios.Registros;

/// <summary>
/// Linha crua de <c>categorias_de_gasto</c>, como o Dapper a materializa.
/// Existe para que a entidade de domínio não precise de setters públicos nem de
/// construtor sem parâmetros só para agradar o mapeador.
/// </summary>
internal sealed record RegistroDeCategoria(
    Guid Id,
    Guid UsuarioId,
    string Nome,
    string Tipo,
    string CorHexadecimal,
    bool Ativa);
