import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { App } from './app';
import { routes } from './app.routes';

describe('App', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [provideRouter(routes), provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
  });

  it('monta o shell da aplicação', () => {
    const fixture = TestBed.createComponent(App);

    expect(fixture.componentInstance).toBeTruthy();
  });

  it('mostra a barra superior e o menu lateral', async () => {
    const fixture = TestBed.createComponent(App);
    await fixture.whenStable();

    const elemento = fixture.nativeElement as HTMLElement;

    expect(elemento.querySelector('app-barra-superior')).not.toBeNull();
    expect(elemento.querySelector('app-menu-lateral')).not.toBeNull();
  });
});
