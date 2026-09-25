/**
 * Converte entre a `Date` que o datepicker do Material usa e o texto `aaaa-MM-dd` da API.
 *
 * Passa longe de `toISOString()` e de `new Date('2026-09-21')`: os dois interpretam a data
 * em UTC e, de madrugada no fuso de São Paulo, devolvem o dia anterior. Data de gasto é
 * data pura — o dia em que o dinheiro saiu, sem fuso nenhum.
 */
export function dataPuraDeDate(data: Date): string {
  const ano = String(data.getFullYear()).padStart(4, '0');
  const mes = String(data.getMonth() + 1).padStart(2, '0');
  const dia = String(data.getDate()).padStart(2, '0');

  return `${ano}-${mes}-${dia}`;
}

export function dateDeDataPura(texto: string): Date {
  const [ano, mes, dia] = texto.split('-').map(Number);

  return new Date(ano, mes - 1, dia);
}

/** Exibição curta na tabela, ex.: `21/09`. */
export function dataPuraPorExtensoCurto(texto: string): string {
  const [, mes, dia] = texto.split('-');

  return `${dia}/${mes}`;
}
