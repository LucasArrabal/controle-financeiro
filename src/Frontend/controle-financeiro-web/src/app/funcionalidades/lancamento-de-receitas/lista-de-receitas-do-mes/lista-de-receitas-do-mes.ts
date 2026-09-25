import { ChangeDetectionStrategy, Component, inject, input, output, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import {
  DadosDaConfirmacao,
  DialogoDeConfirmacao,
} from '../../../compartilhado/componentes/dialogo-de-confirmacao/dialogo-de-confirmacao';
import { DadosDaReceita, Receita } from '../../../compartilhado/modelos/receita';
import {
  dataPuraDeDate,
  dateDeDataPura,
} from '../../../nucleo/formatadores/conversor-de-data-pura';
import { FormatadorDeMoedaBrasileiraPipe } from '../../../nucleo/formatadores/formatador-de-moeda-brasileira.pipe';
import { interpretarMoedaBrasileira } from '../../../nucleo/formatadores/interpretador-de-moeda-brasileira';

export interface EdicaoDeReceita {
  readonly id: string;
  readonly dados: DadosDaReceita;
}

/**
 * Receitas do mês. São quatro campos apenas, então a lista tocável vale em qualquer largura —
 * uma tabela aqui só acrescentaria linhas de grade sem acrescentar informação.
 */
@Component({
  selector: 'app-lista-de-receitas-do-mes',
  imports: [
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatDatepickerModule,
    MatCheckboxModule,
    MatButtonModule,
    MatIconModule,
    MatTooltipModule,
    MatProgressBarModule,
    FormatadorDeMoedaBrasileiraPipe,
  ],
  templateUrl: './lista-de-receitas-do-mes.html',
  styleUrl: './lista-de-receitas-do-mes.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ListaDeReceitasDoMes {
  private readonly construtorDeFormulario = inject(FormBuilder);
  private readonly dialogos = inject(MatDialog);

  readonly receitas = input.required<readonly Receita[]>();
  readonly carregando = input(false);

  readonly editar = output<EdicaoDeReceita>();
  readonly excluir = output<Receita>();
  readonly replicar = output<Receita>();

  protected readonly idEmEdicao = signal<string | null>(null);

  protected readonly formularioDeEdicao = this.construtorDeFormulario.nonNullable.group({
    descricao: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(150)]],
    valor: ['', [Validators.required]],
    dataDoRecebimento: [new Date(), [Validators.required]],
    recorrente: [false],
  });

  protected comecarEdicao(receita: Receita): void {
    this.formularioDeEdicao.setValue({
      descricao: receita.descricao,
      valor: receita.valor.toFixed(2).replace('.', ','),
      dataDoRecebimento: dateDeDataPura(receita.dataDoRecebimento),
      recorrente: receita.recorrente,
    });

    this.idEmEdicao.set(receita.id);
  }

  protected cancelarEdicao(): void {
    this.idEmEdicao.set(null);
  }

  protected confirmarEdicao(): void {
    const id = this.idEmEdicao();

    if (id === null || this.formularioDeEdicao.invalid) {
      this.formularioDeEdicao.markAllAsTouched();
      return;
    }

    const valores = this.formularioDeEdicao.getRawValue();
    const valor = interpretarMoedaBrasileira(valores.valor);

    if (valor === null || valor <= 0) {
      this.formularioDeEdicao.controls.valor.setErrors({ valorIlegivel: true });
      return;
    }

    this.editar.emit({
      id,
      dados: {
        descricao: valores.descricao.trim(),
        valor,
        dataDoRecebimento: dataPuraDeDate(valores.dataDoRecebimento),
        recorrente: valores.recorrente,
      },
    });
  }

  /** Chamado pela página quando a API confirma a gravação. */
  encerrarEdicao(): void {
    this.idEmEdicao.set(null);
  }

  protected pedirExclusao(receita: Receita): void {
    const dados: DadosDaConfirmacao = {
      titulo: 'Excluir receita',
      mensagem: `"${receita.descricao}" será excluída definitivamente. Essa ação não pode ser desfeita.`,
      rotuloDeConfirmacao: 'Excluir',
      ehDestrutiva: true,
    };

    this.dialogos
      .open<DialogoDeConfirmacao, DadosDaConfirmacao, boolean>(DialogoDeConfirmacao, {
        data: dados,
        width: 'min(26rem, calc(100vw - 2rem))',
        autoFocus: 'dialog',
      })
      .afterClosed()
      .subscribe((confirmou) => {
        if (confirmou) {
          this.idEmEdicao.set(null);
          this.excluir.emit(receita);
        }
      });
  }

  protected apenasODia(data: string): string {
    return data.split('-')[2];
  }

  protected apenasOMes(data: string): string {
    return data.split('-')[1];
  }
}
