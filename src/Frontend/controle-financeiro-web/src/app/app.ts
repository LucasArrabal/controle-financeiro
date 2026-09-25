import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ObservadorDeTamanhoDeTela } from './compartilhado/servicos/observador-de-tamanho-de-tela';
import { BarraSuperior } from './layout/barra-superior/barra-superior';
import { MenuLateral } from './layout/menu-lateral/menu-lateral';
import { NavegacaoInferior } from './layout/navegacao-inferior/navegacao-inferior';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, BarraSuperior, MenuLateral, NavegacaoInferior],
  templateUrl: './app.html',
  styleUrl: './app.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class App {
  protected readonly tela = inject(ObservadorDeTamanhoDeTela);
}
