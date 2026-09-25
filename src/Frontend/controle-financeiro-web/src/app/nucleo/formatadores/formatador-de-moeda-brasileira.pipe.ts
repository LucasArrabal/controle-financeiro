import { Pipe, PipeTransform } from '@angular/core';

/** Exibe um número como `R$ 1.234,56`. */
@Pipe({ name: 'moedaBrasileira' })
export class FormatadorDeMoedaBrasileiraPipe implements PipeTransform {
  private static readonly formatador = new Intl.NumberFormat('pt-BR', {
    style: 'currency',
    currency: 'BRL',
  });

  transform(valor: number | null | undefined): string {
    return FormatadorDeMoedaBrasileiraPipe.formatador.format(valor ?? 0);
  }
}
