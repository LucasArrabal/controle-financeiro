import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import {
  DadosDaReceita,
  Receita,
  ResultadoDaReplicacao,
} from '../../../compartilhado/modelos/receita';
import { ConfiguracaoDaApi } from '../../../nucleo/configuracao/configuracao-da-api';

@Injectable({ providedIn: 'root' })
export class ServicoDeReceitas {
  private readonly http = inject(HttpClient);

  /** @param competencia texto no formato `aaaa-MM`. */
  listarDoMes(competencia: string): Observable<Receita[]> {
    const parametros = new HttpParams().set('competencia', competencia);

    return this.http.get<Receita[]>(ConfiguracaoDaApi.receitas(), { params: parametros });
  }

  registrar(dados: DadosDaReceita): Observable<Receita> {
    return this.http.post<Receita>(ConfiguracaoDaApi.receitas(), dados);
  }

  editar(id: string, dados: DadosDaReceita): Observable<Receita> {
    return this.http.put<Receita>(ConfiguracaoDaApi.receita(id), dados);
  }

  excluir(id: string): Observable<void> {
    return this.http.delete<void>(ConfiguracaoDaApi.receita(id));
  }

  /** Copia a receita para os próximos meses. Meses já ocupados voltam em `competenciasIgnoradas`. */
  replicarNosMesesSeguintes(id: string, meses: number): Observable<ResultadoDaReplicacao> {
    const parametros = new HttpParams().set('meses', meses);

    return this.http.post<ResultadoDaReplicacao>(
      ConfiguracaoDaApi.replicacaoDeReceita(id),
      null,
      { params: parametros },
    );
  }
}
