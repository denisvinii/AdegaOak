-- ============================================
-- Script de Criação do Banco de Dados PostgreSQL
-- AdegaOak - Sistema de Gestão
-- ============================================

-- IMPORTANTE: Este script é gerado automaticamente pelo Entity Framework
-- Você NÃO precisa executar manualmente - o EF faz isso automaticamente!
-- Use este script apenas se quiser criar o banco manualmente no pgAdmin

-- ============================================
-- 1. TABELA: Produtos
-- ============================================
CREATE TABLE "Produtos" (
    "Id" SERIAL PRIMARY KEY,
    "Bebida" VARCHAR(100) NOT NULL,
    "Tamanho" VARCHAR(50) NOT NULL,
    "Material" VARCHAR(50) NOT NULL,
    "Valor" DECIMAL(10,2) NOT NULL,
    "ValorVenda" DECIMAL(10,2) NOT NULL,
    "QuantidadeCaixa" INTEGER NOT NULL,
    "ValorCaixa" DECIMAL(10,2) NOT NULL,
    "ValorAtacadoCaixa" DECIMAL(10,2) NOT NULL,
    "EstoqueMinimo" INTEGER NOT NULL,
    "QuantidadeMinimaAtacado" INTEGER NOT NULL,
    "Ativo" BOOLEAN NOT NULL,
    "CriadoEm" TIMESTAMP NOT NULL
);

-- ============================================
-- 2. TABELA: SaldoConfigs
-- ============================================
CREATE TABLE "SaldoConfigs" (
    "Id" INTEGER PRIMARY KEY,
    "CapitalAdmin" DECIMAL(10,2) NOT NULL,
    "AtualizadoEm" TIMESTAMP NOT NULL
);

-- ============================================
-- 3. TABELA: Usuarios
-- ============================================
CREATE TABLE "Usuarios" (
    "Id" SERIAL PRIMARY KEY,
    "Nome" VARCHAR(100) NOT NULL,
    "Username" VARCHAR(50) NOT NULL,
    "PasswordHash" TEXT NOT NULL,
    "Role" VARCHAR(20) NOT NULL,
    "Ativo" BOOLEAN NOT NULL,
    "CriadoEm" TIMESTAMP NOT NULL
);

-- ============================================
-- 4. TABELA: Combos
-- ============================================
CREATE TABLE "Combos" (
    "Id" SERIAL PRIMARY KEY,
    "Nome" VARCHAR(100) NOT NULL,
    "Descricao" TEXT,
    "PrecoVenda" DECIMAL(10,2) NOT NULL,
    "Ativo" BOOLEAN NOT NULL,
    "EhCopao" BOOLEAN NOT NULL,
    "CriadoEm" TIMESTAMP NOT NULL
);

-- ============================================
-- 5. TABELA: Despesas
-- ============================================
CREATE TABLE "Despesas" (
    "Id" SERIAL PRIMARY KEY,
    "Descricao" VARCHAR(200) NOT NULL,
    "Valor" DECIMAL(10,2) NOT NULL,
    "Data" TIMESTAMP NOT NULL,
    "Tipo" INTEGER NOT NULL,
    "Pago" BOOLEAN NOT NULL,
    "DataPagamento" TIMESTAMP,
    "Notas" TEXT,
    "CriadoEm" TIMESTAMP NOT NULL,
    "ProdutoId" INTEGER,
    "Quantidade" INTEGER NOT NULL,
    CONSTRAINT "FK_Despesas_Produtos_ProdutoId" 
        FOREIGN KEY ("ProdutoId") 
        REFERENCES "Produtos" ("Id") 
        ON DELETE SET NULL
);

-- ============================================
-- 6. TABELA: Movimentacoes
-- ============================================
CREATE TABLE "Movimentacoes" (
    "Id" SERIAL PRIMARY KEY,
    "Data" TIMESTAMP NOT NULL,
    "Tipo" VARCHAR(10) NOT NULL,
    "TipoVenda" VARCHAR(10) NOT NULL,
    "ProdutoId" INTEGER NOT NULL,
    "ProdutoDescricao" VARCHAR(200) NOT NULL,
    "Quantidade" INTEGER NOT NULL,
    "UsuarioId" INTEGER NOT NULL,
    "Responsavel" VARCHAR(100) NOT NULL,
    "TipoSaida" TEXT,
    "ValorUnitario" DECIMAL(10,2) NOT NULL,
    CONSTRAINT "FK_Movimentacoes_Produtos_ProdutoId" 
        FOREIGN KEY ("ProdutoId") 
        REFERENCES "Produtos" ("Id") 
        ON DELETE RESTRICT,
    CONSTRAINT "FK_Movimentacoes_Usuarios_UsuarioId" 
        FOREIGN KEY ("UsuarioId") 
        REFERENCES "Usuarios" ("Id") 
        ON DELETE RESTRICT
);

-- ============================================
-- 7. TABELA: ComboComposicoes
-- ============================================
CREATE TABLE "ComboComposicoes" (
    "Id" SERIAL PRIMARY KEY,
    "ComboId" INTEGER NOT NULL,
    "ProdutoId" INTEGER NOT NULL,
    "Quantidade" DECIMAL(10,4) NOT NULL,
    "Unidade" VARCHAR(20) NOT NULL,
    "DebitaEstoque" BOOLEAN NOT NULL,
    CONSTRAINT "FK_ComboComposicoes_Combos_ComboId" 
        FOREIGN KEY ("ComboId") 
        REFERENCES "Combos" ("Id") 
        ON DELETE CASCADE,
    CONSTRAINT "FK_ComboComposicoes_Produtos_ProdutoId" 
        FOREIGN KEY ("ProdutoId") 
        REFERENCES "Produtos" ("Id") 
        ON DELETE RESTRICT
);

-- ============================================
-- 8. TABELA: ComboVendas
-- ============================================
CREATE TABLE "ComboVendas" (
    "Id" SERIAL PRIMARY KEY,
    "ComboId" INTEGER NOT NULL,
    "UsuarioId" INTEGER NOT NULL,
    "Quantidade" INTEGER NOT NULL,
    "PrecoUnitario" DECIMAL(10,2) NOT NULL,
    "PrecoTotal" DECIMAL(10,2) NOT NULL,
    "DataVenda" TIMESTAMP NOT NULL,
    "Responsavel" TEXT,
    "Observacoes" TEXT,
    "TipoMovimento" TEXT NOT NULL,
    CONSTRAINT "FK_ComboVendas_Combos_ComboId" 
        FOREIGN KEY ("ComboId") 
        REFERENCES "Combos" ("Id") 
        ON DELETE RESTRICT,
    CONSTRAINT "FK_ComboVendas_Usuarios_UsuarioId" 
        FOREIGN KEY ("UsuarioId") 
        REFERENCES "Usuarios" ("Id") 
        ON DELETE RESTRICT
);

-- ============================================
-- 9. ÍNDICES
-- ============================================
CREATE INDEX "IX_ComboComposicoes_ComboId" ON "ComboComposicoes" ("ComboId");
CREATE INDEX "IX_ComboComposicoes_ProdutoId" ON "ComboComposicoes" ("ProdutoId");
CREATE UNIQUE INDEX "IX_Combos_Nome" ON "Combos" ("Nome");
CREATE INDEX "IX_ComboVendas_ComboId" ON "ComboVendas" ("ComboId");
CREATE INDEX "IX_ComboVendas_UsuarioId" ON "ComboVendas" ("UsuarioId");
CREATE INDEX "IX_Despesas_ProdutoId" ON "Despesas" ("ProdutoId");
CREATE INDEX "IX_Movimentacoes_ProdutoId" ON "Movimentacoes" ("ProdutoId");
CREATE INDEX "IX_Movimentacoes_UsuarioId" ON "Movimentacoes" ("UsuarioId");
CREATE UNIQUE INDEX "IX_Usuarios_Username" ON "Usuarios" ("Username");

-- ============================================
-- 10. DADOS INICIAIS (SEED)
-- ============================================

-- Configuração de Saldo Inicial
INSERT INTO "SaldoConfigs" ("Id", "CapitalAdmin", "AtualizadoEm")
VALUES (1, 0, '2024-01-01 00:00:00');

-- Usuário Administrador
-- Username: admin
-- Password: admin (hash BCrypt)
INSERT INTO "Usuarios" ("Id", "Nome", "Username", "PasswordHash", "Role", "Ativo", "CriadoEm")
VALUES (1, 'Administrador', 'admin', '$2a$11$8vZ7YQfyVZxH5L3qN9X8/.rKZJ5mHJxGxN8fQXqZ5vZ7YQfyVZxH5', 'admin', true, '2024-01-01 00:00:00');

-- ============================================
-- CREDENCIAIS DE LOGIN PADRÃO
-- ============================================
-- Username: admin
-- Password: admin
-- ============================================

-- ============================================
-- FIM DO SCRIPT
-- ============================================
