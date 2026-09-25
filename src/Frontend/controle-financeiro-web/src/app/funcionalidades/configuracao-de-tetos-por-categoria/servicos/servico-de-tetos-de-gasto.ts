import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { DadosDosTetosDoMes, TetoDeGasto } from '../../../compartilhado/modelos/teto-de-gasto';
import { ConfiguracaoDaApi } from '../../../nucleo/configuracao/configuracao-da-api';

@Injectable({ providedIn: 'root' })
export class ServicoDeTetosDeGasto {
  private readonly http = inject(HttpClient);

  /** Uma linha por categoria ativa, com ou sem teto definido. */
  listarDoMes(competencia: string): Observable<TetoDeGasto[]> {
    const parametros = new HttpParams().set('competencia', competencia);

    return this.http.get<TetoDeGasto[]>(ConfiguracaoDaApi.tetosDeGasto(), { params: parametros });
  }

  /** Grava a tela inteira de uma vez; devolve os tetos já atualizados. */
  definir(dados: DadosDosTetosDoMes): Observable<TetoDeGasto[]> {
    return this.http.put<TetoDeGasto[]>(ConfiguracaoDaApi.tetosDeGasto(), dados);
  }

  copiarDoMesAnterior(competencia: string): Observable<TetoDeGasto[]> {
    const parametros = new HttpParams().set('competencia', competencia);

    return this.http.post<TetoDeGasto[]>(ConfiguracaoDaApi.copiaDeTetosDoMesAnterior(), null, {
      params: parametros,
    });
  }
}
