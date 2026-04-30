-- =====================================================
-- SCRIPT DE CRIAÇÃO DE ÍNDICES PARA PERFORMANCE
-- Execute este script no Supabase SQL Editor
-- =====================================================

-- Índices para tabela Movimentacoes (queries mais lentas)
CREATE INDEX IF NOT EXISTS "IX_Movimentacoes_Data" ON "Movimentacoes" ("Data" DESC);
CREATE INDEX IF NOT EXISTS "IX_Movimentacoes_Tipo" ON "Movimentacoes" ("Tipo");
CREATE INDEX IF NOT EXISTS "IX_Movimentacoes_ProdutoId" ON "Movimentacoes" ("ProdutoId");
CREATE INDEX IF NOT EXISTS "IX_Movimentacoes_UsuarioId" ON "Movimentacoes" ("UsuarioId");
CREATE INDEX IF NOT EXISTS "IX_Movimentacoes_TipoVenda" ON "Movimentacoes" ("TipoVenda");
CREATE INDEX IF NOT EXISTS "IX_Movimentacoes_Data_Tipo" ON "Movimentacoes" ("Data" DESC, "Tipo");

-- Índices para tabela Despesas
CREATE INDEX IF NOT EXISTS "IX_Despesas_Data" ON "Despesas" ("Data" DESC);
CREATE INDEX IF NOT EXISTS "IX_Despesas_Pago" ON "Despesas" ("Pago");
CREATE INDEX IF NOT EXISTS "IX_Despesas_Data_Pago" ON "Despesas" ("Data" DESC, "Pago");
CREATE INDEX IF NOT EXISTS "IX_Despesas_ProdutoId" ON "Despesas" ("ProdutoId");

-- Índices para tabela ComboVendas
CREATE INDEX IF NOT EXISTS "IX_ComboVendas_DataVenda" ON "ComboVendas" ("DataVenda" DESC);
CREATE INDEX IF NOT EXISTS "IX_ComboVendas_ComboId" ON "ComboVendas" ("ComboId");
CREATE INDEX IF NOT EXISTS "IX_ComboVendas_UsuarioId" ON "ComboVendas" ("UsuarioId");

-- Índices para tabela Produtos
CREATE INDEX IF NOT EXISTS "IX_Produtos_Ativo" ON "Produtos" ("Ativo");
CREATE INDEX IF NOT EXISTS "IX_Produtos_Bebida" ON "Produtos" ("Bebida");

-- Índices para tabela Combos
CREATE INDEX IF NOT EXISTS "IX_Combos_Ativo" ON "Combos" ("Ativo");
CREATE INDEX IF NOT EXISTS "IX_Combos_EhCopao" ON "Combos" ("EhCopao");

-- Índices para tabela Usuarios
CREATE INDEX IF NOT EXISTS "IX_Usuarios_Username" ON "Usuarios" ("Username");
CREATE INDEX IF NOT EXISTS "IX_Usuarios_Ativo" ON "Usuarios" ("Ativo");

-- Verificar índices criados
SELECT 
    schemaname,
    tablename,
    indexname,
    indexdef
FROM pg_indexes
WHERE schemaname = 'public'
ORDER BY tablename, indexname;
