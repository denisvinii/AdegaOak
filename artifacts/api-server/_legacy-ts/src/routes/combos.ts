import { Router, type IRouter } from "express";
import { pool } from "../lib/db";

const router: IRouter = Router();

async function loadCombo(combo_id: number) {
  const c = await pool.query(`
    SELECT combo_id, nome, descricao, preco_venda::float8 AS preco_venda, ativo, data_criacao
    FROM combos WHERE combo_id = $1`, [combo_id]);
  if (!c.rows[0]) return null;
  const comp = await pool.query(`
    SELECT cc.composicao_id, cc.product_id,
           (e.bebida || ' ' || COALESCE(e.tamanho,'')) AS produto,
           cc.quantidade::float8 AS quantidade, cc.unidade, cc.debita_estoque,
           e.valor::float8 AS custo_unitario
    FROM combo_composicao cc
    LEFT JOIN estoque e ON e.productid = cc.product_id
    WHERE cc.combo_id = $1`, [combo_id]);
  const composicao = comp.rows.map((r) => ({
    composicao_id: r.composicao_id,
    product_id: r.product_id,
    produto: r.produto,
    quantidade: Number(r.quantidade),
    unidade: r.unidade,
    debita_estoque: r.debita_estoque,
  }));
  const custo_total = comp.rows.reduce((s, r) => s + Number(r.custo_unitario ?? 0) * Number(r.quantidade ?? 0), 0);
  return { ...c.rows[0], composicao, custo_total };
}

router.get("/combos", async (_req, res, next) => {
  try {
    const r = await pool.query(`SELECT combo_id FROM combos ORDER BY nome`);
    const out = await Promise.all(r.rows.map((row) => loadCombo(row.combo_id)));
    res.json(out.filter(Boolean));
  } catch (e) { next(e); }
});

router.get("/combos/:combo_id", async (req, res, next) => {
  try {
    const c = await loadCombo(Number(req.params.combo_id));
    if (!c) return res.status(404).json({ error: "Combo não encontrado" });
    res.json(c);
  } catch (e) { next(e); }
});

router.post("/combos", async (req, res, next) => {
  const client = await pool.connect();
  try {
    const b = req.body ?? {};
    await client.query("BEGIN");
    const c = await client.query(
      `INSERT INTO combos (nome, descricao, preco_venda, ativo, data_criacao)
       VALUES ($1,$2,$3,$4,NOW()) RETURNING combo_id`,
      [b.nome, b.descricao ?? null, b.preco_venda, b.ativo ?? true],
    );
    const combo_id = c.rows[0].combo_id;
    for (const it of (b.composicao ?? []) as Array<any>) {
      await client.query(
        `INSERT INTO combo_composicao (combo_id, product_id, quantidade, unidade, debita_estoque)
         VALUES ($1,$2,$3,$4,$5)`,
        [combo_id, it.product_id, it.quantidade, it.unidade, it.debita_estoque ?? true],
      );
    }
    await client.query("COMMIT");
    res.status(201).json(await loadCombo(combo_id));
  } catch (e) {
    await client.query("ROLLBACK").catch(() => {});
    next(e);
  } finally { client.release(); }
});

router.patch("/combos/:combo_id", async (req, res, next) => {
  const client = await pool.connect();
  try {
    const id = Number(req.params.combo_id);
    const b = req.body ?? {};
    await client.query("BEGIN");
    const sets: string[] = []; const args: unknown[] = [];
    for (const k of ["nome","descricao","preco_venda","ativo"] as const) {
      if (b[k] !== undefined) { args.push(b[k]); sets.push(`${k} = $${args.length}`); }
    }
    if (sets.length) {
      args.push(id);
      await client.query(`UPDATE combos SET ${sets.join(", ")} WHERE combo_id = $${args.length}`, args);
    }
    if (Array.isArray(b.composicao)) {
      await client.query(`DELETE FROM combo_composicao WHERE combo_id = $1`, [id]);
      for (const it of b.composicao as Array<any>) {
        await client.query(
          `INSERT INTO combo_composicao (combo_id, product_id, quantidade, unidade, debita_estoque)
           VALUES ($1,$2,$3,$4,$5)`,
          [id, it.product_id, it.quantidade, it.unidade, it.debita_estoque ?? true],
        );
      }
    }
    await client.query("COMMIT");
    res.json(await loadCombo(id));
  } catch (e) {
    await client.query("ROLLBACK").catch(() => {});
    next(e);
  } finally { client.release(); }
});

router.delete("/combos/:combo_id", async (req, res, next) => {
  try {
    const id = Number(req.params.combo_id);
    await pool.query(`DELETE FROM combo_composicao WHERE combo_id = $1`, [id]);
    await pool.query(`DELETE FROM combos WHERE combo_id = $1`, [id]);
    res.status(204).end();
  } catch (e) { next(e); }
});

export default router;
