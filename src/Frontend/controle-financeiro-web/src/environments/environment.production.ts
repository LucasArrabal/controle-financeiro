/**
 * Ambiente de produção. A URL da API é injetada no momento do build:
 * o script de deploy troca o marcador `__URL_DA_API__` pelo endereço real
 * (Render ou Cloud Run), lido de variável de ambiente.
 */
export const ambiente = {
  producao: true,
  urlDaApi: '__URL_DA_API__',
};
