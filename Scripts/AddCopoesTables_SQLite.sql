-- ============================================================
-- SCRIPT: Criar Tabelas de COPÕES
-- Banco de Dados: SQLite (Local)
-- ============================================================

-- ============================================================
-- 1. TABELA: copoes
-- ============================================================
CREATE TABLE IF NOT EXISTS copoes (
    copao_id INTEGER PRIMARY KEY AUTOINCREMENT,
    nome TEXT NOT NULL UNIQUE,
    descricao TEXT,
    preco_venda DECIMAL(10,2) NOT NULL,
    ativo BOOLEAN DEFAULT 1,
    data_criacao TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- ============================================================
-- 2. TABELA: copao_composicao
-- ============================================================
CREATE TABLE IF NOT EXISTS copao_composicao (
    composicao_id INTEGER PRIMARY KEY AUTOINCREMENT,
    copao_id INTEGER NOT NULL,
    product_id INTEGER NOT NULL,
    quantidade DECIMAL(10,4) NOT NULL,
    unidade TEXT NOT NULL,
    debita_estoque BOOLEAN DEFAULT 0,
    FOREIGN KEY (copao_id) REFERENCES copoes(copao_id) ON DELETE CASCADE,
    FOREIGN KEY (product_id) REFERENCES estoque(productid) ON DELETE RESTRICT
);

CREATE INDEX IF NOT EXISTS idx_copao_composicao_copao 
ON copao_composicao(copao_id);

CREATE INDEX IF NOT EXISTS idx_copao_composicao_product 
ON copao_composicao(product_id);

-- ============================================================
-- 3. TABELA: copao_vendas (Histórico)
-- ============================================================
CREATE TABLE IF NOT EXISTS copao_vendas (
    venda_id INTEGER PRIMARY KEY AUTOINCREMENT,
    copao_id INTEGER NOT NULL,
    quantidade INTEGER NOT NULL,
    preco_unitario DECIMAL(10,2) NOT NULL,
    preco_total DECIMAL(10,2) NOT NULL,
    data_venda TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    responsavel TEXT,
    observacoes TEXT,
    FOREIGN KEY (copao_id) REFERENCES copoes(copao_id)
);

CREATE INDEX IF NOT EXISTS idx_copao_vendas_copao 
ON copao_vendas(copao_id);

CREATE INDEX IF NOT EXISTS idx_copao_vendas_data 
ON copao_vendas(data_venda);

-- ============================================================
-- 4. VERIFICAÇÃO
-- ============================================================
SELECT 'Tabelas criadas com sucesso!' as status;
