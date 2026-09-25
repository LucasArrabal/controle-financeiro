import { HttpClient, HttpParams } from '@angular/common/http';
import { computed, inject, Injectable, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { ConfiguracaoDaApi } from '../../nucleo/configuracao/configuracao-da-api';
import { Categoria, DadosDaCategoria } from '../modelos/categoria';

/**
 * Categorias são poucas e mudam pouco, então ficam num cache de signal carregado uma vez
 * e compartilhado por todas as telas que precisam do select colorido.
 */
@Injectable({ providedIn: 'root' })
export class ServicoDeCategorias {
  private readonly http = inject(HttpClient);

  private readonly cache = signal<Categoria[]>([]);
  private readonly carregandoAgora = signal(false);

  /** Todas as categorias conhecidas, incluindo as desativadas. */
  readonly categorias = this.cache.asReadonly();

  readonly carregando = this.carregandoAgora.asReadonly();

  /** Só as ativas — é o que o formulário de lançamento pode oferecer. */
  readonly ativas = computed(() => this.cache().filter((categoria) => categoria.ativa));

  /** Índice por id, para a tabela resolver nome e cor sem varrer a lista. */
  readonly porId = computed(
    () => new Map(this.cache().map((categoria) => [categoria.id, categoria])),
  );

  recarregar(): void {
    this.carregandoAgora.set(true);

    this.listar({ incluirInativas: true }).subscribe({
      next: (categorias) => {
        this.cache.set(categorias);
        this.carregandoAgora.set(false);
      },
      error: () => this.carregandoAgora.set(false),
    });
  }

  listar(opcoes: { incluirInativas: boolean }): Observable<Categoria[]> {
    const parametros = new HttpParams().set('incluirInativas', opcoes.incluirInativas);

    return this.http.get<Categoria[]>(ConfiguracaoDaApi.categorias(), { params: parametros });
  }

  criar(dados: DadosDaCategoria): Observable<Categoria> {
    return this.http
      .post<Categoria>(ConfiguracaoDaApi.categorias(), dados)
      .pipe(tap(() => this.recarregar()));
  }

  editar(id: string, dados: DadosDaCategoria): Observable<Categoria> {
    return this.http
      .put<Categoria>(ConfiguracaoDaApi.categoria(id), dados)
      .pipe(tap(() => this.recarregar()));
  }

  desativar(id: string): Observable<void> {
    return this.http
      .delete<void>(ConfiguracaoDaApi.categoria(id))
      .pipe(tap(() => this.recarregar()));
  }
}
