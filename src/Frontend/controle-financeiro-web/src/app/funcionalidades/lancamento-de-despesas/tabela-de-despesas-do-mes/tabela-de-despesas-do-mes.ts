import { ChangeDetectionStrategy, Component, inject, input, output, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSelectModule } from '@angular/material/select';
import { MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import {
  DadosDaConfirmacao,
  DialogoDeConfirmacao,
} from '../../../compartilhado/componentes/dialogo-de-confirmacao/dialogo-de-confirmacao';
import { Categoria } from '../../../compartilhado/modelos/categoria';
import { DadosDaDespesa, Despesa } from '../../../compartilhado/modelos/despesa';
import { ObservadorDeTamanhoDeTela } from '../../../compartilhado/servicos/observador-de-tamanho-de-tela';
import {
  dataPuraDeDate,
  dateDeDataPura,
} from '../../../nucleo/formatadores/conversor-de-data-pura';
import { FormatadorDeMoedaBrasileiraPipe } from '../../../nucleo/formatadores/formatador-de-moeda-brasileira.pipe';
import { interpretarMoedaBrasileira } from '../../../nucleo/formatadores/interpretador-de-moeda-brasileira';

export interface EdicaoDeDespesa {
  readonly id: string;
  readonly dados: DadosDaDespesa;
}

/**
 * Lançamentos do mês. No desktop é uma tabela; no celular, uma lista tocável em que a linha
 * inteira abre a edição — tabela de seis colunas não cabe num telefone.
 * Os dois modos compartilham o mesmo formulário de edição e a mesma confirmação de exclusão.
 */
@Component({
  selector: 'app-tabela-de-despesas-do-mes',
  imports: [
    ReactiveFormsModule,
    MatTableModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatDatepickerModule,
    MatButtonModule,
    MatIconModule,
    MatTooltipModule,
    MatProgressBarModule,
    FormatadorDeMoedaBrasileiraPipe,
  ],
  templateUrl: './tabela-de-despesas-do-mes.html',
  styleUrl: './tabela-de-despesas-do-mes.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TabelaDeDespesasDoMes {
  private readonly construtorDeFormulario = inject(FormBuilder);
  private readonly dialogos = inject(MatDialog);

  protected readonly tela = inject(ObservadorDeTamanhoDeTela);

  readonly despesas = input.required<readonly Despesa[]>();
  readonly categorias = input.required<readonly Categoria[]>();
  readonly carregando = input(false);
  /** Diferencia "nenhum lançamento no mês" de "nenhum resultado para este filtro". */
  readonly filtroAtivo = input(false);

  readonly editar = output<EdicaoDeDespesa>();
  readonly excluir = output<Despesa>();

  protected readonly colunas = [
    'dataDoGasto',
    'descricao',
    'categoria',
    'formaDePagamento',
    'valor',
    'acoes',
  ] as const;

  protected readonly idEmEdicao = signal<string | null>(null);

  protected readonly formularioDeEdicao = this.construtorDeFormulario.nonNullable.group({
    descricao: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(150)]],
    valor: ['', [Validators.required]],
    dataDoGasto: [new Date(), [Validators.required]],
    categoriaId: ['', [Validators.required]],
    formaDePagamento: [''],
    observacao: [''],
  });

  protected comecarEdicao(despesa: Despesa): void {
    this.formularioDeEdicao.setValue({
      descricao: despesa.descricao,
      valor: despesa.valor.toFixed(2).replace('.', ','),
      dataDoGasto: dateDeDataPura(despesa.dataDoGasto),
      categoriaId: despesa.categoriaId,
      formaDePagamento: despesa.formaDePagamento ?? '',
      observacao: despesa.observacao ?? '',
    });

    this.idEmEdicao.set(despesa.id);
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
        dataDoGasto: dataPuraDeDate(valores.dataDoGasto),
        categoriaId: valores.categoriaId,
        formaDePagamento: valores.formaDePagamento.trim() || null,
        observacao: valores.observacao.trim() || null,
      },
    });
  }

  /** Chamado pela página quando a API confirma a gravação. */
  encerrarEdicao(): void {
    this.idEmEdicao.set(null);
  }

  protected pedirExclusao(despesa: Despesa): void {
    const dados: DadosDaConfirmacao = {
      titulo: 'Excluir despesa',
      mensagem: `"${despesa.descricao}" será excluída definitivamente. Essa ação não pode ser desfeita.`,
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
          this.excluir.emit(despesa);
        }
      });
  }

  protected diaEMes(dataDoGasto: string): string {
    const [, mes, dia] = dataDoGasto.split('-');
    return `${dia}/${mes}`;
  }

  protected apenasODia(dataDoGasto: string): string {
    return dataDoGasto.split('-')[2];
  }

  protected apenasOMes(dataDoGasto: string): string {
    return dataDoGasto.split('-')[1];
  }
}
