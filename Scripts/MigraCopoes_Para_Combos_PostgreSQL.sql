-- ==================== MIGRAÇÃO: COPOES -> COMBOS ====================
-- PostgreSQL
-- Renomeia tabelas mantendo dados intactos

BEGIN;

-- Renomear tabelas
ALTER TABLE IF EXISTS copoes RENAME TO combos;
ALTER TABLE IF EXISTS copao_composicao RENAME TO combo_composicao;
ALTER TABLE IF EXISTS copao_vendas RENAME TO combo_vendas;

-- Renomear sequences/constraints se necessário
ALTER SEQUENCE IF EXISTS copoes_copao_id_seq RENAME TO combos_combo_id_seq;
ALTER SEQUENCE IF EXISTS copao_composicao_composicao_id_seq RENAME TO combo_composicao_composicao_id_seq;
ALTER SEQUENCE IF EXISTS copao_vendas_venda_id_seq RENAME TO combo_vendas_venda_id_seq;

-- Renomear colunas na tabela combos
ALTER TABLE combos RENAME COLUMN copao_id TO combo_id;

-- Renomear colunas na tabela combo_composicao
ALTER TABLE combo_composicao RENAME COLUMN copao_id TO combo_id;

-- Renomear colunas na tabela combo_vendas
ALTER TABLE combo_vendas RENAME COLUMN copao_id TO combo_id;

-- Adicionar coluna tipo_movimento se não existir
ALTER TABLE combo_vendas ADD COLUMN IF NOT EXISTS tipo_movimento VARCHAR(30) DEFAULT 'Normal';

-- Criar índices com novos nomes
CREATE INDEX IF NOT EXISTS idx_combo_vendas_tipo_movimento ON combo_vendas(tipo_movimento);
CREATE INDEX IF NOT EXISTS idx_combo_composicao_combo_id ON combo_composicao(combo_id);
CREATE INDEX IF NOT EXISTS idx_combo_vendas_combo_id ON combo_vendas(combo_id);

-- Atualizar constraints de chave estrangeira
ALTER TABLE combo_composicao DROP CONSTRAINT IF EXISTS copao_composicao_copao_id_fkey;
ALTER TABLE combo_composicao ADD CONSTRAINT combo_composicao_combo_id_fkey 
    FOREIGN KEY (combo_id) REFERENCES combos(combo_id) ON DELETE CASCADE;

ALTER TABLE combo_vendas DROP CONSTRAINT IF EXISTS copao_vendas_copao_id_fkey;
ALTER TABLE combo_vendas ADD CONSTRAINT combo_vendas_combo_id_fkey 
    FOREIGN KEY (combo_id) REFERENCES combos(combo_id);

COMMIT;

SELECT 'Migração concluída com sucesso!' as status;
