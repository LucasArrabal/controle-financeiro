import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { ChartConfiguration, ChartData } from 'chart.js';
import { BaseChartDirective } from 'ng2-charts';
import { GastoPorCategoria } from '../../../compartilhado/modelos/resumo-mensal';
import { FormatadorDeMoedaBrasileiraPipe } from '../../../nucleo/formatadores/formatador-de-moeda-brasileira.pipe';

const formatadorDeMoeda = new Intl.NumberFormat('pt-BR', {
  style: 'currency',
  currency: 'BRL',
});

/**
 * Distribuição dos gastos do mês, cada fatia na cor cadastrada da categoria.
 * Sem legenda própria: a lista de tetos logo abaixo já traz nome e cor de cada categoria,
 * e uma segunda legenda só repetiria a informação.
 */
@Component({
  selector: 'app-grafico-pizza-gastos-por-categoria',
  imports: [BaseChartDirective, MatIconModule, FormatadorDeMoedaBrasileiraPipe],
  templateUrl: './grafico-pizza-gastos-por-categoria.html',
  styleUrl: './grafico-pizza-gastos-por-categoria.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class GraficoPizzaGastosPorCategoria {
  readonly gastos = input.required<readonly GastoPorCategoria[]>();
  readonly totalDeDespesas = input.required<number>();

  /** Categoria sem gasto não vira fatia de tamanho zero no gráfico. */
  protected readonly fatias = computed(() =>
    this.gastos().filter((gasto) => gasto.valorGasto > 0),
  );

  protected readonly dados = computed<ChartData<'doughnut'>>(() => ({
    labels: this.fatias().map((fatia) => fatia.nomeDaCategoria),
    datasets: [
      {
        data: this.fatias().map((fatia) => fatia.valorGasto),
        backgroundColor: this.fatias().map((fatia) => fatia.corHexadecimal),
        borderWidth: 0,
        // Um respiro entre as fatias no lugar de borda colorida, que teria de mudar
        // junto com o tema claro/escuro.
        spacing: 3,
        hoverOffset: 8,
      },
    ],
  }));

  protected readonly opcoes: ChartConfiguration<'doughnut'>['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    cutout: '68%',
    plugins: {
      legend: { display: false },
      tooltip: {
        padding: 12,
        boxPadding: 6,
        callbacks: {
          label: (contexto) => {
            const fatia = this.fatias()[contexto.dataIndex];

            if (!fatia) {
              return '';
            }

            const percentual = fatia.percentualDoTotalGasto
              .toFixed(1)
              .replace('.', ',');

            return ` ${formatadorDeMoeda.format(fatia.valorGasto)} · ${percentual}%`;
          },
        },
      },
    },
  };
}
