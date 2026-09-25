import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { SeletorDeCompetencia } from '../../compartilhado/componentes/seletor-de-competencia/seletor-de-competencia';
import { ObservadorDeTamanhoDeTela } from '../../compartilhado/servicos/observador-de-tamanho-de-tela';

/**
 * Barra do topo, reduzida ao essencial: só o seletor de competência, que é o controle
 * compartilhado por todas as telas.
 */
@Component({
  selector: 'app-barra-superior',
  imports: [SeletorDeCompetencia],
  templateUrl: './barra-superior.html',
  styleUrl: './barra-superior.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BarraSuperior {
  protected readonly tela = inject(ObservadorDeTamanhoDeTela);
}
