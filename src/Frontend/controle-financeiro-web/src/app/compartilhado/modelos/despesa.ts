/**
 * Despesa como a API devolve. `dataDoGasto` é data pura em `aaaa-MM-dd` — não é instante,
 * não tem fuso e nunca deve passar por `new Date(texto)` sem cuidado, sob pena de voltar um dia.
 */
export interface Despesa {
  readonly id: string;
  readonly descricao: string;
  readonly valor: number;
  readonly dataDoGasto: string;
  readonly categoriaId: string;
  readonly nomeDaCategoria: string;
  readonly corDaCategoria: string;
  readonly formaDePagamento: string | null;
  readonly observacao: string | null;
  readonly criadoEm: string;
}

/** Corpo dos POST e PUT de despesa. */
export interface DadosDaDespesa {
  readonly descricao: string;
  readonly valor: number;
  readonly dataDoGasto: string;
  readonly categoriaId: string;
  readonly formaDePagamento: string | null;
  readonly observacao: string | null;
}

export const FORMAS_DE_PAGAMENTO: readonly string[] = [
  'Pix',
  'Débito',
  'Crédito',
  'Dinheiro',
  'Boleto',
  'Transferência',
];
