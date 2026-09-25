/**
 * Receita como a API devolve. `dataDoRecebimento` é data pura em `aaaa-MM-dd`,
 * pelo mesmo motivo da despesa: é o dia em que o dinheiro entrou, sem fuso.
 */
export interface Receita {
  readonly id: string;
  readonly descricao: string;
  readonly valor: number;
  readonly dataDoRecebimento: string;
  readonly recorrente: boolean;
  readonly criadoEm: string;
}

/** Corpo dos POST e PUT de receita. */
export interface DadosDaReceita {
  readonly descricao: string;
  readonly valor: number;
  readonly dataDoRecebimento: string;
  readonly recorrente: boolean;
}

/** Resultado de replicar uma receita recorrente nos meses seguintes. */
export interface ResultadoDaReplicacao {
  readonly competenciasCriadas: readonly string[];
  readonly competenciasIgnoradas: readonly string[];
}
