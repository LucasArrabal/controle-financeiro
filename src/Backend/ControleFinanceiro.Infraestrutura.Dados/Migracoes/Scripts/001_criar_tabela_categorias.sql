-- Categorias de gasto. Nunca são apagadas: a coluna "ativa" faz o soft delete,
-- para que despesas antigas continuem tendo nome e cor no painel.
create table if not exists categorias_de_gasto (
    id               uuid        not null,
    usuario_id       uuid        not null,
    nome             varchar(60) not null,
    tipo             varchar(20) not null,
    cor_hexadecimal  char(7)     not null,
    ativa            boolean     not null default true,

    constraint pk_categorias_de_gasto primary key (id),
    constraint ck_categorias_de_gasto_nome check (length(btrim(nome)) >= 2),
    constraint ck_categorias_de_gasto_cor check (cor_hexadecimal ~ '^#[0-9A-F]{6}$'),
    constraint ck_categorias_de_gasto_tipo
        check (tipo in ('GastoFixo', 'Mercado', 'Essencial', 'Lazer'))
);

-- Nome único por usuário, ignorando maiúsculas/minúsculas.
create unique index if not exists ux_categorias_de_gasto_usuario_nome
    on categorias_de_gasto (usuario_id, lower(nome));
