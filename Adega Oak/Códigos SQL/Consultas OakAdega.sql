DROP TABLE teste

SELECT * FROM movimentacoes


SELECT SUM(valor_unitario*quantidade) FROM movimentacoes where tipo = 'Entrada'
SELECT * FROM  estoque 

3417.15

SELECT COUNT(1) FROM estoque 
--WHERE productid NOT IN (SELECT productid FROM estoque
HAVING COUNT(1) > 0

SELECT m.data, m.tipo, m.productid, m.produto, m.quantidade, m.responsavel, m.saida,
       COALESCE(e.valor_venda, 0) as valor
FROM movimentacoes m
LEFT JOIN estoque e ON e.productid = m.productid
ORDER BY m.data DESC


SELECT COALESCE(SUM(CASE WHEN m.tipo = 'Saída' THEN (m.quantidade * e.valor_venda) ELSE 0 END), 0) 
    - COALESCE(SUM(CASE WHEN m.tipo = 'Saída' THEN (m.quantidade * e.valor) ELSE 0 END), 0),
	COALESCE(SUM(CASE WHEN m.tipo = 'Saída' THEN (m.quantidade * e.valor_venda) ELSE 0 END), 0),
	COALESCE(SUM(CASE WHEN m.tipo = 'Saída' THEN (m.quantidade * e.valor_venda) ELSE 0 END), 0) * 100 / COALESCE(SUM(CASE WHEN m.tipo = 'Saída' THEN (m.quantidade * e.valor) ELSE 0 END), 0) - 100
FROM movimentacoes m
INNER JOIN estoque e ON m.productid = e.productid

SELECT * FROM movimentacoes
SELECT * FROM estoque