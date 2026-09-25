import { Routes } from '@angular/router';
import { provideCharts, withDefaultRegisterables } from 'ng2-charts';

/**
 * Rotas do painel num arquivo separado de propósito: é o que mantém o Chart.js fora do
 * pacote inicial. Registrar o `provideCharts` direto no `app.routes.ts` funcionaria, mas o
 * `import` estático de lá já arrastaria a biblioteca para quem só abre a tela de despesas.
 */
export const rotasDoPainel: Routes = [
  {
    path: '',
    title: 'Painel do mês · Controle Financeiro',
    providers: [provideCharts(withDefaultRegisterables())],
    loadComponent: () =>
      import('./pagina-do-painel/pagina-do-painel').then((modulo) => modulo.PaginaDoPainel),
  },
];
