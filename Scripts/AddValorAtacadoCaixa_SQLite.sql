-- Script para adicionar coluna valor_atacado_caixa na tabela estoque do SQLite
-- Execute este script no seu banco de dados local (adega_local.db)

-- Adicionar a coluna valor_atacado_caixa se ela não existir
-- NOTA: SQLite não suporta IF NOT EXISTS para ALTER TABLE,
-- portanto o código C# já faz a verificação antes de adicionar.

-- Se precisar adicionar manualmente, use:
ALTER TABLE estoque ADD COLUMN valor_atacado_caixa DECIMAL(10,2) DEFAULT 0;

-- Verificar se a coluna foi criada corretamente
PRAGMA table_info(estoque);

-- Você deve ver uma linha com:
-- cid: 10
-- name: valor_atacado_caixa
-- type: DECIMAL(10,2)
-- notnull: 0
-- dflt_value: 0
-- pk: 0
