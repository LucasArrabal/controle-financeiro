/** Situação de um teto, espelhando o enum `SituacaoDoTeto` do domínio. */
export type SituacaoDoTeto = 'DentroDoLimite' | 'EmAtencao' | 'Estourado';

/**
 * Uma fatia do gráfico e uma linha da lista de tetos. `tetoDefinido` nulo significa
 * categoria sem limite no mês: aparece na lista, mas sem barra de progresso.
 */
export interface GastoPorCategoria {
  readonly categoriaId: string;
  readonly nomeDaCategoria: string;
  readonly corHexadecimal: string;
  readonly valorGasto: number;
  readonly percentualDoTotalGasto: number;
  readonly tetoDefinido: number | null;
  readonly percentualDoTetoConsumido: number | null;
  readonly situacaoDoTeto: SituacaoDoTeto | null;
}

/** Payload do painel, vindo de uma única consulta agregada no banco. */
export interface ResumoMensal {
  readonly competencia: string;
  readonly totalDeReceitas: number;
  readonly totalDeDespesas: number;
  readonly saldo: number;
  readonly percentualDaRendaComprometida: number;
  readonly gastosPorCategoria: readonly GastoPorCategoria[];
}

export const RESUMO_MENSAL_VAZIO: ResumoMensal = {
  competencia: '',
  totalDeReceitas: 0,
  totalDeDespesas: 0,
  saldo: 0,
  percentualDaRendaComprometida: 0,
  gastosPorCategoria: [],
};
