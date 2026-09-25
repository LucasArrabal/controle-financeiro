import { ambiente } from '../../../environments/environment';

/**
 * Único lugar que monta URL da API. Nenhum serviço concatena caminho por conta própria,
 * então trocar a versão do prefixo (`/api/v1`) é mexer só aqui.
 */
export const ConfiguracaoDaApi = {
  urlBase: ambiente.urlDaApi,

  categorias: (): string => `${ambiente.urlDaApi}/categorias`,
  categoria: (id: string): string => `${ambiente.urlDaApi}/categorias/${id}`,

  despesas: (): string => `${ambiente.urlDaApi}/despesas`,
  despesa: (id: string): string => `${ambiente.urlDaApi}/despesas/${id}`,

  receitas: (): string => `${ambiente.urlDaApi}/receitas`,
  receita: (id: string): string => `${ambiente.urlDaApi}/receitas/${id}`,
  replicacaoDeReceita: (id: string): string => `${ambiente.urlDaApi}/receitas/${id}/replicar`,

  tetosDeGasto: (): string => `${ambiente.urlDaApi}/tetos-de-gasto`,
  copiaDeTetosDoMesAnterior: (): string =>
    `${ambiente.urlDaApi}/tetos-de-gasto/copiar-do-mes-anterior`,

  resumoMensal: (): string => `${ambiente.urlDaApi}/painel/resumo-mensal`,
} as const;
