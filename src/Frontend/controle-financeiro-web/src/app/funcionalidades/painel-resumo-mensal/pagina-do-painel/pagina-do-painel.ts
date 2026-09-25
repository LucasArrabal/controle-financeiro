import { ChangeDetectionStrategy, Component, computed, effect, inject, signal } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { CartaoDeIndicador } from '../../../compartilhado/componentes/cartao-de-indicador/cartao-de-indicador';
import { RESUMO_MENSAL_VAZIO, ResumoMensal } from '../../../compartilhado/modelos/resumo-mensal';
import { EstadoDaCompetencia } from '../../../compartilhado/servicos/estado-da-competencia';
import { GraficoPizzaGastosPorCategoria } from '../grafico-pizza-gastos-por-categoria/grafico-pizza-gastos-por-categoria';
import { ListaDeTetosEConsumo } from '../lista-de-tetos-e-consumo/lista-de-tetos-e-consumo';
import { ServicoDoPainel } from '../servicos/servico-do-painel';

const formatadorDeMoeda = new Intl.NumberFormat('pt-BR', {
  style: 'currency',
  currency: 'BRL',
});

@Component({
  selector: 'app-pagina-do-painel',
  imports: [
    MatIconModule,
    MatProgressBarModule,
    CartaoDeIndicador,
    GraficoPizzaGastosPorCategoria,
    ListaDeTetosEConsumo,
  ],
  templateUrl: './pagina-do-painel.html',
  styleUrl: './pagina-do-painel.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PaginaDoPainel {
  private readonly servicoDoPainel = inject(ServicoDoPainel);

  protected readonly estadoDaCompetencia = inject(EstadoDaCompetencia);

  protected readonly resumo = signal<ResumoMensal>(RESUMO_MENSAL_VAZIO);
  protected readonly carregando = signal(false);

  protected readonly totalDeReceitasFormatado = computed(() =>
    formatadorDeMoeda.format(this.resumo().totalDeReceitas),
  );

  protected readonly totalDeDespesasFormatado = computed(() =>
    formatadorDeMoeda.format(this.resumo().totalDeDespesas),
  );

  protected readonly saldoFormatado = computed(() =>
    formatadorDeMoeda.format(this.resumo().saldo),
  );

  protected readonly rendaComprometidaFormatada = computed(
    () => `${this.resumo().percentualDaRendaComprometida.toFixed(1).replace('.', ',')}%`,
  );

  protected readonly saldoEhNegativo = computed(() => this.resumo().saldo < 0);

  /** Acima de 100% da renda o mês fecha no vermelho — é o número que pede atenção. */
  protected readonly rendaEstourada = computed(
    () => this.resumo().percentualDaRendaComprometida > 100,
  );

  protected readonly detalheDaRenda = computed(() => {
    const resumo = this.resumo();

    if (resumo.totalDeReceitas <= 0) {
      return 'Sem receita lançada no mês';
    }

    return this.rendaEstourada()
      ? 'Os gastos passaram do que entrou'
      : 'do que entrou já foi gasto';
  });

  protected readonly tetosEstourados = computed(
    () => this.resumo().gastosPorCategoria.filter((gasto) => gasto.situacaoDoTeto === 'Estourado'),
  );

  protected readonly nomesEstourados = computed(() =>
    this.tetosEstourados()
      .map((gasto) => gasto.nomeDaCategoria)
      .join(', '),
  );

  constructor() {
    effect(() => {
      const competencia = this.estadoDaCompetencia.comoTexto();
      this.recarregar(competencia);
    });
  }

  private recarregar(competencia: string): void {
    this.carregando.set(true);

    this.servicoDoPainel.obterResumoMensal(competencia).subscribe({
      next: (resumo) => {
        this.resumo.set(resumo);
        this.carregando.set(false);
      },
      error: () => {
        this.resumo.set(RESUMO_MENSAL_VAZIO);
        this.carregando.set(false);
      },
    });
  }
}
