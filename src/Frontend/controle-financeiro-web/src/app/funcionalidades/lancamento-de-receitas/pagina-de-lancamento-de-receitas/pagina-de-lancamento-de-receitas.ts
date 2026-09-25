import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  inject,
  signal,
  viewChild,
} from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Competencia, competenciaPorExtenso } from '../../../compartilhado/modelos/competencia';
import { DadosDaReceita, Receita } from '../../../compartilhado/modelos/receita';
import { EstadoDaCompetencia } from '../../../compartilhado/servicos/estado-da-competencia';
import { ObservadorDeTamanhoDeTela } from '../../../compartilhado/servicos/observador-de-tamanho-de-tela';
import { FormatadorDeMoedaBrasileiraPipe } from '../../../nucleo/formatadores/formatador-de-moeda-brasileira.pipe';
import {
  DadosDaReplicacao,
  DialogoDeReplicacao,
} from '../dialogo-de-replicacao/dialogo-de-replicacao';
import { FormularioDeReceita } from '../formulario-de-receita/formulario-de-receita';
import {
  EdicaoDeReceita,
  ListaDeReceitasDoMes,
} from '../lista-de-receitas-do-mes/lista-de-receitas-do-mes';
import { ServicoDeReceitas } from '../servicos/servico-de-receitas';

@Component({
  selector: 'app-pagina-de-lancamento-de-receitas',
  imports: [
    MatButtonModule,
    MatIconModule,
    FormularioDeReceita,
    ListaDeReceitasDoMes,
    FormatadorDeMoedaBrasileiraPipe,
  ],
  templateUrl: './pagina-de-lancamento-de-receitas.html',
  styleUrl: './pagina-de-lancamento-de-receitas.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PaginaDeLancamentoDeReceitas {
  private readonly servicoDeReceitas = inject(ServicoDeReceitas);
  private readonly avisos = inject(MatSnackBar);
  private readonly dialogos = inject(MatDialog);

  protected readonly estadoDaCompetencia = inject(EstadoDaCompetencia);
  protected readonly tela = inject(ObservadorDeTamanhoDeTela);

  private readonly formulario = viewChild(FormularioDeReceita);
  private readonly lista = viewChild.required(ListaDeReceitasDoMes);

  protected readonly receitas = signal<readonly Receita[]>([]);
  protected readonly carregando = signal(false);
  protected readonly salvando = signal(false);

  /** Em tela estreita o formulário fica atrás do botão flutuante; no desktop, sempre à vista. */
  private readonly abertoEmTelaEstreita = signal(false);
  protected readonly formularioVisivel = computed(
    () => this.tela.ehTelaLarga() || this.abertoEmTelaEstreita(),
  );

  private readonly aguardandoFoco = signal(false);

  protected readonly totalDoMes = computed(() =>
    this.receitas().reduce((soma, receita) => soma + receita.valor, 0),
  );

  protected readonly quantidadeDeRecorrentes = computed(
    () => this.receitas().filter((receita) => receita.recorrente).length,
  );

  constructor() {
    effect(() => {
      const competencia = this.estadoDaCompetencia.comoTexto();
      this.recarregarReceitas(competencia);
    });

    // O formulário só existe no DOM depois que o template reage ao sinal.
    effect(() => {
      const formulario = this.formulario();

      if (formulario && this.aguardandoFoco()) {
        this.aguardandoFoco.set(false);
        formulario.focarNaDescricao();
      }
    });
  }

  protected abrirFormulario(): void {
    this.abertoEmTelaEstreita.set(true);
    this.aguardandoFoco.set(true);
  }

  protected fecharFormulario(): void {
    this.abertoEmTelaEstreita.set(false);
  }

  protected registrar(dados: DadosDaReceita): void {
    this.salvando.set(true);

    this.servicoDeReceitas.registrar(dados).subscribe({
      next: (receita) => {
        this.salvando.set(false);
        this.absorverLancamento(receita);
        this.avisos.open(`"${receita.descricao}" lançada.`, undefined, { duration: 2500 });

        if (!receita.recorrente) {
          this.formulario()?.prepararProximoLancamento();
          return;
        }

        // É aqui que a pessoa acabou de dizer que o lançamento se repete todo mês.
        // O formulário só é limpo depois que o diálogo fecha: limpar antes colocaria o foco
        // na descrição, o diálogo roubaria esse foco, e o blur faria o Material acusar
        // "descrição é obrigatória" num campo que a pessoa nem chegou a tocar.
        this.oferecerReplicacao(receita, () => this.formulario()?.prepararProximoLancamento());
      },
      error: () => this.salvando.set(false),
    });
  }

  protected aplicarEdicao({ id, dados }: EdicaoDeReceita): void {
    this.servicoDeReceitas.editar(id, dados).subscribe({
      next: (receita) => {
        this.lista().encerrarEdicao();
        this.absorverEdicao(receita);
        this.avisos.open('Receita atualizada.', undefined, { duration: 2500 });
      },
    });
  }

  protected remover(receita: Receita): void {
    this.servicoDeReceitas.excluir(receita.id).subscribe({
      next: () => {
        this.receitas.update((atuais) => atuais.filter((item) => item.id !== receita.id));
        this.avisos.open(`"${receita.descricao}" excluída.`, undefined, { duration: 2500 });
      },
    });
  }

  /** @param aoFechar roda depois do diálogo, tenha a replicação sido aceita ou recusada. */
  protected oferecerReplicacao(receita: Receita, aoFechar?: () => void): void {
    const dados: DadosDaReplicacao = {
      descricao: receita.descricao,
      competenciaDeOrigem: competenciaPorExtenso(competenciaDaData(receita.dataDoRecebimento)),
    };

    this.dialogos
      .open<DialogoDeReplicacao, DadosDaReplicacao, number>(DialogoDeReplicacao, {
        data: dados,
        width: 'min(28rem, calc(100vw - 2rem))',
        autoFocus: 'dialog',
      })
      .afterClosed()
      .subscribe((meses) => {
        if (meses) {
          this.replicar(receita, meses);
        }

        aoFechar?.();
      });
  }

  private replicar(receita: Receita, meses: number): void {
    this.servicoDeReceitas.replicarNosMesesSeguintes(receita.id, meses).subscribe({
      next: (resultado) => {
        this.avisos.open(descreverReplicacao(resultado), 'Fechar', { duration: 6000 });
      },
    });
  }

  private recarregarReceitas(competencia: string): void {
    this.carregando.set(true);

    this.servicoDeReceitas.listarDoMes(competencia).subscribe({
      next: (receitas) => {
        this.receitas.set(receitas);
        this.carregando.set(false);
      },
      error: () => {
        this.receitas.set([]);
        this.carregando.set(false);
      },
    });
  }

  private absorverLancamento(receita: Receita): void {
    if (!receita.dataDoRecebimento.startsWith(this.estadoDaCompetencia.comoTexto())) {
      this.avisos.open(
        `Lançada em ${receita.dataDoRecebimento}, fora do mês que está sendo exibido.`,
        'Fechar',
        { duration: 5000 },
      );
      return;
    }

    this.receitas.update((atuais) => [...atuais, receita].sort(compararPelaOrdemDaApi));
  }

  private absorverEdicao(receita: Receita): void {
    const continuaNoMes = receita.dataDoRecebimento.startsWith(this.estadoDaCompetencia.comoTexto());

    this.receitas.update((atuais) => {
      const semAntiga = atuais.filter((item) => item.id !== receita.id);

      return continuaNoMes ? [...semAntiga, receita].sort(compararPelaOrdemDaApi) : semAntiga;
    });
  }
}

/** Mesma ordenação do `order by data_do_recebimento desc, criado_em desc` da API. */
function compararPelaOrdemDaApi(uma: Receita, outra: Receita): number {
  const porData = outra.dataDoRecebimento.localeCompare(uma.dataDoRecebimento);

  return porData !== 0 ? porData : outra.criadoEm.localeCompare(uma.criadoEm);
}

/** Extrai ano e mês de uma data pura `aaaa-MM-dd` sem passar por `Date`. */
function competenciaDaData(dataPura: string): Competencia {
  const [ano, mes] = dataPura.split('-').map(Number);
  return { ano, mes };
}

function descreverReplicacao(resultado: {
  competenciasCriadas: readonly string[];
  competenciasIgnoradas: readonly string[];
}): string {
  const criadas = resultado.competenciasCriadas.length;
  const ignoradas = resultado.competenciasIgnoradas.length;

  if (criadas === 0) {
    return 'Nenhum mês novo: todos já tinham um lançamento com essa descrição.';
  }

  const inicio = criadas === 1 ? 'Replicada em 1 mês' : `Replicada em ${criadas} meses`;

  return ignoradas === 0
    ? `${inicio}.`
    : `${inicio}. ${ignoradas} já tinham esse lançamento e foram pulados.`;
}
