import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'despesas' },
  {
    path: 'despesas',
    title: 'Despesas · Controle Financeiro',
    loadComponent: () =>
      import(
        './funcionalidades/lancamento-de-despesas/pagina-de-lancamento-de-despesas/pagina-de-lancamento-de-despesas'
      ).then((modulo) => modulo.PaginaDeLancamentoDeDespesas),
  },
  {
    path: 'receitas',
    title: 'Receitas · Controle Financeiro',
    loadComponent: () =>
      import(
        './funcionalidades/lancamento-de-receitas/pagina-de-lancamento-de-receitas/pagina-de-lancamento-de-receitas'
      ).then((modulo) => modulo.PaginaDeLancamentoDeReceitas),
  },
  {
    path: 'tetos',
    title: 'Tetos por categoria · Controle Financeiro',
    loadComponent: () =>
      import(
        './funcionalidades/configuracao-de-tetos-por-categoria/pagina-de-configuracao-de-tetos/pagina-de-configuracao-de-tetos'
      ).then((modulo) => modulo.PaginaDeConfiguracaoDeTetos),
  },
  {
    path: 'painel',
    loadChildren: () =>
      import('./funcionalidades/painel-resumo-mensal/rotas-do-painel').then(
        (modulo) => modulo.rotasDoPainel,
      ),
  },
  { path: '**', redirectTo: 'despesas' },
];
