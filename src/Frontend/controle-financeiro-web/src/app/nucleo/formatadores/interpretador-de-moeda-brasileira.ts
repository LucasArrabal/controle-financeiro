/**
 * Lê o que a pessoa digitou no campo de valor e devolve um número, aceitando
 * tanto `1.234,56` quanto `1234.56`, com ou sem `R$`.
 *
 * O caso ambíguo é um ponto sozinho: `1.500` é mil e quinhentos (separador de milhar),
 * mas `1.5` é um e cinquenta. A regra é o tamanho do último grupo — três dígitos com
 * algo antes é milhar; qualquer outra coisa é decimal.
 */
export function interpretarMoedaBrasileira(texto: string | null | undefined): number | null {
  if (texto === null || texto === undefined) {
    return null;
  }

  const limpo = String(texto)
    .replace(/\s/g, '')
    .replace(/^R\$/i, '')
    .trim();

  if (limpo === '') {
    return null;
  }

  if (!/^-?[\d.,]+$/.test(limpo)) {
    return null;
  }

  const temVirgula = limpo.includes(',');
  const temPonto = limpo.includes('.');

  let normalizado: string;

  if (temVirgula && temPonto) {
    // `1.234,56` — ponto é milhar, vírgula é decimal.
    normalizado = limpo.replace(/\./g, '').replace(',', '.');
  } else if (temVirgula) {
    normalizado = limpo.replace(',', '.');
  } else if (temPonto) {
    const grupos = limpo.split('.');
    const ultimoGrupo = grupos[grupos.length - 1];
    const ehSeparadorDeMilhar = grupos.length > 1 && ultimoGrupo.length === 3 && grupos[0] !== '';
    normalizado = ehSeparadorDeMilhar ? grupos.join('') : limpo;
  } else {
    normalizado = limpo;
  }

  const numero = Number(normalizado);

  return Number.isFinite(numero) ? Math.round(numero * 100) / 100 : null;
}
