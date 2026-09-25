import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule, MatMenuTrigger } from '@angular/material/menu';
import { MatTooltipModule } from '@angular/material/tooltip';
import { NOMES_DOS_MESES } from '../../modelos/competencia';
import { EstadoDaCompetencia } from '../../servicos/estado-da-competencia';

/**
 * Setas para andar mês a mês e um menu com grade de meses para saltar direto.
 * Não usa datepicker de propósito: competência é mês e ano, dia não existe aqui.
 */
@Component({
  selector: 'app-seletor-de-competencia',
  imports: [MatButtonModule, MatIconModule, MatMenuModule, MatTooltipModule],
  templateUrl: './seletor-de-competencia.html',
  styleUrl: './seletor-de-competencia.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SeletorDeCompetencia {
  protected readonly estado = inject(EstadoDaCompetencia);

  protected readonly meses = NOMES_DOS_MESES.map((nome, indice) => ({
    numero: indice + 1,
    abreviacao: nome.slice(0, 3),
    nome,
  }));

  /** Ano que a grade do menu está mostrando, independente do ano selecionado. */
  protected readonly anoEmFoco = signal(this.estado.competencia().ano);

  protected aoAbrirMenu(): void {
    this.anoEmFoco.set(this.estado.competencia().ano);
  }

  protected recuarAno(evento: MouseEvent): void {
    evento.stopPropagation();
    this.anoEmFoco.update((ano) => ano - 1);
  }

  protected avancarAno(evento: MouseEvent): void {
    evento.stopPropagation();
    this.anoEmFoco.update((ano) => ano + 1);
  }

  protected selecionarMes(mes: number, gatilho: MatMenuTrigger): void {
    this.estado.definir({ ano: this.anoEmFoco(), mes });
    gatilho.closeMenu();
  }

  protected ehMesSelecionado(mes: number): boolean {
    const competencia = this.estado.competencia();
    return competencia.mes === mes && competencia.ano === this.anoEmFoco();
  }
}
