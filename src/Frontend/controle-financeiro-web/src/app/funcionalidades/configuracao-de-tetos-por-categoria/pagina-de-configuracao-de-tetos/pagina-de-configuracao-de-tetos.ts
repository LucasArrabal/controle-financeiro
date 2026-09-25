import { ChangeDetectionStrategy, Component, computed, effect, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormControl, FormRecord, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import {
  DadosDaConfirmacao,
  DialogoDeConfirmacao,
} from '../../../compartilhado/componentes/dialogo-de-confirmacao/dialogo-de-confirmacao';
import { TetoDeGasto } from '../../../compartilhado/modelos/teto-de-gasto';
import { EstadoDaCompetencia } from '../../../compartilhado/servicos/estado-da-competencia';
import { FormatadorDeMoedaBrasileiraPipe } from '../../../nucleo/formatadores/formatador-de-moeda-brasileira.pipe';
import { interpretarMoedaBrasileira } from '../../../nucleo/formatadores/interpretador-de-moeda-brasileira';
import { ServicoDeReceitas } from '../../lancamento-de-receitas/servicos/servico-de-receitas';
import { ServicoDeTetosDeGasto } from '../servicos/servico-de-tetos-de-gasto';

@Component({
  selector: 'app-pagina-de-configuracao-de-tetos',
  imports: [
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatTooltipModule,
    MatProgressBarModule,
    FormatadorDeMoedaBrasileiraPipe,
  ],
  templateUrl: './pagina-de-configuracao-de-tetos.html',
  styleUrl: './pagina-de-configuracao-de-tetos.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PaginaDeConfiguracaoDeTetos {
  private readonly servicoDeTetos = inject(ServicoDeTetosDeGasto);
  private readonly servicoDeReceitas = inject(ServicoDeReceitas);
  private readonly avisos = inject(MatSnackBar);
  private readonly dialogos = inject(MatDialog);

  protected readonly estadoDaCompetencia = inject(EstadoDaCompetencia);

  protected readonly linhas = signal<readonly TetoDeGasto[]>([]);
  protected readonly carregando = signal(false);
  protected readonly salvando = signal(false);
  protected readonly receitaPrevista = signal(0);

  /** Um controle de texto por categoria, indexado pelo id — o estado da tela inteira. */
  protected readonly formulario = new FormRecord<FormControl<string>>({});

  private readonly valoresDigitados = toSignal(this.formulario.valueChanges, {
    initialValue: {} as Record<string, string | undefined>,
  });

  protected readonly somaDosTetos = computed(() => {
    const valores = this.valoresDigitados();

    return Object.values(valores).reduce<number>(
      (soma, texto) => soma + (interpretarMoedaBrasileira(texto ?? '') ?? 0),
      0,
    );
  });

  /** O aviso que a tela existe para dar: os limites somados passam do que entra no mês. */
  protected readonly tetosPassamDaReceita = computed(
    () => this.receitaPrevista() > 0 && this.somaDosTetos() > this.receitaPrevista(),
  );

  protected readonly sobraPrevista = computed(() => this.receitaPrevista() - this.somaDosTetos());

  protected readonly quantidadeSemTeto = computed(() => {
    const valores = this.valoresDigitados();

    return this.linhas().filter((linha) => !(valores[linha.categoriaId] ?? '').trim()).length;
  });

  constructor() {
    effect(() => {
      const competencia = this.estadoDaCompetencia.comoTexto();
      this.recarregar(competencia);
    });
  }

  protected salvar(): void {
    const competencia = this.estadoDaCompetencia.comoTexto();
    const valores = this.formulario.getRawValue();

    const tetos = this.linhas().map((linha) => ({
      categoriaId: linha.categoriaId,
      valorLimite: interpretarMoedaBrasileira(valores[linha.categoriaId] ?? ''),
    }));

    const ilegivel = this.linhas().find((linha) => {
      const texto = (valores[linha.categoriaId] ?? '').trim();
      return texto !== '' && interpretarMoedaBrasileira(texto) === null;
    });

    if (ilegivel) {
      this.avisos.open(
        `O valor de "${ilegivel.nomeDaCategoria}" não foi reconhecido.`,
        'Fechar',
        { duration: 5000 },
      );
      return;
    }

    this.salvando.set(true);

    this.servicoDeTetos.definir({ competencia, tetos }).subscribe({
      next: (atualizados) => {
        this.salvando.set(false);
        this.aplicarLinhas(atualizados);
        this.avisos.open('Tetos gravados.', undefined, { duration: 2500 });
      },
      error: () => this.salvando.set(false),
    });
  }

  protected pedirCopiaDoMesAnterior(): void {
    const dados: DadosDaConfirmacao = {
      titulo: 'Copiar tetos do mês anterior',
      mensagem:
        `Os tetos de ${this.estadoDaCompetencia.porExtenso()} serão substituídos pelos do mês ` +
        'anterior. O que estiver digitado e ainda não gravado se perde.',
      rotuloDeConfirmacao: 'Copiar',
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
          this.copiarDoMesAnterior();
        }
      });
  }

  private copiarDoMesAnterior(): void {
    this.salvando.set(true);

    this.servicoDeTetos.copiarDoMesAnterior(this.estadoDaCompetencia.comoTexto()).subscribe({
      next: (atualizados) => {
        this.salvando.set(false);
        this.aplicarLinhas(atualizados);
        this.avisos.open('Tetos copiados do mês anterior.', undefined, { duration: 2500 });
      },
      error: () => this.salvando.set(false),
    });
  }

  private recarregar(competencia: string): void {
    this.carregando.set(true);

    this.servicoDeTetos.listarDoMes(competencia).subscribe({
      next: (tetos) => {
        this.aplicarLinhas(tetos);
        this.carregando.set(false);
      },
      error: () => {
        this.aplicarLinhas([]);
        this.carregando.set(false);
      },
    });

    // A receita prevista do mês é o que dá sentido ao aviso de soma dos tetos.
    this.servicoDeReceitas.listarDoMes(competencia).subscribe({
      next: (receitas) =>
        this.receitaPrevista.set(receitas.reduce((soma, receita) => soma + receita.valor, 0)),
      error: () => this.receitaPrevista.set(0),
    });
  }

  private aplicarLinhas(tetos: readonly TetoDeGasto[]): void {
    this.linhas.set(tetos);

    // Recria os controles do zero: categoria pode ter sido criada ou desativada entre os meses.
    for (const nome of Object.keys(this.formulario.controls)) {
      this.formulario.removeControl(nome);
    }

    for (const teto of tetos) {
      this.formulario.addControl(
        teto.categoriaId,
        new FormControl(textoDoLimite(teto.valorLimite), { nonNullable: true }),
      );
    }
  }
}

/** Zero é um teto de verdade e precisa aparecer como "0,00"; ausência de teto é campo vazio. */
function textoDoLimite(valorLimite: number | null): string {
  return valorLimite === null ? '' : valorLimite.toFixed(2).replace('.', ',');
}
