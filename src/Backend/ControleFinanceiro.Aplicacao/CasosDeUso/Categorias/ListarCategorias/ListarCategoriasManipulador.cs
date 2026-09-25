using ControleFinanceiro.Aplicacao.Contratos;
using ControleFinanceiro.Aplicacao.Contratos.Repositorios;
using ControleFinanceiro.Aplicacao.Dtos.Respostas;

namespace ControleFinanceiro.Aplicacao.CasosDeUso.Categorias.ListarCategorias;

public sealed class ListarCategoriasManipulador
{
    private readonly IRepositorioDeCategorias _repositorioDeCategorias;
    private readonly IProvedorDoUsuarioAtual _provedorDoUsuarioAtual;

    public ListarCategoriasManipulador(
        IRepositorioDeCategorias repositorioDeCategorias,
        IProvedorDoUsuarioAtual provedorDoUsuarioAtual)
    {
        _repositorioDeCategorias = repositorioDeCategorias;
        _provedorDoUsuarioAtual = provedorDoUsuarioAtual;
    }

    public async Task<IReadOnlyList<RespostaDeCategoria>> ManipularAsync(
        ListarCategoriasConsulta consulta,
        CancellationToken cancelamento)
    {
        var categorias = await _repositorioDeCategorias.ListarAsync(
            _provedorDoUsuarioAtual.UsuarioId,
            apenasAtivas: !consulta.IncluirInativas,
            cancelamento);

        return categorias.Select(ConversorDeCategoriaParaResposta.Converter).ToList();
    }
}
