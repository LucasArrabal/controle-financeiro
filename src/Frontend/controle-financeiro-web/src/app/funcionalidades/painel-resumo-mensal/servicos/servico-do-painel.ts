import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ResumoMensal } from '../../../compartilhado/modelos/resumo-mensal';
import { ConfiguracaoDaApi } from '../../../nucleo/configuracao/configuracao-da-api';

@Injectable({ providedIn: 'root' })
export class ServicoDoPainel {
  private readonly http = inject(HttpClient);

  /** @param competencia texto no formato `aaaa-MM`. */
  obterResumoMensal(competencia: string): Observable<ResumoMensal> {
    const parametros = new HttpParams().set('competencia', competencia);

    return this.http.get<ResumoMensal>(ConfiguracaoDaApi.resumoMensal(), { params: parametros });
  }
}
