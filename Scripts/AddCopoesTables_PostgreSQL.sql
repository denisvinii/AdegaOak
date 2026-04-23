-- ============================================================
-- SCRIPT: Criar Tabelas de COPÕES
-- Banco de Dados: PostgreSQL (Aiven)
-- ============================================================

-- ============================================================
-- 1. TABELA: copoes
-- ============================================================
CREATE TABLE IF NOT EXISTS copoes (
    copao_id SERIAL PRIMARY KEY,
    nome VARCHAR(100) NOT NULL UNIQUE,
    descricao VARCHAR(255),
    preco_venda DECIMAL(10,2) NOT NULL,
    ativo BOOLEAN DEFAULT true,
    data_criacao TIMESTAMP DEFAULT NOW()
);

COMMENT ON TABLE copoes IS 'Tabela de Copões (combos). Ex: Copão Caipirinha';
COMMENT ON COLUMN copoes.nome IS 'Nome único do copão. Ex: "Copão Caipirinha"';
COMMENT ON COLUMN copoes.preco_venda IS 'Preço de venda do copão completo';

-- ============================================================
-- 2. TABELA: copao_composicao
-- ============================================================
CREATE TABLE IF NOT EXISTS copao_composicao (
    composicao_id SERIAL PRIMARY KEY,
    copao_id INT NOT NULL,
    product_id INT NOT NULL,
    quantidade DECIMAL(10,4) NOT NULL,
    unidade VARCHAR(20) NOT NULL,
    debita_estoque BOOLEAN DEFAULT false,
    FOREIGN KEY (copao_id) REFERENCES copoes(copao_id) ON DELETE CASCADE,
    FOREIGN KEY (product_id) REFERENCES estoque(productid) ON DELETE RESTRICT
);

COMMENT ON TABLE copao_composicao IS 'Composição dos produtos que formam cada copão';
COMMENT ON COLUMN copao_composicao.quantidade IS 'Quantidade do produto. Ex: 0.05 (litros), 1 (unidade)';
COMMENT ON COLUMN copao_composicao.unidade IS 'Unidade de medida: L, ml, un, g';
COMMENT ON COLUMN copao_composicao.debita_estoque IS 'Se true, debita do estoque ao vender (ex: Gelo). Se false, apenas registro informativo (ex: Gim, Energético)';

CREATE INDEX IF NOT EXISTS idx_copao_composicao_copao 
ON copao_composicao(copao_id);

CREATE INDEX IF NOT EXISTS idx_copao_composicao_product 
ON copao_composicao(product_id);

-- ============================================================
-- 3. TABELA: copao_vendas (Histórico)
-- ============================================================
CREATE TABLE IF NOT EXISTS copao_vendas (
    venda_id SERIAL PRIMARY KEY,
    copao_id INT NOT NULL,
    quantidade INT NOT NULL,
    preco_unitario DECIMAL(10,2) NOT NULL,
    preco_total DECIMAL(10,2) NOT NULL,
    data_venda TIMESTAMP DEFAULT NOW(),
    responsavel VARCHAR(100),
    observacoes VARCHAR(255),
    FOREIGN KEY (copao_id) REFERENCES copoes(copao_id)
);

COMMENT ON TABLE copao_vendas IS 'Histórico de vendas de copões';
COMMENT ON COLUMN copao_vendas.quantidade IS 'Quantos copões foram vendidos nesta transação';
COMMENT ON COLUMN copao_vendas.preco_unitario IS 'Preço unitário do copão na data da venda';
COMMENT ON COLUMN copao_vendas.preco_total IS 'Total = quantidade × preco_unitario';

CREATE INDEX IF NOT EXISTS idx_copao_vendas_copao 
ON copao_vendas(copao_id);

CREATE INDEX IF NOT EXISTS idx_copao_vendas_data 
ON copao_vendas(data_venda);

-- ============================================================
-- 4. VERIFICAÇÃO
-- ============================================================
SELECT 
    'Tabelas criadas com sucesso!' as status,
    NOW() as data_execucao;
