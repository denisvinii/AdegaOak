import { Router, type IRouter } from "express";
import { pool, asInt } from "../lib/db";

const router: IRouter = Router();

router.get("/dashboard/saldo", async (_req, res, next) => {
  try {
    const r = await pool.query(`
      WITH agg AS (
        SELECT
          COALESCE(SUM(CASE WHEN tipo='Entrada' THEN quantidade*valor_unitario END),0)::float8 AS total_entradas,
          COALESCE(SUM(CASE WHEN tipo='Saída'   THEN quantidade*valor_unitario END),0)::float8 AS total_saidas,
          COALESCE(SUM(CASE WHEN tipo='Entrada' AND LOWER(COALESCE(responsavel,''))='admin' THEN quantidade*valor_unitario END),0)::float8 AS investido_admin
        FROM movimentacoes
      ),
      lucro_calc AS (
        SELECT COALESCE(SUM(
          m.quantidade * (m.valor_unitario - COALESCE(e.valor,0))
        ),0)::float8 AS lucro
        FROM movimentacoes m JOIN estoque e ON e.productid = m.productid
        WHERE m.tipo='Saída'
      )
      SELECT a.total_entradas, a.total_saidas,
             (a.total_saidas - (a.total_entradas - a.investido_admin))::float8 AS saldo,
             a.investido_admin,
             (a.total_saidas - a.total_entradas + a.investido_admin)::float8 AS capital_empresa,
             l.lucro,
             CASE WHEN a.total_saidas > 0 THEN (l.lucro / a.total_saidas * 100)::float8 ELSE 0 END AS margem_percentual
      FROM agg a, lucro_calc l
    `);
    res.json(r.rows[0]);
  } catch (e) { next(e); }
});

router.get("/dashboard/overview", async (_req, res, next) => {
  try {
    const r = await pool.query(`
      WITH v AS (
        SELECT
          COALESCE(SUM(CASE WHEN data::date = CURRENT_DATE THEN quantidade*valor_unitario END),0)::float8 AS vendas_hoje,
          COALESCE(SUM(CASE WHEN data >= NOW() - INTERVAL '7 days' THEN quantidade*valor_unitario END),0)::float8 AS vendas_semana,
          COALESCE(SUM(CASE WHEN data >= NOW() - INTERVAL '30 days' THEN quantidade*valor_unitario END),0)::float8 AS vendas_mes
        FROM movimentacoes WHERE tipo='Saída'
      ),
      p AS (
        SELECT COUNT(*)::int AS total_produtos,
               COUNT(*) FILTER (WHERE COALESCE((
                 SELECT SUM(CASE WHEN tipo='Entrada' THEN quantidade WHEN tipo='Saída' THEN -quantidade END)
                 FROM movimentacoes m WHERE m.productid = e.productid),0) <= e.estoque_minimo)::int AS produtos_baixo_estoque,
               COALESCE(SUM(
                 GREATEST(COALESCE((SELECT SUM(CASE WHEN tipo='Entrada' THEN quantidade WHEN tipo='Saída' THEN -quantidade END)
                                    FROM movimentacoes m WHERE m.productid = e.productid),0),0) * e.valor
               ),0)::float8 AS valor_estoque
        FROM estoque e WHERE COALESCE(ativo,true) = true
      ),
      d AS (
        SELECT
          COALESCE(SUM(CASE WHEN pago = false THEN valor END),0)::float8 AS despesas_pendentes,
          COALESCE(SUM(CASE WHEN pago = true AND data >= NOW() - INTERVAL '30 days' THEN valor END),0)::float8 AS despesas_pagas_mes
        FROM despesas
      )
      SELECT v.vendas_hoje, v.vendas_semana, v.vendas_mes,
             p.total_produtos, p.produtos_baixo_estoque, p.valor_estoque,
             d.despesas_pendentes, d.despesas_pagas_mes
      FROM v, p, d
    `);
    res.json(r.rows[0]);
  } catch (e) { next(e); }
});

router.get("/dashboard/sales-trend", async (req, res, next) => {
  try {
    const days = asInt(req.query.days, 30) ?? 30;
    const r = await pool.query(`
      WITH series AS (
        SELECT generate_series(CURRENT_DATE - ($1::int - 1) * INTERVAL '1 day', CURRENT_DATE, INTERVAL '1 day')::date AS d
      ),
      agg AS (
        SELECT data::date AS d,
               SUM(quantidade * valor_unitario)::float8 AS vendas,
               SUM(quantidade * (valor_unitario - COALESCE(e.valor,0)))::float8 AS lucro,
               COUNT(*)::int AS transacoes
        FROM movimentacoes m JOIN estoque e ON e.productid = m.productid
        WHERE m.tipo='Saída' AND m.data >= CURRENT_DATE - ($1::int - 1) * INTERVAL '1 day'
        GROUP BY data::date
      )
      SELECT TO_CHAR(s.d, 'YYYY-MM-DD') AS date,
             COALESCE(a.vendas,0) AS vendas,
             COALESCE(a.lucro,0) AS lucro,
             COALESCE(a.transacoes,0) AS transacoes
      FROM series s LEFT JOIN agg a ON a.d = s.d ORDER BY s.d
    `, [days]);
    res.json(r.rows);
  } catch (e) { next(e); }
});

router.get("/dashboard/top-products", async (req, res, next) => {
  try {
    const days = asInt(req.query.days, 30) ?? 30;
    const limit = asInt(req.query.limit, 10) ?? 10;
    const r = await pool.query(`
      SELECT m.productid,
             MAX(m.produto) AS produto,
             SUM(m.quantidade)::int AS quantidade_vendida,
             SUM(m.quantidade * m.valor_unitario)::float8 AS receita
      FROM movimentacoes m
      WHERE m.tipo='Saída' AND m.data >= NOW() - ($1::int * INTERVAL '1 day')
      GROUP BY m.productid
      ORDER BY receita DESC
      LIMIT $2
    `, [days, limit]);
    res.json(r.rows);
  } catch (e) { next(e); }
});

router.get("/dashboard/recent-activity", async (req, res, next) => {
  try {
    const limit = asInt(req.query.limit, 15) ?? 15;
    const r = await pool.query(`
      (SELECT 'm-' || id AS id,
              CASE WHEN tipo='Saída' THEN 'venda' ELSE 'entrada' END AS kind,
              CASE WHEN tipo='Saída' THEN 'Venda — ' ELSE 'Entrada — ' END || produto AS title,
              COALESCE(responsavel,'') || COALESCE(' • ' || saida, '') AS subtitle,
              (quantidade * valor_unitario)::float8 AS amount,
              data AS when
       FROM movimentacoes)
      UNION ALL
      (SELECT 'd-' || id, 'despesa',
              'Despesa — ' || descricao,
              CASE tipo WHEN 0 THEN 'Operacional' WHEN 1 THEN 'Fornecedor' WHEN 2 THEN 'Salário' ELSE 'Outros' END
                || CASE WHEN pago THEN ' • Pago' ELSE ' • Pendente' END,
              valor::float8, data
       FROM despesas)
      UNION ALL
      (SELECT 'cv-' || v.venda_id, 'combo',
              'Combo — ' || COALESCE(c.nome,'?'),
              COALESCE(v.responsavel,''), v.preco_total::float8, v.data_venda
       FROM combo_vendas v LEFT JOIN combos c ON c.combo_id = v.combo_id)
      ORDER BY "when" DESC LIMIT $1
    `, [limit]);
    res.json(r.rows);
  } catch (e) { next(e); }
});

export default router;
