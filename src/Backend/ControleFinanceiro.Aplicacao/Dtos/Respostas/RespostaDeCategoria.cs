namespace ControleFinanceiro.Aplicacao.Dtos.Respostas;

/// <summary>Categoria como a API a devolve. Nenhuma entidade de domínio cruza a fronteira.</summary>
public sealed record RespostaDeCategoria(
    Guid Id,
    string Nome,
    string Tipo,
    string CorHexadecimal,
    bool Ativa);
