import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  inject,
  input,
  output,
  viewChild,
} from '@angular/core';
import {
  FormBuilder,
  FormGroupDirective,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { DadosDaCategoria, TIPOS_DE_CATEGORIA } from '../../../compartilhado/modelos/categoria';

const PADRAO_DE_COR_HEXADECIMAL = /^#[0-9a-fA-F]{6}$/;

/**
 * Formulário de criação de categoria. Segue o mesmo comportamento de "Enter salva e o foco
 * volta" dos formulários de despesa e receita, para manter o hábito de quem já lança em série.
 */
@Component({
  selector: 'app-formulario-de-categoria',
  imports: [
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
  ],
  templateUrl: './formulario-de-categoria.html',
  styleUrl: './formulario-de-categoria.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FormularioDeCategoria {
  private readonly construtorDeFormulario = inject(FormBuilder);

  readonly salvando = input(false);

  readonly salvar = output<DadosDaCategoria>();

  protected readonly tipos = TIPOS_DE_CATEGORIA;

  private readonly campoDeNome = viewChild.required<ElementRef<HTMLInputElement>>('campoDeNome');
  private readonly diretivaDoFormulario = viewChild.required(FormGroupDirective);

  protected readonly formulario = this.construtorDeFormulario.nonNullable.group({
    nome: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(60)]],
    tipo: ['', [Validators.required]],
    cor: [proximaCorSugerida(), [Validators.required, Validators.pattern(PADRAO_DE_COR_HEXADECIMAL)]],
  });

  /** O seletor nativo de cor exige sempre um #RRGGBB válido; um valor em digitação usa um fallback neutro. */
  protected corParaOPicker(): string {
    const valor = this.formulario.controls.cor.value;
    return PADRAO_DE_COR_HEXADECIMAL.test(valor) ? valor : '#000000';
  }

  protected aoEscolherNoPicker(evento: Event): void {
    const valor = (evento.target as HTMLInputElement).value;
    this.formulario.controls.cor.setValue(valor.toUpperCase());
  }

  /** "Enter salva", exceto com um painel aberto (select), onde Enter escolhe a opção. */
  protected aoPressionarEnter(evento: Event): void {
    const alvo = evento.target as HTMLElement | null;

    if (alvo?.getAttribute('aria-expanded') === 'true') {
      return;
    }

    evento.preventDefault();
    this.enviar();
  }

  protected enviar(): void {
    if (this.formulario.invalid || this.salvando()) {
      this.formulario.markAllAsTouched();
      return;
    }

    const valores = this.formulario.getRawValue();

    this.salvar.emit({
      nome: valores.nome.trim(),
      tipo: valores.tipo as DadosDaCategoria['tipo'],
      corHexadecimal: valores.cor.toUpperCase(),
    });
  }

  /** Chamado pela página depois que o backend confirma a criação. */
  prepararProximaCategoria(): void {
    this.diretivaDoFormulario().resetForm({
      nome: '',
      tipo: '',
      cor: proximaCorSugerida(),
    });

    this.campoDeNome().nativeElement.focus();
  }
}

/** Sugere uma cor diferente a cada categoria nova, para não repetir sempre a mesma. */
function proximaCorSugerida(): string {
  const paleta = ['#5B4BE6', '#00897B', '#C2185B', '#F9A825', '#3949AB', '#6D4C41', '#00838F'];
  return paleta[Math.floor(Math.random() * paleta.length)];
}
