-- Dados de demonstração: 20 despesas, 3 receitas e 4 tetos no mês corrente, para o painel
-- já abrir preenchido. Só roda quando CONTROLEFINANCEIRO_SEMEAR_DADOS_DE_EXEMPLO=true,
-- justamente para nunca escapar para produção.
--
-- As datas são relativas ao mês atual, então o gráfico continua cheio em qualquer mês.

do $$
declare
    usuario        constant uuid := '11111111-1111-1111-1111-111111111111';
    gastos_fixos   constant uuid := 'a1000000-0000-4000-8000-000000000001';
    mercado        constant uuid := 'a1000000-0000-4000-8000-000000000002';
    essencial      constant uuid := 'a1000000-0000-4000-8000-000000000003';
    lazer          constant uuid := 'a1000000-0000-4000-8000-000000000004';
    inicio_do_mes  constant date := date_trunc('month', current_date)::date;
    -- Não pode se chamar "competencia": em PL/pgSQL o nome colidiria com a coluna
    -- de mesmo nome no insert de tetos_de_gasto_mensal.
    competencia_do_mes constant char(7) := to_char(current_date, 'YYYY-MM');
begin
    -- Não semeia duas vezes o mesmo mês.
    if exists (select 1 from despesas where usuario_id = usuario
                and data_do_gasto >= inicio_do_mes
                and data_do_gasto < inicio_do_mes + interval '1 month') then
        raise notice 'Dados de exemplo do mês % já existem; nada a fazer.', competencia_do_mes;
        return;
    end if;

    insert into despesas (id, usuario_id, descricao, valor, data_do_gasto, categoria_id,
                          forma_de_pagamento, observacao)
    values
        -- Gastos fixos
        (gen_random_uuid(), usuario, 'Aluguel',                1850.00, inicio_do_mes + 4,  gastos_fixos, 'Débito',   null),
        (gen_random_uuid(), usuario, 'Condomínio',              520.00, inicio_do_mes + 4,  gastos_fixos, 'Débito',   null),
        (gen_random_uuid(), usuario, 'Internet fibra',          129.90, inicio_do_mes + 9,  gastos_fixos, 'Cartão',   null),
        (gen_random_uuid(), usuario, 'Plano de celular',         59.90, inicio_do_mes + 11, gastos_fixos, 'Cartão',   null),
        (gen_random_uuid(), usuario, 'Energia elétrica',        214.35, inicio_do_mes + 14, gastos_fixos, 'Débito',   null),

        -- Mercado
        (gen_random_uuid(), usuario, 'Compra do mês',           612.40, inicio_do_mes + 2,  mercado,      'Cartão',   'Atacadão'),
        (gen_random_uuid(), usuario, 'Feira',                    98.70, inicio_do_mes + 6,  mercado,      'Pix',      null),
        (gen_random_uuid(), usuario, 'Padaria',                  43.20, inicio_do_mes + 8,  mercado,      'Pix',      null),
        (gen_random_uuid(), usuario, 'Reposição da semana',     187.55, inicio_do_mes + 13, mercado,      'Cartão',   null),
        (gen_random_uuid(), usuario, 'Hortifruti',               76.10, inicio_do_mes + 17, mercado,      'Pix',      null),
        (gen_random_uuid(), usuario, 'Açougue',                 158.90, inicio_do_mes + 20, mercado,      'Cartão',   null),

        -- Essencial
        (gen_random_uuid(), usuario, 'Farmácia',                 87.30, inicio_do_mes + 3,  essencial,    'Cartão',   null),
        (gen_random_uuid(), usuario, 'Combustível',             280.00, inicio_do_mes + 7,  essencial,    'Cartão',   null),
        (gen_random_uuid(), usuario, 'Consulta médica',         220.00, inicio_do_mes + 12, essencial,    'Pix',      null),
        (gen_random_uuid(), usuario, 'Material de limpeza',      94.60, inicio_do_mes + 16, essencial,    'Cartão',   null),
        (gen_random_uuid(), usuario, 'Transporte por app',       64.80, inicio_do_mes + 19, essencial,    'Cartão',   null),

        -- Lazer
        (gen_random_uuid(), usuario, 'Cinema',                   72.00, inicio_do_mes + 5,  lazer,        'Cartão',   null),
        (gen_random_uuid(), usuario, 'Jantar fora',             186.40, inicio_do_mes + 10, lazer,        'Cartão',   null),
        (gen_random_uuid(), usuario, 'Assinatura de streaming',   44.90, inicio_do_mes + 15, lazer,        'Cartão',   null),
        (gen_random_uuid(), usuario, 'Bar com amigos',          132.70, inicio_do_mes + 18, lazer,        'Pix',      null);

    insert into receitas (id, usuario_id, descricao, valor, data_do_recebimento, recorrente)
    values
        (gen_random_uuid(), usuario, 'Salário',            7800.00, inicio_do_mes + 4,  true),
        (gen_random_uuid(), usuario, 'Freela de projeto',  1200.00, inicio_do_mes + 12, false),
        (gen_random_uuid(), usuario, 'Reembolso',           180.00, inicio_do_mes + 21, false);

    insert into tetos_de_gasto_mensal (id, usuario_id, categoria_id, competencia, valor_limite)
    values
        (gen_random_uuid(), usuario, gastos_fixos, competencia_do_mes, 3000.00),
        (gen_random_uuid(), usuario, mercado,      competencia_do_mes, 1500.00),
        (gen_random_uuid(), usuario, essencial,    competencia_do_mes,  900.00),
        (gen_random_uuid(), usuario, lazer,        competencia_do_mes,  400.00)
    on conflict (categoria_id, competencia) do nothing;
end $$;
