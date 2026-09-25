import { ChangeDetectionStrategy, Component, inject, input, output, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSelectModule } from '@angular/material/select';
import { MatTooltipModule } from '@angular/material/tooltip';
import {
  DadosDaConfirmacao,
  DialogoDeConfirmacao,
} from '../../../compartilhado/componentes/dialogo-de-confirmacao/dialogo-de-confirmacao';
import {
  Categoria,
  DadosDaCategoria,
  TIPOS_DE_CATEGORIA,
} from '../../../compartilhado/modelos/categoria';

export interface EdicaoDeCategoria {
  readonly id: string;
  readonly dados: DadosDaCategoria;
}

const PADRAO_DE_COR_HEXADECIMAL = /^#[0-9a-fA-F]{6}$/;

/**
 * Categorias cadastradas, ativas e inativas. Cada linha edita nome, tipo e cor no lugar.
 * Desativar pede confirmação, porque a categoria some do formulário de lançamento; reativar
 * é imediato, já que é sempre reversível e não tira nada de ninguém.
 */
@Component({
  selector: 'app-lista-de-categorias',
  imports: [
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatTooltipModule,
    MatProgressBarModule,
  ],
  templateUrl: './lista-de-categorias.html',
  styleUrl: './lista-de-categorias.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ListaDeCategorias {
  private readonly construtorDeFormulario = inject(FormBuilder);
  private readonly dialogos = inject(MatDialog);

  readonly categorias = input.required<readonly Categoria[]>();
  readonly carregando = input(false);

  readonly editar = output<EdicaoDeCategoria>();
  readonly desativar = output<Categoria>();
  readonly reativar = output<Categoria>();

  protected readonly tipos = TIPOS_DE_CATEGORIA;
  protected readonly idEmEdicao = signal<string | null>(null);

  protected readonly formularioDeEdicao = this.construtorDeFormulario.nonNullable.group({
    nome: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(60)]],
    tipo: ['', [Validators.required]],
    cor: ['#000000', [Validators.required, Validators.pattern(PADRAO_DE_COR_HEXADECIMAL)]],
  });

  protected comecarEdicao(categoria: Categoria): void {
    this.formularioDeEdicao.setValue({
      nome: categoria.nome,
      tipo: categoria.tipo,
      cor: categoria.corHexadecimal,
    });

    this.idEmEdicao.set(categoria.id);
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

    const categoriaOriginal = this.categorias().find((categoria) => categoria.id === id);
    const valores = this.formularioDeEdicao.getRawValue();

    this.editar.emit({
      id,
      dados: {
        nome: valores.nome.trim(),
        tipo: valores.tipo as DadosDaCategoria['tipo'],
        corHexadecimal: valores.cor.toUpperCase(),
        // A edição preserva o estado ativo/inativo; só os botões dedicados o alteram.
        ativa: categoriaOriginal?.ativa ?? true,
      },
    });
  }

  /** Chamado pela página quando a API confirma a gravação. */
  encerrarEdicao(): void {
    this.idEmEdicao.set(null);
  }

  protected corParaOPickerDaEdicao(): string {
    const valor = this.formularioDeEdicao.controls.cor.value;
    return PADRAO_DE_COR_HEXADECIMAL.test(valor) ? valor : '#000000';
  }

  protected aoEscolherNoPickerDaEdicao(evento: Event): void {
    const valor = (evento.target as HTMLInputElement).value;
    this.formularioDeEdicao.controls.cor.setValue(valor.toUpperCase());
  }

  protected pedirDesativacao(categoria: Categoria): void {
    const dados: DadosDaConfirmacao = {
      titulo: 'Desativar categoria',
      mensagem:
        `"${categoria.nome}" some do formulário de lançamento, mas os gastos já registrados ` +
        'nela continuam no histórico e no painel. Dá para reativar a qualquer momento.',
      rotuloDeConfirmacao: 'Desativar',
    };

    this.dialogos
      .open<DialogoDeConfirmacao, DadosDaConfirmacao, boolean>(DialogoDeConfirmacao, {
        data: dados,
        width: 'min(28rem, calc(100vw - 2rem))',
        autoFocus: 'dialog',
      })
      .afterClosed()
      .subscribe((confirmou) => {
        if (confirmou) {
          this.idEmEdicao.set(null);
          this.desativar.emit(categoria);
        }
      });
  }

  protected rotuloDoTipo(tipo: string): string {
    return this.tipos.find((item) => item.valor === tipo)?.rotulo ?? tipo;
  }
}
