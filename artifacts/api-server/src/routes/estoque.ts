import { Router, type IRouter } from "express";
import { pool, asBool } from "../lib/db";

const router: IRouter = Router();

const SELECT_BASE = `
  SELECT
    e.productid, e.bebida, e.tamanho, e.material,
    e.valor::float8 AS valor,
    e.valor_venda::float8 AS valor_venda,
    e.quantidade_caixa,
    e.valor_caixa::float8 AS valor_caixa,
    e.valor_atacado_caixa::float8 AS valor_atacado_caixa,
    e.estoque_minimo,
    e.quantidade_minima_atacado,
    e.ativo,
    COALESCE((
      SELECT SUM(CASE WHEN m.tipo = 'Entrada' THEN m.quantidade
                      WHEN m.tipo = 'Saída'   THEN -m.quantidade
                      ELSE 0 END)
      FROM movimentacoes m WHERE m.productid = e.productid
    ), 0)::int AS quantidade
  FROM estoque e
`;

router.get("/estoque", async (req, res, next) => {
  try {
    const search = (req.query.search as string | undefined)?.trim();
    const low = asBool(req.query.low);
    const args: unknown[] = [];
    const where: string[] = ["COALESCE(e.ativo, true) = true"];
    if (search) {
      args.push(`%${search}%`);
      where.push(`(e.bebida ILIKE $${args.length} OR e.tamanho ILIKE $${args.length} OR e.material ILIKE $${args.length})`);
    }
    let sql = `${SELECT_BASE} WHERE ${where.join(" AND ")} ORDER BY e.bebida, e.tamanho`;
    if (low) {
      sql = `SELECT * FROM (${sql}) sub WHERE quantidade <= sub.estoque_minimo`;
    }
    const r = await pool.query(sql, args);
    res.json(r.rows);
  } catch (e) { next(e); }
});

router.get("/estoque/:productid", async (req, res, next) => {
  try {
    const id = Number(req.params.productid);
    const r = await pool.query(`${SELECT_BASE} WHERE e.productid = $1`, [id]);
    if (!r.rows[0]) return res.status(404).json({ error: "Produto não encontrado" });
    res.json(r.rows[0]);
  } catch (e) { next(e); }
});

router.post("/estoque", async (req, res, next) => {
  try {
    const b = req.body ?? {};
    const r = await pool.query(
      `INSERT INTO estoque
        (bebida, tamanho, material, valor, valor_venda, quantidade_caixa, valor_caixa, valor_atacado_caixa, estoque_minimo, quantidade_minima_atacado, ativo, criado_em)
       VALUES ($1,$2,$3,$4,$5,$6,$7,$8,$9,$10,true, NOW())
       RETURNING productid`,
      [b.bebida, b.tamanho, b.material, b.valor, b.valor_venda,
       b.quantidade_caixa ?? 1, b.valor_caixa ?? 0, b.valor_atacado_caixa ?? 0,
       b.estoque_minimo ?? 0, b.quantidade_minima_atacado ?? 20],
    );
    const detail = await pool.query(`${SELECT_BASE} WHERE e.productid = $1`, [r.rows[0].productid]);
    res.status(201).json(detail.rows[0]);
  } catch (e) { next(e); }
});

router.patch("/estoque/:productid", async (req, res, next) => {
  try {
    const id = Number(req.params.productid);
    const b = req.body ?? {};
    const fields: string[] = [];
    const args: unknown[] = [];
    const allow = ["bebida","tamanho","material","valor","valor_venda","quantidade_caixa","valor_caixa","valor_atacado_caixa","estoque_minimo","quantidade_minima_atacado"];
    for (const k of allow) {
      if (b[k] !== undefined) { args.push(b[k]); fields.push(`${k} = $${args.length}`); }
    }
    if (fields.length) {
      args.push(id);
      await pool.query(`UPDATE estoque SET ${fields.join(", ")} WHERE productid = $${args.length}`, args);
    }
    const detail = await pool.query(`${SELECT_BASE} WHERE e.productid = $1`, [id]);
    if (!detail.rows[0]) return res.status(404).json({ error: "Produto não encontrado" });
    res.json(detail.rows[0]);
  } catch (e) { next(e); }
});

router.delete("/estoque/:productid", async (req, res, next) => {
  try {
    const id = Number(req.params.productid);
    await pool.query(`UPDATE estoque SET ativo = false WHERE productid = $1`, [id]);
    res.status(204).end();
  } catch (e) { next(e); }
});

export default router;
