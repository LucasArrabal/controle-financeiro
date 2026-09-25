using ControleFinanceiro.Aplicacao.Contratos;
using ControleFinanceiro.Aplicacao.Contratos.Repositorios;
using ControleFinanceiro.Aplicacao.Dtos.Respostas;
using ControleFinanceiro.Dominio.Excecoes;
using ControleFinanceiro.Dominio.ObjetosDeValor;

namespace ControleFinanceiro.Aplicacao.CasosDeUso.TetosDeGasto.CopiarTetosDoMesAnterior;

public sealed class CopiarTetosDoMesAnteriorManipulador
{
    private readonly IRepositorioDeTetosDeGasto _repositorioDeTetos;
    private readonly IRepositorioDeCategorias _repositorioDeCategorias;
    private readonly IProvedorDoUsuarioAtual _provedorDoUsuarioAtual;

    public CopiarTetosDoMesAnteriorManipulador(
        IRepositorioDeTetosDeGasto repositorioDeTetos,
        IRepositorioDeCategorias repositorioDeCategorias,
        IProvedorDoUsuarioAtual provedorDoUsuarioAtual)
    {
        _repositorioDeTetos = repositorioDeTetos;
        _repositorioDeCategorias = repositorioDeCategorias;
        _provedorDoUsuarioAtual = provedorDoUsuarioAtual;
    }

    public async Task<IReadOnlyList<RespostaDeTetoDeGasto>> ManipularAsync(
        CopiarTetosDoMesAnteriorComando comando,
        CancellationToken cancelamento)
    {
        var destino = CompetenciaMensal.Analisar(comando.Competencia);
        var origem = destino.Anterior();
        var usuarioId = _provedorDoUsuarioAtual.UsuarioId;

        var tetosDaOrigem = await _repositorioDeTetos.ListarPorCompetenciaAsync(
            usuarioId,
            origem,
            cancelamento);

        RegraDeNegocioVioladaException.LancarSe(
            tetosDaOrigem.Count == 0,
            $"O mês {origem} não tem nenhum teto definido para copiar.");

        var categoriasAtivas = await _repositorioDeCategorias.ListarAsync(
            usuarioId,
            apenasAtivas: true,
            cancelamento);

        var idsAtivos = categoriasAtivas.Select(categoria => categoria.Id).ToHashSet();

        // Categoria desativada depois do mês anterior não volta a ter teto no mês novo.
        var copias = tetosDaOrigem
            .Where(teto => idsAtivos.Contains(teto.CategoriaId))
            .Select(teto => teto.CopiarPara(destino))
            .ToList();

        await _repositorioDeTetos.SalvarEmLoteAsync(
            usuarioId,
            destino,
            copias,
            categoriasSemTeto: [],
            cancelamento);

        var atualizados = await _repositorioDeTetos.ListarPorCompetenciaAsync(
            usuarioId,
            destino,
            cancelamento);

        return MontadorDeRespostaDeTetos.Montar(categoriasAtivas, atualizados);
    }
}
