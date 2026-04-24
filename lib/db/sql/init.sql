-- Adega Oak — Gestão
-- Idempotent schema bootstrap. Safe to run on a fresh or existing Postgres database.
-- Tables match the original WPF AdegaOak schema and are also compatible with the
-- Brazilian Portuguese column names used by the API server.

CREATE TABLE IF NOT EXISTS estoque (
    productid                 SERIAL PRIMARY KEY,
    bebida                    TEXT NOT NULL,
    tamanho                   TEXT NOT NULL,
    material                  TEXT NOT NULL,
    valor                     NUMERIC(10,2) NOT NULL DEFAULT 0,
    valor_venda               NUMERIC(10,2) NOT NULL DEFAULT 0,
    quantidade_caixa          INTEGER       NOT NULL DEFAULT 12,
    valor_caixa               NUMERIC(10,2) NOT NULL DEFAULT 0,
    valor_atacado_caixa       NUMERIC(10,2) NOT NULL DEFAULT 0,
    estoque_minimo            INTEGER       NOT NULL DEFAULT 5,
    quantidade_minima_atacado INTEGER       NOT NULL DEFAULT 20,
    ativo                     BOOLEAN       NOT NULL DEFAULT TRUE,
    criado_em                 TIMESTAMP     NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS movimentacoes (
    id              SERIAL PRIMARY KEY,
    data            TIMESTAMP     NOT NULL DEFAULT NOW(),
    tipo            TEXT          NOT NULL CHECK (tipo IN ('Entrada', 'Saída')),
    tipo_venda      TEXT          NOT NULL DEFAULT 'Varejo',
    productid       INTEGER       NOT NULL REFERENCES estoque(productid),
    produto         TEXT          NOT NULL,
    quantidade      INTEGER       NOT NULL,
    responsavel     TEXT          NOT NULL,
    saida           TEXT,
    valor_unitario  NUMERIC(10,2) NOT NULL DEFAULT 0,
    observacoes     TEXT
);
CREATE INDEX IF NOT EXISTS idx_movimentacoes_productid ON movimentacoes(productid);
CREATE INDEX IF NOT EXISTS idx_movimentacoes_data      ON movimentacoes(data DESC);
CREATE INDEX IF NOT EXISTS idx_movimentacoes_tipo      ON movimentacoes(tipo);

CREATE TABLE IF NOT EXISTS funcionarios (
    id        SERIAL PRIMARY KEY,
    username  TEXT      NOT NULL UNIQUE,
    ativo     BOOLEAN   NOT NULL DEFAULT TRUE,
    criado_em TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS combos (
    combo_id      SERIAL PRIMARY KEY,
    nome          VARCHAR(120) NOT NULL,
    descricao     VARCHAR(500),
    preco_venda   NUMERIC(10,2) NOT NULL DEFAULT 0,
    ativo         BOOLEAN       NOT NULL DEFAULT TRUE,
    data_criacao  TIMESTAMP     NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS combo_composicao (
    composicao_id   SERIAL PRIMARY KEY,
    combo_id        INTEGER      NOT NULL REFERENCES combos(combo_id) ON DELETE CASCADE,
    product_id      INTEGER      NOT NULL REFERENCES estoque(productid),
    quantidade      NUMERIC(10,2) NOT NULL,
    unidade         VARCHAR(20)   NOT NULL DEFAULT 'un',
    debita_estoque  BOOLEAN       NOT NULL DEFAULT TRUE
);
CREATE INDEX IF NOT EXISTS idx_combo_composicao_combo_id ON combo_composicao(combo_id);

CREATE TABLE IF NOT EXISTS combo_vendas (
    venda_id        SERIAL PRIMARY KEY,
    combo_id        INTEGER       NOT NULL REFERENCES combos(combo_id),
    quantidade      INTEGER       NOT NULL,
    preco_unitario  NUMERIC(10,2) NOT NULL,
    preco_total     NUMERIC(10,2) NOT NULL,
    data_venda      TIMESTAMP     NOT NULL DEFAULT NOW(),
    responsavel     VARCHAR(60),
    observacoes     VARCHAR(500),
    tipo_movimento  TEXT          NOT NULL DEFAULT 'Saida'
);
CREATE INDEX IF NOT EXISTS idx_combo_vendas_data_venda ON combo_vendas(data_venda DESC);

CREATE TABLE IF NOT EXISTS despesas (
    id              SERIAL PRIMARY KEY,
    descricao       TEXT          NOT NULL,
    valor           NUMERIC(10,2) NOT NULL,
    data            TIMESTAMP     NOT NULL,
    tipo            INTEGER       NOT NULL DEFAULT 0, -- 0 Operacional · 1 Fornecedor · 2 Salário · 3 Outros
    pago            BOOLEAN       NOT NULL DEFAULT FALSE,
    data_pagamento  TIMESTAMP,
    notas           TEXT,
    criado_em       TIMESTAMP     NOT NULL DEFAULT NOW()
);
CREATE INDEX IF NOT EXISTS idx_despesas_data ON despesas(data DESC);
CREATE INDEX IF NOT EXISTS idx_despesas_pago ON despesas(pago);

-- Seed default funcionarios on first run only
INSERT INTO funcionarios (username, ativo)
SELECT 'admin', TRUE
WHERE NOT EXISTS (SELECT 1 FROM funcionarios WHERE username = 'admin');

INSERT INTO funcionarios (username, ativo)
SELECT 'vendedor', TRUE
WHERE NOT EXISTS (SELECT 1 FROM funcionarios WHERE username = 'vendedor');
