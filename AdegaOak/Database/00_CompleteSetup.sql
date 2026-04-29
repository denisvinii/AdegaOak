-- ============================================================
-- Complete Database Setup for AdegaOak
-- ============================================================
-- This script creates all tables and inserts required seed data
-- Run this if you're having database issues
-- ============================================================

-- Drop existing tables (in correct order due to foreign keys)
DROP TABLE IF EXISTS ComboVendas;
DROP TABLE IF EXISTS ComboComposicoes;
DROP TABLE IF EXISTS Combos;
DROP TABLE IF EXISTS Movimentacoes;
DROP TABLE IF EXISTS Despesas;
DROP TABLE IF EXISTS Produtos;
DROP TABLE IF EXISTS Usuarios;
DROP TABLE IF EXISTS SaldoConfigs;
DROP TABLE IF EXISTS __EFMigrationsHistory;

-- ========================================
-- Table: Usuarios
-- ========================================
CREATE TABLE Usuarios (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Nome TEXT NOT NULL,
    Username TEXT NOT NULL UNIQUE,
    PasswordHash TEXT NOT NULL,
    Role TEXT NOT NULL CHECK(Role IN ('admin', 'funcionario')),
    Ativo INTEGER NOT NULL DEFAULT 1,
    CriadoEm TEXT NOT NULL DEFAULT (datetime('now'))
);

-- ========================================
-- Table: Produtos
-- ========================================
CREATE TABLE Produtos (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Bebida TEXT NOT NULL,
    Tamanho TEXT NOT NULL,
    Material TEXT NOT NULL,
    Valor REAL NOT NULL DEFAULT 0,
    ValorVenda REAL NOT NULL DEFAULT 0,
    QuantidadeCaixa INTEGER NOT NULL DEFAULT 1,
    ValorCaixa REAL NOT NULL DEFAULT 0,
    ValorAtacadoCaixa REAL NOT NULL DEFAULT 0,
    EstoqueMinimo INTEGER NOT NULL DEFAULT 0,
    QuantidadeMinimaAtacado INTEGER NOT NULL DEFAULT 20,
    Ativo INTEGER NOT NULL DEFAULT 1,
    CriadoEm TEXT NOT NULL DEFAULT (datetime('now'))
);

-- ========================================
-- Table: SaldoConfigs
-- ========================================
CREATE TABLE SaldoConfigs (
    Id INTEGER PRIMARY KEY,
    CapitalAdmin REAL NOT NULL DEFAULT 0,
    AtualizadoEm TEXT NOT NULL DEFAULT (datetime('now'))
);

-- ========================================
-- Table: Movimentacoes
-- ========================================
CREATE TABLE Movimentacoes (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Data TEXT NOT NULL DEFAULT (datetime('now')),
    Tipo TEXT NOT NULL CHECK(Tipo IN ('Entrada', 'Saída')),
    TipoVenda TEXT NOT NULL DEFAULT 'Varejo',
    ProdutoId INTEGER NOT NULL,
    ProdutoDescricao TEXT NOT NULL,
    Quantidade INTEGER NOT NULL,
    UsuarioId INTEGER NOT NULL,
    Responsavel TEXT NOT NULL,
    TipoSaida TEXT,
    ValorUnitario REAL NOT NULL DEFAULT 0,
    FOREIGN KEY (ProdutoId) REFERENCES Produtos(Id) ON DELETE RESTRICT,
    FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id) ON DELETE RESTRICT
);

-- ========================================
-- Table: Despesas
-- ========================================
CREATE TABLE Despesas (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Descricao TEXT NOT NULL,
    Valor REAL NOT NULL,
    Data TEXT NOT NULL DEFAULT (datetime('now')),
    Tipo INTEGER NOT NULL DEFAULT 0,
    Pago INTEGER NOT NULL DEFAULT 0,
    DataPagamento TEXT,
    Notas TEXT,
    CriadoEm TEXT NOT NULL DEFAULT (datetime('now')),
    ProdutoId INTEGER,
    Quantidade INTEGER NOT NULL DEFAULT 0,
    FOREIGN KEY (ProdutoId) REFERENCES Produtos(Id) ON DELETE SET NULL
);

-- ========================================
-- Table: Combos
-- ========================================
CREATE TABLE Combos (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Nome TEXT NOT NULL UNIQUE,
    Descricao TEXT,
    PrecoVenda REAL NOT NULL,
    Ativo INTEGER NOT NULL DEFAULT 1,
    EhCopao INTEGER NOT NULL DEFAULT 0,
    CriadoEm TEXT NOT NULL DEFAULT (datetime('now'))
);

-- ========================================
-- Table: ComboComposicoes
-- ========================================
CREATE TABLE ComboComposicoes (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ComboId INTEGER NOT NULL,
    ProdutoId INTEGER NOT NULL,
    Quantidade REAL NOT NULL,
    Unidade TEXT NOT NULL DEFAULT 'unidade',
    DebitaEstoque INTEGER NOT NULL DEFAULT 1,
    FOREIGN KEY (ComboId) REFERENCES Combos(Id) ON DELETE CASCADE,
    FOREIGN KEY (ProdutoId) REFERENCES Produtos(Id) ON DELETE RESTRICT
);

-- ========================================
-- Table: ComboVendas
-- ========================================
CREATE TABLE ComboVendas (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ComboId INTEGER NOT NULL,
    UsuarioId INTEGER NOT NULL,
    Quantidade INTEGER NOT NULL,
    PrecoUnitario REAL NOT NULL,
    PrecoTotal REAL NOT NULL,
    DataVenda TEXT NOT NULL DEFAULT (datetime('now')),
    Responsavel TEXT,
    Observacoes TEXT,
    TipoMovimento TEXT NOT NULL DEFAULT 'Venda',
    FOREIGN KEY (ComboId) REFERENCES Combos(Id) ON DELETE RESTRICT,
    FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id) ON DELETE RESTRICT
);

-- ========================================
-- Table: __EFMigrationsHistory
-- ========================================
CREATE TABLE __EFMigrationsHistory (
    MigrationId TEXT PRIMARY KEY,
    ProductVersion TEXT NOT NULL
);

-- ============================================================
-- SEED DATA
-- ============================================================

-- SaldoConfig (REQUIRED - single config row, always Id = 1)
INSERT INTO SaldoConfigs (Id, CapitalAdmin, AtualizadoEm)
VALUES (1, 0.00, datetime('now'));

-- Admin User (REQUIRED)
-- Password: admin123 (hashed with BCrypt)
INSERT INTO Usuarios (Id, Nome, Username, PasswordHash, Role, Ativo, CriadoEm)
VALUES (
    1,
    'Administrador',
    'admin',
    '$2a$11$8vZ7YQfyVZxH5L3qN9X8/.rKZJ5mHJxGxN8fQXqZ5vZ7YQfyVZxH5',
    'admin',
    1,
    datetime('now')
);

-- Migration History
INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion)
VALUES ('20240101000000_InitialCreate', '8.0.0');

-- ============================================================
-- VERIFICATION
-- ============================================================

SELECT '=== Database Setup Complete ===' as Status;
SELECT 'Tables Created:' as Info;
SELECT name FROM sqlite_master WHERE type='table' ORDER BY name;

SELECT '' as Separator;
SELECT 'Row Counts:' as Info;
SELECT 'Usuarios' as Table, COUNT(*) as Count FROM Usuarios
UNION ALL
SELECT 'Produtos', COUNT(*) FROM Produtos
UNION ALL
SELECT 'SaldoConfigs', COUNT(*) FROM SaldoConfigs
UNION ALL
SELECT 'Movimentacoes', COUNT(*) FROM Movimentacoes
UNION ALL
SELECT 'Despesas', COUNT(*) FROM Despesas
UNION ALL
SELECT 'Combos', COUNT(*) FROM Combos
UNION ALL
SELECT 'ComboComposicoes', COUNT(*) FROM ComboComposicoes
UNION ALL
SELECT 'ComboVendas', COUNT(*) FROM ComboVendas;

SELECT '' as Separator;
SELECT 'SaldoConfig Data:' as Info;
SELECT * FROM SaldoConfigs;

SELECT '' as Separator;
SELECT 'Admin User:' as Info;
SELECT Id, Nome, Username, Role, Ativo FROM Usuarios WHERE Role = 'admin';

SELECT '' as Separator;
SELECT '=== Setup Successful ===' as Status;
SELECT 'You can now start the backend and login with:' as Info;
SELECT '  Username: admin' as Credentials;
SELECT '  Password: admin123' as Credentials;
