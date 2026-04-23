-- REVERSÃO: Renomear COMBOS de volta para COPOES
-- PostgreSQL

BEGIN;

-- Renomear tabelas de volta
ALTER TABLE IF EXISTS combos RENAME TO copoes;
ALTER TABLE IF EXISTS combo_composicao RENAME TO copao_composicao;
ALTER TABLE IF EXISTS combo_vendas RENAME TO copao_vendas;

-- Renomear sequences de volta
ALTER SEQUENCE IF EXISTS combos_combo_id_seq RENAME TO copoes_copao_id_seq;
ALTER SEQUENCE IF EXISTS combo_composicao_composicao_id_seq RENAME TO copao_composicao_composicao_id_seq;
ALTER SEQUENCE IF EXISTS combo_vendas_venda_id_seq RENAME TO copao_vendas_venda_id_seq;

-- Renomear coluna de volta
ALTER TABLE copoes RENAME COLUMN combo_id TO copao_id;
ALTER TABLE copao_composicao RENAME COLUMN combo_id TO copao_id;
ALTER TABLE copao_vendas RENAME COLUMN combo_id TO copao_id;

-- Remover índices novos
DROP INDEX IF EXISTS idx_combo_vendas_tipo_movimento;
DROP INDEX IF EXISTS idx_combo_composicao_combo;
DROP INDEX IF EXISTS idx_combo_composicao_product;
DROP INDEX IF EXISTS idx_combo_vendas_combo;
DROP INDEX IF EXISTS idx_combo_vendas_data;

-- Restaurar constraints antigas
ALTER TABLE copao_composicao DROP CONSTRAINT IF EXISTS combo_composicao_combo_id_fkey;
ALTER TABLE copao_composicao ADD CONSTRAINT copao_composicao_copao_id_fkey 
    FOREIGN KEY (copao_id) REFERENCES copoes(copao_id) ON DELETE CASCADE;

ALTER TABLE copao_vendas DROP CONSTRAINT IF EXISTS combo_vendas_combo_id_fkey;
ALTER TABLE copao_vendas ADD CONSTRAINT copao_vendas_copao_id_fkey 
    FOREIGN KEY (copao_id) REFERENCES copoes(copao_id);

-- Remover coluna tipo_movimento se existir
ALTER TABLE copao_vendas DROP COLUMN IF EXISTS tipo_movimento;

COMMIT;

SELECT 'Reversão concluída!' as status;
