-- Script para adicionar coluna valor_atacado_caixa na tabela estoque do PostgreSQL
-- Execute este script no seu banco de dados Aiven PostgreSQL

-- Adicionar a coluna valor_atacado_caixa se ela não existir
ALTER TABLE IF EXISTS estoque
ADD COLUMN IF NOT EXISTS valor_atacado_caixa DECIMAL DEFAULT 0;

-- Comentário na coluna para documentação
COMMENT ON COLUMN estoque.valor_atacado_caixa IS 'Valor especial de caixa para vendas em atacado (mínimo 20 caixas)';

-- Criar índice para melhor performance (opcional)
CREATE INDEX IF NOT EXISTS idx_estoque_valor_atacado ON estoque(valor_atacado_caixa);

-- Verificar se a coluna foi criada corretamente
SELECT column_name, data_type, is_nullable, column_default
FROM information_schema.columns
WHERE table_name = 'estoque' AND column_name = 'valor_atacado_caixa';
