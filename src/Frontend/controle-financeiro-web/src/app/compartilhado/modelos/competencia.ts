/**
 * Mês de referência dos lançamentos. Espelha o `CompetenciaMensal` do backend e
 * viaja pela API sempre como texto no formato `aaaa-MM`.
 */
export interface Competencia {
  readonly ano: number;
  readonly mes: number;
}

const NOMES_DOS_MESES = [
  'janeiro',
  'fevereiro',
  'março',
  'abril',
  'maio',
  'junho',
  'julho',
  'agosto',
  'setembro',
  'outubro',
  'novembro',
  'dezembro',
] as const;

export function competenciaDeData(data: Date): Competencia {
  return { ano: data.getFullYear(), mes: data.getMonth() + 1 };
}

export function competenciaDeHoje(): Competencia {
  return competenciaDeData(new Date());
}

/** Texto que a API espera em `?competencia=`, ex.: `2026-09`. */
export function formatarCompetencia({ ano, mes }: Competencia): string {
  return `${String(ano).padStart(4, '0')}-${String(mes).padStart(2, '0')}`;
}

/** Texto do seletor, ex.: `setembro de 2026`. */
export function competenciaPorExtenso({ ano, mes }: Competencia): string {
  return `${NOMES_DOS_MESES[mes - 1]} de ${ano}`;
}

export function somarMeses({ ano, mes }: Competencia, meses: number): Competencia {
  const referencia = new Date(ano, mes - 1 + meses, 1);
  return competenciaDeData(referencia);
}

export function competenciasIguais(uma: Competencia, outra: Competencia): boolean {
  return uma.ano === outra.ano && uma.mes === outra.mes;
}

export { NOMES_DOS_MESES };
