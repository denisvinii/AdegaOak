-- Migration: Add atacado columns and indexes (idempotent)
-- Use em ambiente de teste primeiro. Faça backup do banco antes de executar.
BEGIN;

-- 1) Adiciona coluna de valor de atacado por caixa na tabela estoque
ALTER TABLE estoque
    ADD COLUMN IF NOT EXISTS valor_atacado_caixa DECIMAL DEFAULT 0;

-- 2) Adiciona coluna de quantidade mínima de atacado (em CAIXAS) na tabela estoque
ALTER TABLE estoque
    ADD COLUMN IF NOT EXISTS quantidade_minima_atacado INTEGER DEFAULT 20;

-- 3) Se a tabela configuracao_atacado existir, migra valores para estoque.quantidade_minima_atacado
--    para garantir que o valor exibido/editar no app venha do estoque.
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'configuracao_atacado') THEN
        UPDATE estoque e
        SET quantidade_minima_atacado = c.quantidade_minima_atacado
        FROM configuracao_atacado c
        WHERE c.productid = e.productid AND c.quantidade_minima_atacado IS NOT NULL;
    END IF;
END$$;

-- 4) Garante que as colunas de movimentacoes necessárias existem
ALTER TABLE movimentacoes
    ADD COLUMN IF NOT EXISTS tipo_venda TEXT DEFAULT 'Varejo';

ALTER TABLE movimentacoes
    ADD COLUMN IF NOT EXISTS valor_unitario DECIMAL DEFAULT 0;

-- 5) Cria índices recomendados para evitar scans em queries de sincronização e relatórios
CREATE INDEX IF NOT EXISTS ix_movimentacoes_productid_data ON movimentacoes (productid, data);
CREATE INDEX IF NOT EXISTS ix_movimentacoes_productid_data_tipo_venda ON movimentacoes (productid, data, tipo_venda);

-- 6) (opcional) Atualiza triggers ou views que dependam do esquema antigo — informar manualmente se houver

COMMIT;

-- Instruções pós-migração:
-- 1) Verifique se estoque tem as novas colunas:
--      SELECT column_name FROM information_schema.columns WHERE table_name='estoque' AND column_name IN ('valor_atacado_caixa','quantidade_minima_atacado');
-- 2) Valide amostras:
--      SELECT productid, bebida, quantidade_minima_atacado, valor_atacado_caixa FROM estoque ORDER BY productid LIMIT 50;
-- 3) Se desejar remover a tabela configuracao_atacado após validação, faça backup e depois DROP TABLE configuracao_atacado;
-- 4) Crie índices adicionais conforme necessidade de queries (por ex. estoque(productid) já é PK).
