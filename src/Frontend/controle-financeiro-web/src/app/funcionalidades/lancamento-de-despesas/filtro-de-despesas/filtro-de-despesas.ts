import { ChangeDetectionStrategy, Component, computed, effect, inject, input, output } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { Categoria } from '../../../compartilhado/modelos/categoria';

export interface FiltroDeDespesasValor {
  readonly texto: string;
  readonly categoriaIds: readonly string[];
}

const FILTRO_VAZIO = { texto: '', categoriaIds: [] as string[] };

/**
 * Filtro da lista do mês: texto livre (casa com descrição, observação ou forma de pagamento)
 * e categoria, combináveis. Emite o valor a cada mudança — a página é quem filtra, este
 * componente só descreve o que foi pedido.
 */
@Component({
  selector: 'app-filtro-de-despesas',
  imports: [
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
  ],
  templateUrl: './filtro-de-despesas.html',
  styleUrl: './filtro-de-despesas.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FiltroDeDespesas {
  private readonly construtorDeFormulario = inject(FormBuilder);

  readonly categorias = input.required<readonly Categoria[]>();

  readonly filtroAlterado = output<FiltroDeDespesasValor>();

  protected readonly formulario = this.construtorDeFormulario.nonNullable.group({
    texto: [''],
    categoriaIds: [[] as string[]],
  });

  private readonly valores = toSignal(this.formulario.valueChanges, {
    initialValue: this.formulario.getRawValue(),
  });

  /** Signal (não getter) de propósito: o botão "limpar" precisa reagir sem depender de zone.js. */
  protected readonly temFiltroAtivo = computed(() => {
    const valores = this.valores();
    return (valores.texto ?? '').trim() !== '' || (valores.categoriaIds ?? []).length > 0;
  });

  constructor() {
    // Dispara na digitação e na troca de categoria; filtrar uma lista de um mês inteiro
    // é barato o bastante para não precisar de debounce.
    effect(() => {
      const valores = this.valores();

      this.filtroAlterado.emit({
        texto: (valores.texto ?? '').trim().toLowerCase(),
        categoriaIds: valores.categoriaIds ?? [],
      });
    });
  }

  protected limparTexto(): void {
    this.formulario.controls.texto.setValue('');
  }

  protected limparTudo(): void {
    this.formulario.reset(FILTRO_VAZIO);
  }
}
