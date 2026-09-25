-- Teto de gasto por categoria e competência. Ausência de linha significa "sem teto definido";
-- valor_limite = 0 é um teto de verdade e significa "nenhum gasto permitido".
create table if not exists tetos_de_gasto_mensal (
    id            uuid          not null,
    usuario_id    uuid          not null,
    categoria_id  uuid          not null,
    competencia   char(7)       not null,
    valor_limite  numeric(14,2) not null,

    constraint pk_tetos_de_gasto_mensal primary key (id),
    constraint fk_tetos_de_gasto_mensal_categoria foreign key (categoria_id)
        references categorias_de_gasto (id),
    constraint ck_tetos_de_gasto_mensal_limite check (valor_limite >= 0),
    constraint ck_tetos_de_gasto_mensal_competencia
        check (competencia ~ '^[0-9]{4}-(0[1-9]|1[0-2])$')
);

create unique index if not exists ux_tetos_de_gasto_mensal_categoria_competencia
    on tetos_de_gasto_mensal (categoria_id, competencia);

create index if not exists ix_tetos_de_gasto_mensal_usuario_competencia
    on tetos_de_gasto_mensal (usuario_id, competencia);
