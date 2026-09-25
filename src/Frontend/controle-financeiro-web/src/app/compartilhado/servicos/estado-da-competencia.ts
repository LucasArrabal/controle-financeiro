import { computed, Injectable, signal } from '@angular/core';
import {
  Competencia,
  competenciaDeHoje,
  competenciaPorExtenso,
  competenciasIguais,
  formatarCompetencia,
  somarMeses,
} from '../modelos/competencia';

/**
 * A competência escolhida no topo da tela. Vive num serviço de raiz porque é compartilhada
 * entre despesas, receitas, tetos e painel: trocar o mês numa tela vale para todas.
 */
@Injectable({ providedIn: 'root' })
export class EstadoDaCompetencia {
  private readonly competenciaAtual = signal<Competencia>(competenciaDeHoje());

  readonly competencia = this.competenciaAtual.asReadonly();

  /** Formato que a API espera em `?competencia=`. */
  readonly comoTexto = computed(() => formatarCompetencia(this.competenciaAtual()));

  /** Formato que aparece na tela, ex.: `setembro de 2026`. */
  readonly porExtenso = computed(() => competenciaPorExtenso(this.competenciaAtual()));

  readonly ehMesCorrente = computed(() =>
    competenciasIguais(this.competenciaAtual(), competenciaDeHoje()),
  );

  definir(competencia: Competencia): void {
    this.competenciaAtual.set(competencia);
  }

  irParaMesAnterior(): void {
    this.competenciaAtual.update((atual) => somarMeses(atual, -1));
  }

  irParaProximoMes(): void {
    this.competenciaAtual.update((atual) => somarMeses(atual, 1));
  }

  irParaMesCorrente(): void {
    this.competenciaAtual.set(competenciaDeHoje());
  }
}
