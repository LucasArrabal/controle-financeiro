import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';

/** Cor do número, pelo que ele significa — não pelo valor em si. */
export type TomDoIndicador = 'neutro' | 'entrada' | 'saida' | 'atencao';

/**
 * Um número grande com rótulo. Componente burro de propósito: recebe o valor já formatado,
 * para a regra de formatação viver num lugar só e não se espalhar pelos cartões.
 */
@Component({
  selector: 'app-cartao-de-indicador',
  imports: [MatIconModule],
  templateUrl: './cartao-de-indicador.html',
  styleUrl: './cartao-de-indicador.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CartaoDeIndicador {
  readonly rotulo = input.required<string>();
  readonly valor = input.required<string>();
  readonly icone = input.required<string>();
  readonly tom = input<TomDoIndicador>('neutro');
  readonly detalhe = input<string | null>(null);
}
