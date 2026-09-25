import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';

export interface DadosDaConfirmacao {
  readonly titulo: string;
  readonly mensagem: string;
  readonly rotuloDeConfirmacao?: string;
  readonly ehDestrutiva?: boolean;
}

/** Diálogo curto de sim/não, usado antes de qualquer exclusão. */
@Component({
  selector: 'app-dialogo-de-confirmacao',
  imports: [MatButtonModule, MatDialogModule],
  templateUrl: './dialogo-de-confirmacao.html',
  styleUrl: './dialogo-de-confirmacao.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DialogoDeConfirmacao {
  protected readonly dados = inject<DadosDaConfirmacao>(MAT_DIALOG_DATA);
  private readonly referencia = inject<MatDialogRef<DialogoDeConfirmacao, boolean>>(MatDialogRef);

  protected confirmar(): void {
    this.referencia.close(true);
  }

  protected cancelar(): void {
    this.referencia.close(false);
  }
}
