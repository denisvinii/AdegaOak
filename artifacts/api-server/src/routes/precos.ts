import { Router, type IRouter } from "express";
import { pool } from "../lib/db";

const router: IRouter = Router();

router.get("/precos", async (_req, res, next) => {
  try {
    const r = await pool.query(`
      SELECT productid, bebida, tamanho, material,
        valor::float8 AS valor,
        valor_venda::float8 AS valor_venda,
        valor_caixa::float8 AS valor_caixa,
        valor_atacado_caixa::float8 AS valor_atacado_caixa,
        quantidade_caixa,
        CASE WHEN valor > 0 THEN ((valor_venda - valor) / valor * 100)::float8 ELSE 0 END AS margem_percentual
      FROM estoque
      WHERE COALESCE(ativo, true) = true
      ORDER BY bebida, tamanho
    `);
    res.json(r.rows);
  } catch (e) { next(e); }
});

router.patch("/precos", async (req, res, next) => {
  const client = await pool.connect();
  try {
    const items: Array<{ productid: number; valor?: number; valor_venda?: number; valor_caixa?: number; valor_atacado_caixa?: number; }> = req.body?.items ?? [];
    let updated = 0;
    await client.query("BEGIN");
    for (const it of items) {
      const sets: string[] = []; const args: unknown[] = [];
      for (const k of ["valor","valor_venda","valor_caixa","valor_atacado_caixa"] as const) {
        if (it[k] !== undefined) { args.push(it[k]); sets.push(`${k} = $${args.length}`); }
      }
      if (!sets.length) continue;
      args.push(it.productid);
      const r = await client.query(`UPDATE estoque SET ${sets.join(", ")} WHERE productid = $${args.length}`, args);
      updated += r.rowCount ?? 0;
    }
    await client.query("COMMIT");
    res.json({ updated });
  } catch (e) {
    await client.query("ROLLBACK").catch(() => {});
    next(e);
  } finally { client.release(); }
});

export default router;
