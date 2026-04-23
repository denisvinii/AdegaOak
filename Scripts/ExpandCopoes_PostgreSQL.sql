-- ============================================================
-- SCRIPT: Expandir Sistema de COPÕES para Combos
-- Banco de Dados: PostgreSQL (Aiven)
-- Data: 2024
-- COMPATÍVEL 100% COM VENDAS EXISTENTES
-- ============================================================

-- 1. Adicionar coluna tipo_movimento em copao_vendas (para future use, mantém compatibilidade)
ALTER TABLE IF EXISTS copao_vendas
ADD COLUMN IF NOT EXISTS tipo_movimento VARCHAR(30) DEFAULT 'Normal';

COMMENT ON COLUMN copao_vendas.tipo_movimento IS 
'Tipo de movimento: Normal (compatível com sistema existente), ValorZero (novos combos)';

-- 2. Criar índice para performance
CREATE INDEX IF NOT EXISTS idx_copao_vendas_tipo_movimento 
ON copao_vendas(tipo_movimento);

-- 3. VERIFICAÇÃO
SELECT 
    'Tabelas expandidas com sucesso!' as status,
    NOW() as data_execucao;

SELECT 
    column_name, 
    data_type, 
    is_nullable,
    column_default
FROM information_schema.columns
WHERE table_name = 'copao_vendas'
ORDER BY ordinal_position;
