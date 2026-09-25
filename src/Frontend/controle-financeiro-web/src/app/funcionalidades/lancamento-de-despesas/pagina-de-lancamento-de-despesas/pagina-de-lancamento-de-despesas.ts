import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  inject,
  signal,
  viewChild,
} from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar } from '@angular/material/snack-bar';
import { DadosDaDespesa, Despesa } from '../../../compartilhado/modelos/despesa';
import { EstadoDaCompetencia } from '../../../compartilhado/servicos/estado-da-competencia';
import { ObservadorDeTamanhoDeTela } from '../../../compartilhado/servicos/observador-de-tamanho-de-tela';
import { ServicoDeCategorias } from '../../../compartilhado/servicos/servico-de-categorias';
import { FormatadorDeMoedaBrasileiraPipe } from '../../../nucleo/formatadores/formatador-de-moeda-brasileira.pipe';
import { FormularioDeDespesa } from '../formulario-de-despesa/formulario-de-despesa';
import { ServicoDeDespesas } from '../servicos/servico-de-despesas';
import {
  EdicaoDeDespesa,
  TabelaDeDespesasDoMes,
} from '../tabela-de-despesas-do-mes/tabela-de-despesas-do-mes';

@Component({
  selector: 'app-pagina-de-lancamento-de-despesas',
  imports: [
    MatButtonModule,
    MatIconModule,
    FormularioDeDespesa,
    TabelaDeDespesasDoMes,
    FormatadorDeMoedaBrasileiraPipe,
  ],
  templateUrl: './pagina-de-lancamento-de-despesas.html',
  styleUrl: './pagina-de-lancamento-de-despesas.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PaginaDeLancamentoDeDespesas {
  private readonly servicoDeDespesas = inject(ServicoDeDespesas);
  private readonly avisos = inject(MatSnackBar);

  protected readonly servicoDeCategorias = inject(ServicoDeCategorias);
  protected readonly estadoDaCompetencia = inject(EstadoDaCompetencia);
  protected readonly tela = inject(ObservadorDeTamanhoDeTela);

  private readonly formulario = viewChild(FormularioDeDespesa);
  private readonly tabela = viewChild.required(TabelaDeDespesasDoMes);

  protected readonly despesas = signal<readonly Despesa[]>([]);
  protected readonly carregando = signal(false);
  protected readonly salvando = signal(false);

  /** Em tela estreita o formulário fica atrás do botão flutuante; no desktop, sempre à vista. */
  private readonly abertoEmTelaEstreita = signal(false);
  protected readonly formularioVisivel = computed(
    () => this.tela.ehTelaLarga() || this.abertoEmTelaEstreita(),
  );

  protected readonly totalDoMes = computed(() =>
    this.despesas().reduce((soma, despesa) => soma + despesa.valor, 0),
  );

  /** Para onde foi a maior fatia do mês — a pergunta que a tela existe para responder. */
  protected readonly maiorCategoria = computed(() => {
    const porCategoria = new Map<string, { nome: string; cor: string; total: number }>();

    for (const despesa of this.despesas()) {
      const atual = porCategoria.get(despesa.categoriaId);

      porCategoria.set(despesa.categoriaId, {
        nome: despesa.nomeDaCategoria,
        cor: despesa.corDaCategoria,
        total: (atual?.total ?? 0) + despesa.valor,
      });
    }

    return [...porCategoria.values()].sort((uma, outra) => outra.total - uma.total)[0] ?? null;
  });

  protected readonly percentualDaMaiorCategoria = computed(() => {
    const maior = this.maiorCategoria();
    const total = this.totalDoMes();

    return maior === null || total <= 0 ? 0 : Math.round((maior.total / total) * 100);
  });

  /** Levantada ao abrir o formulário e baixada assim que o foco é colocado. */
  private readonly aguardandoFoco = signal(false);

  constructor() {
    this.servicoDeCategorias.recarregar();

    // Trocar a competência na barra do topo recarrega a tabela sozinho.
    effect(() => {
      const competencia = this.estadoDaCompetencia.comoTexto();
      this.recarregarDespesas(competencia);
    });

    // O formulário só existe no DOM depois que o template reage ao sinal, então o foco
    // espera o `viewChild` aparecer em vez de tentar na mesma volta do clique.
    effect(() => {
      const formulario = this.formulario();

      if (formulario && this.aguardandoFoco()) {
        this.aguardandoFoco.set(false);
        formulario.focarNaDescricao();
      }
    });
  }

  protected abrirFormulario(): void {
    this.abertoEmTelaEstreita.set(true);
    this.aguardandoFoco.set(true);
  }

  protected fecharFormulario(): void {
    this.abertoEmTelaEstreita.set(false);
  }

  protected registrar(dados: DadosDaDespesa): void {
    this.salvando.set(true);

    this.servicoDeDespesas.registrar(dados).subscribe({
      next: (despesa) => {
        this.salvando.set(false);
        this.absorverLancamento(despesa);
        this.formulario()?.prepararProximoLancamento();
        this.avisos.open(`"${despesa.descricao}" lançada.`, undefined, { duration: 2500 });
      },
      // O interceptador já mostrou o erro; aqui só destravamos o botão e preservamos o que foi digitado.
      error: () => this.salvando.set(false),
    });
  }

  protected aplicarEdicao({ id, dados }: EdicaoDeDespesa): void {
    this.servicoDeDespesas.editar(id, dados).subscribe({
      next: (despesa) => {
        this.tabela().encerrarEdicao();
        this.absorverEdicao(despesa);
        this.avisos.open('Despesa atualizada.', undefined, { duration: 2500 });
      },
    });
  }

  protected remover(despesa: Despesa): void {
    this.servicoDeDespesas.excluir(despesa.id).subscribe({
      next: () => {
        this.despesas.update((atuais) => atuais.filter((item) => item.id !== despesa.id));
        this.avisos.open(`"${despesa.descricao}" excluída.`, undefined, { duration: 2500 });
      },
    });
  }

  private recarregarDespesas(competencia: string): void {
    this.carregando.set(true);

    this.servicoDeDespesas.listarDoMes(competencia).subscribe({
      next: (despesas) => {
        this.despesas.set(despesas);
        this.carregando.set(false);
      },
      error: () => {
        this.despesas.set([]);
        this.carregando.set(false);
      },
    });
  }

  /**
   * Insere a despesa recém-criada na ordem da tabela sem uma segunda ida ao servidor.
   * Se ela caiu em outro mês, não entra na lista — a competência exibida continua sendo outra.
   */
  private absorverLancamento(despesa: Despesa): void {
    if (!despesa.dataDoGasto.startsWith(this.estadoDaCompetencia.comoTexto())) {
      this.avisos.open(
        `Lançada em ${despesa.dataDoGasto}, fora do mês que está sendo exibido.`,
        'Fechar',
        { duration: 5000 },
      );
      return;
    }

    this.despesas.update((atuais) => [...atuais, despesa].sort(compararPelaOrdemDaApi));
  }

  private absorverEdicao(despesa: Despesa): void {
    const continuaNoMes = despesa.dataDoGasto.startsWith(this.estadoDaCompetencia.comoTexto());

    this.despesas.update((atuais) => {
      const semAntiga = atuais.filter((item) => item.id !== despesa.id);

      return continuaNoMes ? [...semAntiga, despesa].sort(compararPelaOrdemDaApi) : semAntiga;
    });
  }
}

/**
 * Mesma ordenação do `order by data_do_gasto desc, criado_em desc` da API. Sem o desempate por
 * `criadoEm`, editar um lançamento faria ele pular de lugar entre os gastos do mesmo dia.
 */
function compararPelaOrdemDaApi(uma: Despesa, outra: Despesa): number {
  const porData = outra.dataDoGasto.localeCompare(uma.dataDoGasto);

  return porData !== 0 ? porData : outra.criadoEm.localeCompare(uma.criadoEm);
}
