/**
 * Uma linha da tela de tetos. `valorLimite` nulo é "sem teto definido";
 * zero é um teto de verdade e significa "nenhum gasto permitido".
 */
export interface TetoDeGasto {
  readonly categoriaId: string;
  readonly nomeDaCategoria: string;
  readonly corHexadecimal: string;
  readonly valorLimite: number | null;
}

/** Corpo do PUT em lote: o estado da tela inteira num mês. */
export interface DadosDosTetosDoMes {
  readonly competencia: string;
  readonly tetos: readonly LimiteDeCategoria[];
}

export interface LimiteDeCategoria {
  readonly categoriaId: string;
  readonly valorLimite: number | null;
}
