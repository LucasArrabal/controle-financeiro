import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { GastoPorCategoria, SituacaoDoTeto } from '../../../compartilhado/modelos/resumo-mensal';
import { FormatadorDeMoedaBrasileiraPipe } from '../../../nucleo/formatadores/formatador-de-moeda-brasileira.pipe';

/**
 * Consumo do teto de cada categoria. Categoria sem teto definido aparece na lista — com o
 * valor gasto e a fatia do mês — mas sem barra, porque não há limite contra o que medir.
 */
@Component({
  selector: 'app-lista-de-tetos-e-consumo',
  imports: [MatIconModule, MatTooltipModule, FormatadorDeMoedaBrasileiraPipe],
  templateUrl: './lista-de-tetos-e-consumo.html',
  styleUrl: './lista-de-tetos-e-consumo.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ListaDeTetosEConsumo {
  readonly gastos = input.required<readonly GastoPorCategoria[]>();

  /** A barra para de crescer em 100%; o estouro é comunicado pela cor e pelo número. */
  protected larguraDaBarra(gasto: GastoPorCategoria): number {
    return Math.min(gasto.percentualDoTetoConsumido ?? 0, 100);
  }

  protected percentualLegivel(percentual: number | null): string {
    return `${(percentual ?? 0).toFixed(0)}%`;
  }

  protected descricaoDaSituacao(situacao: SituacaoDoTeto | null): string {
    switch (situacao) {
      case 'DentroDoLimite':
        return 'Dentro do limite';
      case 'EmAtencao':
        return 'Chegando no limite';
      case 'Estourado':
        return 'Teto estourado';
      default:
        return 'Sem teto definido';
    }
  }
}
