import { BreakpointObserver } from '@angular/cdk/layout';
import { computed, inject, Injectable } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { map } from 'rxjs';

/** Abaixo disso não cabe trilha lateral: a navegação vai para a base da tela. */
const LARGURA_MAXIMA_DE_TELA_ESTREITA = 899;

/** A partir daqui a tabela de seis colunas cabe sem rolagem horizontal. */
const LARGURA_MINIMA_PARA_TABELA = 1200;

/**
 * Um único ponto de verdade sobre o tamanho da tela, para o layout não depender de
 * `@media` espalhado nem de cada componente medir a janela por conta própria.
 *
 * São dois limites diferentes de propósito: onde a navegação muda de lugar não é onde
 * a tabela deixa de caber.
 */
@Injectable({ providedIn: 'root' })
export class ObservadorDeTamanhoDeTela {
  private readonly observador = inject(BreakpointObserver);

  /** Layout de celular: navegação inferior, botão flutuante, formulário recolhido. */
  readonly ehTelaEstreita = this.acompanhar(
    `(max-width: ${LARGURA_MAXIMA_DE_TELA_ESTREITA}px)`,
    (largura) => largura <= LARGURA_MAXIMA_DE_TELA_ESTREITA,
  );

  readonly ehTelaLarga = computed(() => !this.ehTelaEstreita());

  /** Quando é falso, os lançamentos aparecem como lista em vez de tabela. */
  readonly comportaTabela = this.acompanhar(
    `(min-width: ${LARGURA_MINIMA_PARA_TABELA}px)`,
    (largura) => largura >= LARGURA_MINIMA_PARA_TABELA,
  );

  private acompanhar(consulta: string, valorInicial: (largura: number) => boolean) {
    return toSignal(
      this.observador.observe(consulta).pipe(map((resultado) => resultado.matches)),
      {
        initialValue:
          typeof window !== 'undefined' ? valorInicial(window.innerWidth) : false,
      },
    );
  }
}
