// Troca o marcador `__URL_DA_API__` do bundle de produção pelo endereço real da API,
// lido da variável de ambiente URL_DA_API.
//
// A substituição acontece DEPOIS do build, no `dist/`, de propósito: assim
// `environment.production.ts` continua versionado com o marcador e nenhuma URL de ambiente
// entra no repositório. É o mesmo motivo de a string de conexão do backend vir só de
// variável de ambiente.

import { readdir, readFile, writeFile } from 'node:fs/promises';
import { join } from 'node:path';

const MARCADOR = '__URL_DA_API__';
const PASTA_DO_BUNDLE = 'dist/controle-financeiro-web/browser';

const urlDaApi = process.env.URL_DA_API?.trim();

if (!urlDaApi) {
  console.error(
    'URL_DA_API não foi definida.\n' +
      'Exemplo: URL_DA_API=https://controle-financeiro-api.onrender.com/api/v1 npm run build:producao',
  );
  process.exit(1);
}

if (!/^https?:\/\//.test(urlDaApi)) {
  console.error(`URL_DA_API deve começar com http:// ou https://; recebi "${urlDaApi}".`);
  process.exit(1);
}

if (urlDaApi.endsWith('/')) {
  console.error('URL_DA_API não deve terminar com barra: as rotas já começam com uma.');
  process.exit(1);
}

const arquivos = await readdir(PASTA_DO_BUNDLE, { recursive: true }).catch(() => {
  console.error(`Pasta ${PASTA_DO_BUNDLE} não existe. Rode o build antes.`);
  process.exit(1);
});

let arquivosAlterados = 0;

for (const nome of arquivos) {
  if (!nome.endsWith('.js')) {
    continue;
  }

  const caminho = join(PASTA_DO_BUNDLE, nome);
  const conteudo = await readFile(caminho, 'utf8');

  if (!conteudo.includes(MARCADOR)) {
    continue;
  }

  await writeFile(caminho, conteudo.replaceAll(MARCADOR, urlDaApi), 'utf8');
  arquivosAlterados++;
}

if (arquivosAlterados === 0) {
  // Falhar alto: um bundle publicado com o marcador intacto quebraria toda chamada à API.
  console.error(
    `Nenhum arquivo continha ${MARCADOR}. O build usou environment.production.ts? ` +
      'Verifique o fileReplacements da configuração de produção no angular.json.',
  );
  process.exit(1);
}

console.log(`URL da API aplicada em ${arquivosAlterados} arquivo(s): ${urlDaApi}`);
