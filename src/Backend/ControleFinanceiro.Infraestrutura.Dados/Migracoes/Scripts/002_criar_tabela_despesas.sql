-- Despesas. data_do_gasto é date puro (o dia em que o dinheiro saiu, sem fuso);
-- só criado_em é timestamptz, gravado sempre em UTC.
create table if not exists despesas (
    id                  uuid          not null,
    usuario_id          uuid          not null,
    descricao           varchar(150)  not null,
    valor               numeric(14,2) not null,
    data_do_gasto       date          not null,
    categoria_id        uuid          not null,
    forma_de_pagamento  varchar(30)   null,
    observacao          text          null,
    criado_em           timestamptz   not null default now(),

    constraint pk_despesas primary key (id),
    constraint fk_despesas_categoria foreign key (categoria_id)
        references categorias_de_gasto (id),
    constraint ck_despesas_valor_positivo check (valor > 0),
    constraint ck_despesas_descricao check (length(btrim(descricao)) >= 2)
);

-- Atende ao filtro por competência, que é sempre usuario_id + intervalo de datas.
create index if not exists ix_despesas_usuario_data_do_gasto
    on despesas (usuario_id, data_do_gasto);

create index if not exists ix_despesas_categoria_id
    on despesas (categoria_id);
