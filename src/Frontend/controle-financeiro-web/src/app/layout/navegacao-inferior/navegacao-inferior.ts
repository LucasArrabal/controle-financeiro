import { ChangeDetectionStrategy, Component } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { ITENS_DE_NAVEGACAO } from '../itens-de-navegacao';

/**
 * Navegação do celular, fixa na base e ao alcance do polegar.
 * Substitui a gaveta lateral, que no telefone exige dois toques para qualquer troca de tela.
 */
@Component({
  selector: 'app-navegacao-inferior',
  imports: [MatIconModule, RouterLink, RouterLinkActive],
  templateUrl: './navegacao-inferior.html',
  styleUrl: './navegacao-inferior.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NavegacaoInferior {
  protected readonly itens = ITENS_DE_NAVEGACAO;
}
