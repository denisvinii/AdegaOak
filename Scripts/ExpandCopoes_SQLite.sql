-- ============================================================
-- SCRIPT: Expandir Sistema de COPÕES para Combos
-- Banco de Dados: SQLite
-- Data: 2024
-- COMPATÍVEL 100% COM VENDAS EXISTENTES
-- ============================================================

-- 1. Adicionar coluna tipo_movimento em copao_vendas
PRAGMA table_info(copao_vendas);
-- Se não tiver tipo_movimento:
ALTER TABLE copao_vendas ADD COLUMN tipo_movimento TEXT DEFAULT 'Normal';

-- 2. Índice para performance
CREATE INDEX IF NOT EXISTS idx_copao_vendas_tipo_movimento 
ON copao_vendas(tipo_movimento);

-- 3. Verificação
SELECT 'Tabelas expandidas com sucesso!' as status;
