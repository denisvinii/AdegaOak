-- Consulta de saldo (controle de saldo) - versão compatível com SQLite

-- saldo
WITH controledesaldo AS (
    SELECT
        SUM(CASE WHEN tipo = 'Saída' THEN valor_unitario * quantidade ELSE 0 END) AS total_saidas,
        SUM(CASE WHEN tipo = 'Entrada' THEN valor_unitario * quantidade ELSE 0 END) AS total_entradas
    FROM movimentacoes
    WHERE responsavel <> 'admin'
)
SELECT (total_saidas - total_entradas) AS saldo, total_saidas, total_entradas FROM controledesaldo;

-- Saldo admin (investido por fora)
SELECT SUM(valor_unitario * quantidade) AS investidoporfora FROM movimentacoes WHERE responsavel = 'admin';

-- CAPITAL TOTAL DA EMPRESA ATUAL
WITH controledesaldo AS (
    SELECT
        SUM(CASE WHEN tipo = 'Saída' THEN valor_unitario * quantidade ELSE 0 END) AS total_saidas,
        SUM(CASE WHEN tipo = 'Entrada' THEN valor_unitario * quantidade ELSE 0 END) AS total_entradas
    FROM movimentacoes
    WHERE responsavel <> 'admin'
)
SELECT
    SUM(m.valor_unitario * m.quantidade) + (c.total_saidas - c.total_entradas) AS capitalempresa,
    SUM(m.valor_unitario * m.quantidade) AS investidoporfora,
    (c.total_saidas - c.total_entradas) AS saldo
FROM movimentacoes m
CROSS JOIN controledesaldo c
WHERE m.responsavel = 'admin';



