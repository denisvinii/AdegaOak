-- ============================================================
-- SCRIPT COMPLETO: Adicionar Coluna valor_atacado_caixa
-- Banco de Dados: PostgreSQL (Aiven)
-- Data: 2024
-- ============================================================

-- ============================================================
-- 1. ADICIONAR COLUNA PRINCIPAL
-- ============================================================
ALTER TABLE IF EXISTS estoque
ADD COLUMN IF NOT EXISTS valor_atacado_caixa DECIMAL(10,2) DEFAULT 0;

-- ============================================================
-- 2. ADICIONAR COMENTÁRIO (Documentação)
-- ============================================================
COMMENT ON COLUMN estoque.valor_atacado_caixa IS 
'Valor especial da caixa para vendas em atacado (requer mínimo 20 caixas).
Se deixado em branco (0), o sistema usa o preço normal de caixa.
Recomendação: Aplicar 10-20% de desconto para incentivar vendas em volume.';

-- ============================================================
-- 3. CRIAR ÍNDICE (Performance)
-- ============================================================
CREATE INDEX IF NOT EXISTS idx_estoque_valor_atacado 
ON estoque(valor_atacado_caixa);

-- ============================================================
-- 4. CRIAR ÍNDICE COMPOSTO (Performance avançada)
-- ============================================================
CREATE INDEX IF NOT EXISTS idx_estoque_valor_atacado_produto 
ON estoque(productid, valor_atacado_caixa);

-- ============================================================
-- 5. VERIFICAR SE COLUNA FOI CRIADA
-- ============================================================
SELECT 
    column_name, 
    data_type, 
    is_nullable, 
    column_default,
    EXISTS(SELECT 1 FROM information_schema.columns 
           WHERE table_name = 'estoque' 
           AND column_name = 'valor_atacado_caixa') as existe
FROM information_schema.columns
WHERE table_name = 'estoque' AND column_name = 'valor_atacado_caixa';

-- ============================================================
-- 6. ATUALIZAR VALORES PADRÃO (OPCIONAL)
-- ============================================================
-- Se quiser dar um desconto padrão de 15% em todos os produtos
-- Descomente a linha abaixo e adapte conforme necessário:

-- UPDATE estoque 
-- SET valor_atacado_caixa = ROUND(valor_caixa * 0.85, 2)
-- WHERE valor_atacado_caixa = 0 AND valor_caixa > 0;

-- ============================================================
-- 7. VERIFICAR DADOS ATUALIZADOS
-- ============================================================
SELECT 
    productid,
    bebida,
    tamanho,
    material,
    quantidade_caixa,
    valor_caixa as valor_caixa_varejo,
    valor_atacado_caixa as valor_caixa_atacado,
    CASE 
        WHEN valor_atacado_caixa > 0 THEN 
            ROUND((1 - (valor_atacado_caixa / valor_caixa)) * 100, 1)
        ELSE 0 
    END as desconto_percentual
FROM estoque
ORDER BY bebida, tamanho
LIMIT 20;

-- ============================================================
-- 8. ESTATÍSTICAS (Informações úteis)
-- ============================================================
SELECT 
    COUNT(*) as total_produtos,
    COUNT(CASE WHEN valor_atacado_caixa > 0 THEN 1 END) as com_preco_atacado,
    COUNT(CASE WHEN valor_atacado_caixa = 0 THEN 1 END) as sem_preco_atacado,
    ROUND(AVG(valor_caixa)::numeric, 2) as media_preco_varejo,
    ROUND(AVG(valor_atacado_caixa)::numeric, 2) as media_preco_atacado,
    ROUND(MIN(valor_caixa)::numeric, 2) as min_preco_varejo,
    ROUND(MAX(valor_caixa)::numeric, 2) as max_preco_varejo
FROM estoque
WHERE valor_caixa > 0;

-- ============================================================
-- 9. VALIDAR INTEGRIDADE
-- ============================================================
-- Verificar se há produtos com preço de atacado mas sem preço de caixa
SELECT 
    productid,
    bebida,
    valor_caixa,
    valor_atacado_caixa
FROM estoque
WHERE valor_atacado_caixa > 0 AND valor_caixa = 0;

-- Verificar se há preço de atacado MAIOR que preço de varejo (erro!)
SELECT 
    productid,
    bebida,
    valor_caixa as varejo,
    valor_atacado_caixa as atacado
FROM estoque
WHERE valor_atacado_caixa > valor_caixa;

-- ============================================================
-- 10. BACKUP ANTES DE FAZER GRANDES MUDANÇAS (RECOMENDADO)
-- ============================================================
-- Faça um backup da tabela antes:
-- pg_dump -h seu_host -U seu_usuario -d seu_banco -t estoque > estoque_backup.sql

-- ============================================================
-- FINAL: Verificar que tudo está OK
-- ============================================================
SELECT 
    'Status: ? Coluna valor_atacado_caixa adicionada com sucesso!' as status,
    NOW() as data_execucao;

-- ============================================================
-- NOTAS IMPORTANTES
-- ============================================================
/*
1. SINCRONIZAÇÃO: 
   - Após executar este script, a aplicação sincronizará 
     a coluna para o SQLite local automaticamente

2. CONFIGURAÇÃO:
   - Acesse "Tabela de Preços" na aplicação
   - Preencha a coluna "Valor Atacado Caixa" para cada produto
   - Se deixar em branco (0), o sistema usa preço normal

3. REGRA DE NEGÓCIO:
   - Mínimo de 20 caixas para usar preço de atacado
   - Recomenda-se desconto de 10-20% para atacado

4. MONITORAMENTO:
   - Execute a query #7 periodicamente para ver estatísticas
   - Use query #9 para validar erros de integridade

5. ROLLBACK (Se precisar reverter):
   ALTER TABLE estoque DROP COLUMN valor_atacado_caixa;
*/

-- ============================================================
-- FIM DO SCRIPT
-- ============================================================
