/**
 * Destinos da aplicação, na ordem em que aparecem. Uma lista só, consumida pela trilha
 * lateral do desktop e pela navegação inferior do celular — assim as duas nunca divergem.
 */
export interface ItemDeNavegacao {
  readonly rotulo: string;
  /** Texto curto para a navegação inferior, onde não cabe o rótulo completo. */
  readonly rotuloCurto: string;
  readonly icone: string;
  readonly rota: string;
  /** Telas que ainda não existem aparecem apagadas, em vez de virarem link quebrado. */
  readonly disponivel: boolean;
}

export const ITENS_DE_NAVEGACAO: readonly ItemDeNavegacao[] = [
  {
    rotulo: 'Despesas',
    rotuloCurto: 'Despesas',
    icone: 'receipt_long',
    rota: '/despesas',
    disponivel: true,
  },
  {
    rotulo: 'Receitas',
    rotuloCurto: 'Receitas',
    icone: 'savings',
    rota: '/receitas',
    disponivel: true,
  },
  {
    rotulo: 'Categorias',
    rotuloCurto: 'Categorias',
    icone: 'category',
    rota: '/categorias',
    disponivel: true,
  },
  {
    rotulo: 'Tetos por categoria',
    rotuloCurto: 'Tetos',
    icone: 'speed',
    rota: '/tetos',
    disponivel: true,
  },
  {
    rotulo: 'Painel do mês',
    rotuloCurto: 'Painel',
    icone: 'donut_small',
    rota: '/painel',
    disponivel: true,
  },
];
