import { ChangeDetectionStrategy, Component } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { ITENS_DE_NAVEGACAO } from '../itens-de-navegacao';

/** Trilha de navegação fixa do desktop. No celular quem assume é a navegação inferior. */
@Component({
  selector: 'app-menu-lateral',
  imports: [MatIconModule, MatTooltipModule, RouterLink, RouterLinkActive],
  templateUrl: './menu-lateral.html',
  styleUrl: './menu-lateral.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MenuLateral {
  protected readonly itens = ITENS_DE_NAVEGACAO;
}
