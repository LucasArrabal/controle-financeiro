using ControleFinanceiro.Aplicacao.Contratos;
using ControleFinanceiro.Aplicacao.Contratos.Repositorios;
using ControleFinanceiro.Aplicacao.Dtos.Respostas;
using ControleFinanceiro.Dominio.Entidades;
using FluentValidation;

namespace ControleFinanceiro.Aplicacao.CasosDeUso.Receitas.RegistrarReceita;

public sealed class RegistrarReceitaManipulador
{
    private readonly IRepositorioDeReceitas _repositorioDeReceitas;
    private readonly IProvedorDoUsuarioAtual _provedorDoUsuarioAtual;
    private readonly IProvedorDeDataHora _provedorDeDataHora;
    private readonly IValidator<RegistrarReceitaComando> _validador;

    public RegistrarReceitaManipulador(
        IRepositorioDeReceitas repositorioDeReceitas,
        IProvedorDoUsuarioAtual provedorDoUsuarioAtual,
        IProvedorDeDataHora provedorDeDataHora,
        IValidator<RegistrarReceitaComando> validador)
    {
        _repositorioDeReceitas = repositorioDeReceitas;
        _provedorDoUsuarioAtual = provedorDoUsuarioAtual;
        _provedorDeDataHora = provedorDeDataHora;
        _validador = validador;
    }

    public async Task<RespostaDeReceita> ManipularAsync(
        RegistrarReceitaComando comando,
        CancellationToken cancelamento)
    {
        await _validador.ValidateAndThrowAsync(comando, cancelamento);

        var receita = Receita.Registrar(
            _provedorDoUsuarioAtual.UsuarioId,
            comando.Descricao,
            comando.Valor,
            comando.DataDoRecebimento,
            comando.Recorrente,
            _provedorDeDataHora.AgoraEmUtc);

        await _repositorioDeReceitas.InserirAsync(receita, cancelamento);

        return ConversorDeReceitaParaResposta.Converter(receita);
    }
}
