-- Receitas. "recorrente" marca os lançamentos que se repetem todo mês (salário)
-- e que a tela oferece replicar nas competências seguintes.
create table if not exists receitas (
    id                   uuid          not null,
    usuario_id           uuid          not null,
    descricao            varchar(150)  not null,
    valor                numeric(14,2) not null,
    data_do_recebimento  date          not null,
    recorrente           boolean       not null default false,
    criado_em            timestamptz   not null default now(),

    constraint pk_receitas primary key (id),
    constraint ck_receitas_valor_positivo check (valor > 0),
    constraint ck_receitas_descricao check (length(btrim(descricao)) >= 2)
);

create index if not exists ix_receitas_usuario_data_do_recebimento
    on receitas (usuario_id, data_do_recebimento);
