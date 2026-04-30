-- =====================================================
-- MIGRATION: Sistema de Vendas com Carrinho e Formas de Pagamento
-- Execute este script no PgAdmin
-- =====================================================

-- 1. Criar tabela de Vendas (agrupa múltiplas movimentações)
CREATE TABLE IF NOT EXISTS "Vendas" (
    "Id" SERIAL PRIMARY KEY,
    "Data" TIMESTAMP NOT NULL DEFAULT NOW(),
    "UsuarioId" INTEGER NOT NULL,
    "Responsavel" VARCHAR(100) NOT NULL,
    "ValorTotal" DECIMAL(10,2) NOT NULL,
    "ValorDinheiro" DECIMAL(10,2) NOT NULL DEFAULT 0,
    "ValorCartao" DECIMAL(10,2) NOT NULL DEFAULT 0,
    "ValorPix" DECIMAL(10,2) NOT NULL DEFAULT 0,
    "Observacao" TEXT,
    CONSTRAINT "FK_Vendas_Usuarios" FOREIGN KEY ("UsuarioId") 
        REFERENCES "Usuarios"("Id") ON DELETE RESTRICT
);

-- 2. Adicionar campo VendaId na tabela Movimentacoes (para vincular movimentações a uma venda)
ALTER TABLE "Movimentacoes" 
ADD COLUMN IF NOT EXISTS "VendaId" INTEGER;

ALTER TABLE "Movimentacoes"
ADD CONSTRAINT "FK_Movimentacoes_Vendas" 
FOREIGN KEY ("VendaId") REFERENCES "Vendas"("Id") ON DELETE SET NULL;

-- 3. Criar índices para melhor performance
CREATE INDEX IF NOT EXISTS "IX_Vendas_Data" ON "Vendas"("Data");
CREATE INDEX IF NOT EXISTS "IX_Vendas_UsuarioId" ON "Vendas"("UsuarioId");
CREATE INDEX IF NOT EXISTS "IX_Movimentacoes_VendaId" ON "Movimentacoes"("VendaId");

-- 4. Comentários para documentação
COMMENT ON TABLE "Vendas" IS 'Agrupa múltiplas movimentações de saída em uma única venda';
COMMENT ON COLUMN "Vendas"."ValorDinheiro" IS 'Valor pago em dinheiro';
COMMENT ON COLUMN "Vendas"."ValorCartao" IS 'Valor pago em cartão';
COMMENT ON COLUMN "Vendas"."ValorPix" IS 'Valor pago via PIX';
COMMENT ON COLUMN "Movimentacoes"."VendaId" IS 'Referência à venda que originou esta movimentação';

-- =====================================================
-- FIM DA MIGRATION
-- =====================================================

-- Para verificar se foi criado corretamente:
SELECT 
    table_name, 
    column_name, 
    data_type 
FROM information_schema.columns 
WHERE table_name IN ('Vendas', 'Movimentacoes')
ORDER BY table_name, ordinal_position;
