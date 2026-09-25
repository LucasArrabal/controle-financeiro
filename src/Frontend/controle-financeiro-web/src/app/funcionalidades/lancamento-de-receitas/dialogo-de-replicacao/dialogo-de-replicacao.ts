import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';

export interface DadosDaReplicacao {
  readonly descricao: string;
  /** Mês de origem por extenso, ex.: `setembro de 2026`. */
  readonly competenciaDeOrigem: string;
}

/**
 * Oferecido logo depois de lançar uma receita marcada como recorrente: é o momento em que
 * a pessoa sabe se aquele salário se repete, e o único clique que evita relançá-lo doze vezes.
 */
@Component({
  selector: 'app-dialogo-de-replicacao',
  imports: [MatDialogModule, MatButtonModule, MatButtonToggleModule],
  templateUrl: './dialogo-de-replicacao.html',
  styleUrl: './dialogo-de-replicacao.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DialogoDeReplicacao {
  protected readonly dados = inject<DadosDaReplicacao>(MAT_DIALOG_DATA);
  private readonly referencia = inject<MatDialogRef<DialogoDeReplicacao, number>>(MatDialogRef);

  protected readonly opcoesDeMeses = [3, 6, 11] as const;

  /** Onze cobre o resto do ano a partir do mês seguinte. */
  protected readonly mesesEscolhidos = signal<number>(11);

  protected confirmar(): void {
    this.referencia.close(this.mesesEscolhidos());
  }

  protected recusar(): void {
    this.referencia.close(undefined);
  }
}
