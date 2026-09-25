import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { DadosDaDespesa, Despesa } from '../../../compartilhado/modelos/despesa';
import { ConfiguracaoDaApi } from '../../../nucleo/configuracao/configuracao-da-api';

@Injectable({ providedIn: 'root' })
export class ServicoDeDespesas {
  private readonly http = inject(HttpClient);

  /** @param competencia texto no formato `aaaa-MM`. */
  listarDoMes(competencia: string): Observable<Despesa[]> {
    const parametros = new HttpParams().set('competencia', competencia);

    return this.http.get<Despesa[]>(ConfiguracaoDaApi.despesas(), { params: parametros });
  }

  registrar(dados: DadosDaDespesa): Observable<Despesa> {
    return this.http.post<Despesa>(ConfiguracaoDaApi.despesas(), dados);
  }

  editar(id: string, dados: DadosDaDespesa): Observable<Despesa> {
    return this.http.put<Despesa>(ConfiguracaoDaApi.despesa(id), dados);
  }

  excluir(id: string): Observable<void> {
    return this.http.delete<void>(ConfiguracaoDaApi.despesa(id));
  }
}
