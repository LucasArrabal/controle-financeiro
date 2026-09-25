export type TipoDeCategoria = 'GastoFixo' | 'Mercado' | 'Essencial' | 'Lazer';

export const TIPOS_DE_CATEGORIA: ReadonlyArray<{ valor: TipoDeCategoria; rotulo: string }> = [
  { valor: 'GastoFixo', rotulo: 'Gasto fixo' },
  { valor: 'Mercado', rotulo: 'Mercado' },
  { valor: 'Essencial', rotulo: 'Essencial' },
  { valor: 'Lazer', rotulo: 'Lazer' },
];

/** Categoria como a API devolve. */
export interface Categoria {
  readonly id: string;
  readonly nome: string;
  readonly tipo: TipoDeCategoria;
  readonly corHexadecimal: string;
  readonly ativa: boolean;
}

/** Corpo dos POST e PUT de categoria. */
export interface DadosDaCategoria {
  readonly nome: string;
  readonly tipo: TipoDeCategoria;
  readonly corHexadecimal: string;
  readonly ativa?: boolean;
}
