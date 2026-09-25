import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { catchError, throwError } from 'rxjs';
import { ambiente } from '../../../environments/environment';
import { ConfiguracaoDaApi } from '../configuracao/configuracao-da-api';

/**
 * Traduz o ProblemDetails que a API devolve numa mensagem curta e mostra num aviso.
 * O erro continua subindo: quem chamou ainda pode reagir (não limpar o formulário, por exemplo).
 */
export const interceptadorDeErrosHttp: HttpInterceptorFn = (requisicao, proximo) => {
  const avisos = inject(MatSnackBar);

  return proximo(requisicao).pipe(
    catchError((erro: HttpErrorResponse) => {
      avisos.open(descrever(erro), 'Fechar', {
        duration: 7000,
        horizontalPosition: 'center',
        verticalPosition: 'bottom',
        panelClass: 'aviso-de-erro',
      });

      return throwError(() => erro);
    }),
  );
};

interface ProblemDetails {
  readonly title?: string;
  readonly detail?: string;
  readonly errors?: Record<string, string[]>;
}

function descrever(erro: HttpErrorResponse): string {
  // Status 0 é rede. Em desenvolvimento a causa quase sempre é a API fora do ar ou o
  // certificado local não confiado; em produção essa dica não ajudaria ninguém.
  if (erro.status === 0) {
    return ambiente.producao
      ? 'Não foi possível falar com a API. Verifique sua conexão e tente de novo.'
      : `Não foi possível falar com a API em ${ConfiguracaoDaApi.urlBase}. ` +
        'Confira se ela está rodando e se o certificado de desenvolvimento está confiável ' +
        '(dotnet dev-certs https --trust).';
  }

  const problema = erro.error as ProblemDetails | null;

  if (problema?.errors) {
    const mensagens = Object.values(problema.errors).flat();
    if (mensagens.length > 0) {
      return mensagens.join(' ');
    }
  }

  if (problema?.detail) {
    return problema.detail;
  }

  if (problema?.title) {
    return problema.title;
  }

  if (erro.status >= 500) {
    return 'A API encontrou um erro inesperado. Tente de novo em instantes.';
  }

  return `Falha na requisição (HTTP ${erro.status}).`;
}
