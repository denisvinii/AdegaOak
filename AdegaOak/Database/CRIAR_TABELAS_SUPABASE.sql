-- ============================================
-- SCRIPT DE CRIAÇÃO COMPLETO - SUPABASE
-- Sistema Adega Oak
-- ============================================

-- Limpar tabelas existentes (cuidado em produção!)
DROP TABLE IF EXISTS "ComboVendas" CASCADE;
DROP TABLE IF EXISTS "ComboComposicoes" CASCADE;
DROP TABLE IF EXISTS "Combos" CASCADE;
DROP TABLE IF EXISTS "Movimentacoes" CASCADE;
DROP TABLE IF EXISTS "Vendas" CASCADE;
DROP TABLE IF EXISTS "Despesas" CASCADE;
DROP TABLE IF EXISTS "Produtos" CASCADE;
DROP TABLE IF EXISTS "Usuarios" CASCADE;
DROP TABLE IF EXISTS "SaldoConfigs" CASCADE;

-- ============================================
-- 1. TABELA: Usuarios
-- ============================================
CREATE TABLE "Usuarios" (
    "Id" SERIAL PRIMARY KEY,
    "Nome" VARCHAR(100) NOT NULL,
    "Username" VARCHAR(50) NOT NULL UNIQUE,
    "PasswordHash" TEXT NOT NULL,
    "Role" VARCHAR(20) NOT NULL,
    "Ativo" BOOLEAN NOT NULL DEFAULT true,
    "CriadoEm" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE UNIQUE INDEX "IX_Usuarios_Username" ON "Usuarios"("Username");

-- Inserir usuário admin padrão
-- Senha: Admin@2024
INSERT INTO "Usuarios" ("Nome", "Username", "PasswordHash", "Role", "Ativo", "CriadoEm")
VALUES (
    'Administrador',
    'admin',
    '$2a$11$8vZ7YQfyVZxH5L3qN9X8/.rKZJ5mHJxGxN8fQXqZ5vZ7YQfyVZxH5',
    'admin',
    true,
    '2024-01-01 00:00:00'
);

-- ============================================
-- 2. TABELA: Produtos
-- ============================================
CREATE TABLE "Produtos" (
    "Id" SERIAL PRIMARY KEY,
    "Bebida" VARCHAR(100) NOT NULL,
    "Tamanho" VARCHAR(50) NOT NULL,
    "Material" VARCHAR(50) NOT NULL,
    "Valor" NUMERIC(10,2) NOT NULL,
    "ValorVenda" NUMERIC(10,2) NOT NULL,
    "QuantidadeCaixa" INTEGER NOT NULL,
    "ValorCaixa" NUMERIC(10,2) NOT NULL,
    "ValorAtacadoCaixa" NUMERIC(10,2) NOT NULL,
    "EstoqueMinimo" INTEGER NOT NULL,
    "QuantidadeMinimaAtacado" INTEGER NOT NULL,
    "Ativo" BOOLEAN NOT NULL DEFAULT true,
    "CriadoEm" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- ============================================
-- 3. TABELA: Vendas (Sistema de PDV)
-- ============================================
CREATE TABLE "Vendas" (
    "Id" SERIAL PRIMARY KEY,
    "Data" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UsuarioId" INTEGER NOT NULL,
    "Responsavel" VARCHAR(100) NOT NULL,
    "ValorTotal" NUMERIC(10,2) NOT NULL,
    "ValorDinheiro" NUMERIC(10,2) NOT NULL DEFAULT 0,
    "ValorCartao" NUMERIC(10,2) NOT NULL DEFAULT 0,
    "ValorPix" NUMERIC(10,2) NOT NULL DEFAULT 0,
    "Observacao" TEXT,
    CONSTRAINT "FK_Vendas_Usuarios" FOREIGN KEY ("UsuarioId") 
        REFERENCES "Usuarios"("Id") ON DELETE RESTRICT
);

CREATE INDEX "IX_Vendas_Data" ON "Vendas"("Data");
CREATE INDEX "IX_Vendas_UsuarioId" ON "Vendas"("UsuarioId");

COMMENT ON TABLE "Vendas" IS 'Agrupa múltiplas movimentações de saída em uma única venda';
COMMENT ON COLUMN "Vendas"."ValorDinheiro" IS 'Valor pago em dinheiro';
COMMENT ON COLUMN "Vendas"."ValorCartao" IS 'Valor pago em cartão';
COMMENT ON COLUMN "Vendas"."ValorPix" IS 'Valor pago via PIX';

-- ============================================
-- 4. TABELA: Movimentacoes
-- ============================================
CREATE TABLE "Movimentacoes" (
    "Id" SERIAL PRIMARY KEY,
    "Data" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "Tipo" VARCHAR(10) NOT NULL,
    "TipoVenda" VARCHAR(10) NOT NULL,
    "ProdutoId" INTEGER NOT NULL,
    "ProdutoDescricao" VARCHAR(200) NOT NULL,
    "Quantidade" INTEGER NOT NULL,
    "UsuarioId" INTEGER NOT NULL,
    "Responsavel" VARCHAR(100) NOT NULL,
    "TipoSaida" VARCHAR(50),
    "ValorUnitario" NUMERIC(10,2) NOT NULL,
    "VendaId" INTEGER,
    CONSTRAINT "FK_Movimentacoes_Produtos" FOREIGN KEY ("ProdutoId") 
        REFERENCES "Produtos"("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Movimentacoes_Usuarios" FOREIGN KEY ("UsuarioId") 
        REFERENCES "Usuarios"("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Movimentacoes_Vendas" FOREIGN KEY ("VendaId") 
        REFERENCES "Vendas"("Id") ON DELETE SET NULL
);

CREATE INDEX "IX_Movimentacoes_ProdutoId" ON "Movimentacoes"("ProdutoId");
CREATE INDEX "IX_Movimentacoes_UsuarioId" ON "Movimentacoes"("UsuarioId");
CREATE INDEX "IX_Movimentacoes_VendaId" ON "Movimentacoes"("VendaId");
CREATE INDEX "IX_Movimentacoes_Tipo_Data" ON "Movimentacoes"("Tipo", "Data");
CREATE INDEX "IX_Movimentacoes_Data" ON "Movimentacoes"("Data");

COMMENT ON COLUMN "Movimentacoes"."VendaId" IS 'Referência à venda que originou esta movimentação';

-- ============================================
-- 5. TABELA: Despesas
-- ============================================
CREATE TABLE "Despesas" (
    "Id" SERIAL PRIMARY KEY,
    "Descricao" VARCHAR(200) NOT NULL,
    "Valor" NUMERIC(10,2) NOT NULL,
    "Data" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "Tipo" INTEGER NOT NULL,
    "Pago" BOOLEAN NOT NULL DEFAULT false,
    "DataPagamento" TIMESTAMP,
    "Notas" TEXT,
    "CriadoEm" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "ProdutoId" INTEGER,
    "Quantidade" INTEGER NOT NULL DEFAULT 0,
    CONSTRAINT "FK_Despesas_Produtos" FOREIGN KEY ("ProdutoId") 
        REFERENCES "Produtos"("Id") ON DELETE SET NULL
);

CREATE INDEX "IX_Despesas_ProdutoId" ON "Despesas"("ProdutoId");
CREATE INDEX "IX_Despesas_Data" ON "Despesas"("Data");
CREATE INDEX "IX_Despesas_Pago_Data" ON "Despesas"("Pago", "Data");

-- ============================================
-- 6. TABELA: Combos
-- ============================================
CREATE TABLE "Combos" (
    "Id" SERIAL PRIMARY KEY,
    "Nome" VARCHAR(100) NOT NULL UNIQUE,
    "Descricao" TEXT,
    "PrecoVenda" NUMERIC(10,2) NOT NULL,
    "Ativo" BOOLEAN NOT NULL DEFAULT true,
    "EhCopao" BOOLEAN NOT NULL DEFAULT false,
    "CriadoEm" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE UNIQUE INDEX "IX_Combos_Nome" ON "Combos"("Nome");

-- ============================================
-- 7. TABELA: ComboComposicoes
-- ============================================
CREATE TABLE "ComboComposicoes" (
    "Id" SERIAL PRIMARY KEY,
    "ComboId" INTEGER NOT NULL,
    "ProdutoId" INTEGER NOT NULL,
    "Quantidade" NUMERIC(10,4) NOT NULL,
    "Unidade" VARCHAR(20) NOT NULL,
    "DebitaEstoque" BOOLEAN NOT NULL DEFAULT true,
    CONSTRAINT "FK_ComboComposicoes_Combos" FOREIGN KEY ("ComboId") 
        REFERENCES "Combos"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_ComboComposicoes_Produtos" FOREIGN KEY ("ProdutoId") 
        REFERENCES "Produtos"("Id") ON DELETE RESTRICT
);

CREATE INDEX "IX_ComboComposicoes_ComboId" ON "ComboComposicoes"("ComboId");
CREATE INDEX "IX_ComboComposicoes_ProdutoId" ON "ComboComposicoes"("ProdutoId");

-- ============================================
-- 8. TABELA: ComboVendas
-- ============================================
CREATE TABLE "ComboVendas" (
    "Id" SERIAL PRIMARY KEY,
    "ComboId" INTEGER NOT NULL,
    "UsuarioId" INTEGER NOT NULL,
    "Quantidade" INTEGER NOT NULL,
    "PrecoUnitario" NUMERIC(10,2) NOT NULL,
    "PrecoTotal" NUMERIC(10,2) NOT NULL,
    "DataVenda" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "Responsavel" VARCHAR(100),
    "Observacoes" TEXT,
    "TipoMovimento" VARCHAR(50) NOT NULL,
    CONSTRAINT "FK_ComboVendas_Combos" FOREIGN KEY ("ComboId") 
        REFERENCES "Combos"("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_ComboVendas_Usuarios" FOREIGN KEY ("UsuarioId") 
        REFERENCES "Usuarios"("Id") ON DELETE RESTRICT
);

CREATE INDEX "IX_ComboVendas_ComboId" ON "ComboVendas"("ComboId");
CREATE INDEX "IX_ComboVendas_UsuarioId" ON "ComboVendas"("UsuarioId");

-- ============================================
-- 9. TABELA: SaldoConfigs (Singleton)
-- ============================================
CREATE TABLE "SaldoConfigs" (
    "Id" INTEGER PRIMARY KEY CHECK ("Id" = 1),
    "CapitalAdmin" NUMERIC(10,2) NOT NULL DEFAULT 0,
    "AtualizadoEm" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Inserir registro único
INSERT INTO "SaldoConfigs" ("Id", "CapitalAdmin", "AtualizadoEm")
VALUES (1, 0.00, '2024-01-01 00:00:00');

-- ============================================
-- VERIFICAÇÃO FINAL
-- ============================================
SELECT 'Tabelas criadas com sucesso!' as status;

-- Listar todas as tabelas
SELECT table_name 
FROM information_schema.tables 
WHERE table_schema = 'public' 
ORDER BY table_name;

-- Verificar usuário admin
SELECT "Id", "Nome", "Username", "Role", "Ativo" 
FROM "Usuarios" 
WHERE "Username" = 'admin';
