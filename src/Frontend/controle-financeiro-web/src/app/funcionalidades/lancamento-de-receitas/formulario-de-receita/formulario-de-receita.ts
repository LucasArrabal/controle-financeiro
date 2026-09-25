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
  AbstractControl,
  FormBuilder,
  FormGroupDirective,
  ReactiveFormsModule,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { DadosDaReceita } from '../../../compartilhado/modelos/receita';
import { dataPuraDeDate } from '../../../nucleo/formatadores/conversor-de-data-pura';
import { interpretarMoedaBrasileira } from '../../../nucleo/formatadores/interpretador-de-moeda-brasileira';

/**
 * Formulário de entrada de dinheiro. Mesmo comportamento do de despesa — Enter salva e o foco
 * volta para a descrição — com um campo a menos e a marca de recorrente no lugar da categoria.
 */
@Component({
  selector: 'app-formulario-de-receita',
  imports: [
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatDatepickerModule,
    MatCheckboxModule,
    MatButtonModule,
    MatIconModule,
  ],
  templateUrl: './formulario-de-receita.html',
  styleUrl: './formulario-de-receita.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FormularioDeReceita {
  private readonly construtorDeFormulario = inject(FormBuilder);

  readonly salvando = input(false);

  readonly salvar = output<DadosDaReceita>();

  private readonly campoDeDescricao =
    viewChild.required<ElementRef<HTMLInputElement>>('campoDeDescricao');

  private readonly diretivaDoFormulario = viewChild.required(FormGroupDirective);

  protected readonly formulario = this.construtorDeFormulario.nonNullable.group({
    descricao: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(150)]],
    valor: ['', [Validators.required, validarValorMonetario]],
    dataDoRecebimento: [new Date(), [Validators.required]],
    recorrente: [false],
  });

  /**
   * "Enter salva" também aqui. A exceção é painel aberto (calendário), em que Enter
   * escolhe a data e não deve lançar a receita.
   */
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
    const valor = interpretarMoedaBrasileira(valores.valor);

    if (valor === null) {
      return;
    }

    this.salvar.emit({
      descricao: valores.descricao.trim(),
      valor,
      dataDoRecebimento: dataPuraDeDate(valores.dataDoRecebimento),
      recorrente: valores.recorrente,
    });
  }

  /**
   * Limpa só o que muda de um lançamento para o outro. Data e a marca de recorrente
   * continuam como estavam.
   */
  prepararProximoLancamento(): void {
    const anteriores = this.formulario.getRawValue();

    // `resetForm` (e não `patchValue`) porque só ele zera o "submitted" da diretiva —
    // sem isso o Material pinta os campos recém-limpos de vermelho.
    this.diretivaDoFormulario().resetForm({
      ...anteriores,
      descricao: '',
      valor: '',
    });

    this.focarNaDescricao();
  }

  focarNaDescricao(): void {
    this.campoDeDescricao().nativeElement.focus();
  }
}

/** O backend exige valor maior que zero; o formulário rejeita antes de gastar uma ida ao servidor. */
function validarValorMonetario(controle: AbstractControl): ValidationErrors | null {
  const texto = controle.value as string;

  if (!texto?.trim()) {
    return null;
  }

  const valor = interpretarMoedaBrasileira(texto);

  if (valor === null) {
    return { valorIlegivel: true };
  }

  if (valor <= 0) {
    return { valorNaoPositivo: true };
  }

  return null;
}
