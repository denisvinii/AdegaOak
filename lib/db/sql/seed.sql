-- Optional: sample catalog with typical adega items.
-- Run only on a brand-new database; will not duplicate if products already exist.

INSERT INTO estoque (bebida, tamanho, material, valor, valor_venda, quantidade_caixa, valor_caixa, valor_atacado_caixa, estoque_minimo, quantidade_minima_atacado)
SELECT * FROM (VALUES
    ('Brahma',           '600ml', 'Vidro Retornável',  6.50, 10.00, 12,  78.00, 120.00, 24, 24),
    ('Skol',             '600ml', 'Vidro Retornável',  6.50, 10.00, 12,  78.00, 120.00, 24, 24),
    ('Heineken',         '600ml', 'Vidro Retornável', 11.00, 18.00, 12, 132.00, 216.00, 12, 24),
    ('Coca-Cola',        '350ml', 'Lata',              2.50,  4.50, 12,  30.00,  54.00, 24, 48),
    ('Coca-Cola',        '2L',    'Pet',               6.50, 10.00,  6,  39.00,  60.00, 12, 24),
    ('Guaraná Antarctica','2L',   'Pet',               5.50,  9.00,  6,  33.00,  54.00, 12, 24),
    ('Água Mineral',     '500ml', 'Pet',               1.20,  3.00, 12,  14.40,  36.00, 24, 48),
    ('Energético Red Bull','250ml','Lata',             6.50, 12.00, 24, 156.00, 288.00, 24, 24),
    ('Whisky Black Label','1L',   'Vidro',           180.00, 280.00, 1,  180.00, 180.00,  3,  6),
    ('Whisky Red Label', '1L',    'Vidro',            70.00, 110.00, 1,  70.00,  70.00,  3,  6),
    ('Vinho Tinto Suave','750ml', 'Vidro',            18.00,  30.00, 6, 108.00, 162.00,  6, 12),
    ('Gelo',             '5kg',   'Saco',              8.00,  15.00, 1,   8.00,  15.00, 10, 20),
    ('Carvão',           '3kg',   'Saco',             12.00,  22.00, 1,  12.00,  22.00, 10, 20),
    ('Cigarro Marlboro', 'Maço',  'Pacote',            9.00,  12.00, 10,  90.00, 100.00, 20, 50),
    ('Refresco em Pó',   '25g',   'Sachê',             0.80,   2.00, 24,  19.20,  36.00, 24, 48)
) AS t(bebida, tamanho, material, valor, valor_venda, quantidade_caixa, valor_caixa, valor_atacado_caixa, estoque_minimo, quantidade_minima_atacado)
WHERE NOT EXISTS (SELECT 1 FROM estoque);
