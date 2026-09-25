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
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatButtonModule } from '@angular/material/button';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { Categoria } from '../../../compartilhado/modelos/categoria';
import { DadosDaDespesa, FORMAS_DE_PAGAMENTO } from '../../../compartilhado/modelos/despesa';
import { dataPuraDeDate } from '../../../nucleo/formatadores/conversor-de-data-pura';
import { interpretarMoedaBrasileira } from '../../../nucleo/formatadores/interpretador-de-moeda-brasileira';

/**
 * Formulário curto de lançamento. Pensado para digitar vários gastos seguidos sem tocar no
 * mouse: Enter salva e o foco volta sozinho para a descrição, com a data preservada.
 */
@Component({
  selector: 'app-formulario-de-despesa',
  imports: [
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatAutocompleteModule,
    MatDatepickerModule,
    MatButtonModule,
    MatIconModule,
  ],
  templateUrl: './formulario-de-despesa.html',
  styleUrl: './formulario-de-despesa.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FormularioDeDespesa {
  private readonly construtorDeFormulario = inject(FormBuilder);

  readonly categorias = input.required<readonly Categoria[]>();
  readonly salvando = input(false);

  readonly salvar = output<DadosDaDespesa>();

  protected readonly formasDePagamento = FORMAS_DE_PAGAMENTO;

  private readonly campoDeDescricao =
    viewChild.required<ElementRef<HTMLInputElement>>('campoDeDescricao');

  private readonly diretivaDoFormulario = viewChild.required(FormGroupDirective);

  protected readonly formulario = this.construtorDeFormulario.nonNullable.group({
    descricao: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(150)]],
    valor: ['', [Validators.required, validarValorMonetario]],
    dataDoGasto: [new Date(), [Validators.required]],
    categoriaId: ['', [Validators.required]],
    formaDePagamento: [''],
    observacao: [''],
  });

  /** Data máxima aceita, espelhando a regra de "no máximo 1 ano no futuro" do backend. */
  protected readonly dataMaxima = (() => {
    const limite = new Date();
    limite.setFullYear(limite.getFullYear() + 1);
    return limite;
  })();

  /**
   * "Enter salva" é requisito da tela, então a regra fica explícita em vez de depender só do
   * submit implícito do HTML — que vale para os campos de texto, mas não para os controles do
   * Material que tratam a tecla por conta própria. A exceção é painel aberto (select, sugestão
   * de pagamento, calendário): ali Enter escolhe a opção e não deve lançar a despesa.
   */
  protected aoPressionarEnter(evento: Event): void {
    const alvo = evento.target as HTMLElement | null;

    if (alvo?.getAttribute('aria-expanded') === 'true') {
      return;
    }

    // Cancela o submit nativo para a despesa não ser lançada duas vezes.
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
      dataDoGasto: dataPuraDeDate(valores.dataDoGasto),
      categoriaId: valores.categoriaId,
      formaDePagamento: valores.formaDePagamento.trim() || null,
      observacao: valores.observacao.trim() || null,
    });
  }

  /**
   * Limpa só o que muda de um gasto para o outro. Data e categoria continuam como estavam,
   * porque quem lança em série costuma lançar vários do mesmo dia e da mesma categoria.
   */
  prepararProximoLancamento(): void {
    const anteriores = this.formulario.getRawValue();

    // `resetForm` (e não `patchValue`) porque só ele zera o "submitted" da diretiva —
    // sem isso o Material pinta os campos recém-limpos de vermelho.
    this.diretivaDoFormulario().resetForm({
      ...anteriores,
      descricao: '',
      valor: '',
      observacao: '',
    });

    this.focarNaDescricao();
  }

  /** Usado quando o formulário é aberto pelo botão flutuante no celular. */
  focarNaDescricao(): void {
    this.campoDeDescricao().nativeElement.focus();
  }

  protected corDaCategoria(id: string): string {
    return this.categorias().find((categoria) => categoria.id === id)?.corHexadecimal ?? 'transparent';
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
