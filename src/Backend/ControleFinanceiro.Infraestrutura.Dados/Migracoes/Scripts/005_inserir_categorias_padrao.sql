-- Categorias que já vêm prontas. Os ids são fixos para que o script de dados de exemplo
-- possa referenciá-los sem consulta prévia. O usuário pode criar outras pela tela.
insert into categorias_de_gasto (id, usuario_id, nome, tipo, cor_hexadecimal, ativa)
values
    ('a1000000-0000-4000-8000-000000000001', '11111111-1111-1111-1111-111111111111',
     'Gastos Fixos', 'GastoFixo', '#1565C0', true),
    ('a1000000-0000-4000-8000-000000000002', '11111111-1111-1111-1111-111111111111',
     'Mercado', 'Mercado', '#2E7D32', true),
    ('a1000000-0000-4000-8000-000000000003', '11111111-1111-1111-1111-111111111111',
     'Essencial', 'Essencial', '#EF6C00', true),
    ('a1000000-0000-4000-8000-000000000004', '11111111-1111-1111-1111-111111111111',
     'Lazer', 'Lazer', '#6A1B9A', true)
on conflict (id) do nothing;
