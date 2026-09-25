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
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Categoria, DadosDaCategoria } from '../../../compartilhado/modelos/categoria';
import { ObservadorDeTamanhoDeTela } from '../../../compartilhado/servicos/observador-de-tamanho-de-tela';
import { ServicoDeCategorias } from '../../../compartilhado/servicos/servico-de-categorias';
import { FormularioDeCategoria } from '../formulario-de-categoria/formulario-de-categoria';
import { EdicaoDeCategoria, ListaDeCategorias } from '../lista-de-categorias/lista-de-categorias';

@Component({
  selector: 'app-pagina-de-categorias',
  imports: [MatButtonModule, MatIconModule, FormularioDeCategoria, ListaDeCategorias],
  templateUrl: './pagina-de-categorias.html',
  styleUrl: './pagina-de-categorias.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PaginaDeCategorias {
  private readonly avisos = inject(MatSnackBar);

  protected readonly servicoDeCategorias = inject(ServicoDeCategorias);
  protected readonly tela = inject(ObservadorDeTamanhoDeTela);

  private readonly formulario = viewChild(FormularioDeCategoria);
  private readonly lista = viewChild.required(ListaDeCategorias);

  protected readonly salvando = signal(false);

  /** Em tela estreita o formulário fica atrás do botão flutuante; no desktop, sempre à vista. */
  private readonly abertoEmTelaEstreita = signal(false);
  protected readonly formularioVisivel = computed(
    () => this.tela.ehTelaLarga() || this.abertoEmTelaEstreita(),
  );

  private readonly aguardandoFoco = signal(false);

  /** Ativas primeiro, cada grupo em ordem alfabética — assim quem está usando vê primeiro. */
  protected readonly categoriasOrdenadas = computed(() =>
    [...this.servicoDeCategorias.categorias()].sort((uma, outra) => {
      if (uma.ativa !== outra.ativa) {
        return uma.ativa ? -1 : 1;
      }
      return uma.nome.localeCompare(outra.nome, 'pt-BR');
    }),
  );

  protected readonly quantidadeDeAtivas = computed(
    () => this.servicoDeCategorias.categorias().filter((categoria) => categoria.ativa).length,
  );

  protected readonly quantidadeDeInativas = computed(
    () => this.servicoDeCategorias.categorias().length - this.quantidadeDeAtivas(),
  );

  constructor() {
    this.servicoDeCategorias.recarregar();

    // O formulário só existe no DOM depois que o template reage ao sinal.
    effect(() => {
      const formulario = this.formulario();

      if (formulario && this.aguardandoFoco()) {
        this.aguardandoFoco.set(false);
        formulario.prepararProximaCategoria();
      }
    });
  }

  protected abrirFormulario(): void {
    this.abertoEmTelaEstreita.set(true);
  }

  protected fecharFormulario(): void {
    this.abertoEmTelaEstreita.set(false);
  }

  protected criar(dados: DadosDaCategoria): void {
    this.salvando.set(true);

    this.servicoDeCategorias.criar(dados).subscribe({
      next: (categoria) => {
        this.salvando.set(false);
        this.aguardandoFoco.set(true);
        this.avisos.open(`"${categoria.nome}" criada.`, undefined, { duration: 2500 });
      },
      // O interceptador já mostrou o erro (nome duplicado, cor inválida etc.).
      error: () => this.salvando.set(false),
    });
  }

  protected aplicarEdicao({ id, dados }: EdicaoDeCategoria): void {
    this.servicoDeCategorias.editar(id, dados).subscribe({
      next: (categoria) => {
        this.lista().encerrarEdicao();
        this.avisos.open(`"${categoria.nome}" atualizada.`, undefined, { duration: 2500 });
      },
    });
  }

  protected desativar(categoria: Categoria): void {
    this.servicoDeCategorias.desativar(categoria.id).subscribe({
      next: () =>
        this.avisos.open(`"${categoria.nome}" desativada.`, undefined, { duration: 2500 }),
    });
  }

  protected reativar(categoria: Categoria): void {
    this.servicoDeCategorias
      .editar(categoria.id, {
        nome: categoria.nome,
        tipo: categoria.tipo,
        corHexadecimal: categoria.corHexadecimal,
        ativa: true,
      })
      .subscribe({
        next: () =>
          this.avisos.open(`"${categoria.nome}" reativada.`, undefined, { duration: 2500 }),
      });
  }
}
