import { Router, type IRouter } from "express";
import { pool, asBool } from "../lib/db";

const router: IRouter = Router();

const SELECT = `
  SELECT id, descricao, valor::float8 AS valor, data, tipo, pago, data_pagamento, notas
  FROM despesas
`;

router.get("/despesas", async (req, res, next) => {
  try {
    const pago = asBool(req.query.pago);
    const from = req.query.from as string | undefined;
    const to = req.query.to as string | undefined;
    const args: unknown[] = []; const where: string[] = [];
    if (pago !== undefined) { args.push(pago); where.push(`pago = $${args.length}`); }
    if (from) { args.push(from); where.push(`data >= $${args.length}`); }
    if (to)   { args.push(to);   where.push(`data <= $${args.length}`); }
    const r = await pool.query(`${SELECT} ${where.length ? "WHERE " + where.join(" AND ") : ""} ORDER BY data DESC`, args);
    res.json(r.rows);
  } catch (e) { next(e); }
});

router.post("/despesas", async (req, res, next) => {
  try {
    const b = req.body ?? {};
    const r = await pool.query(
      `INSERT INTO despesas (descricao, valor, data, tipo, pago, data_pagamento, notas, criado_em)
       VALUES ($1,$2,$3,$4,$5,$6,$7,NOW()) RETURNING id`,
      [b.descricao, b.valor, b.data, b.tipo ?? 0, b.pago ?? false, b.data_pagamento ?? null, b.notas ?? null],
    );
    const d = await pool.query(`${SELECT} WHERE id = $1`, [r.rows[0].id]);
    res.status(201).json(d.rows[0]);
  } catch (e) { next(e); }
});

router.patch("/despesas/:id", async (req, res, next) => {
  try {
    const id = Number(req.params.id);
    const b = req.body ?? {};
    const sets: string[] = []; const args: unknown[] = [];
    for (const k of ["descricao","valor","data","tipo","pago","data_pagamento","notas"] as const) {
      if (b[k] !== undefined) { args.push(b[k]); sets.push(`${k} = $${args.length}`); }
    }
    if (sets.length) {
      args.push(id);
      await pool.query(`UPDATE despesas SET ${sets.join(", ")} WHERE id = $${args.length}`, args);
    }
    const d = await pool.query(`${SELECT} WHERE id = $1`, [id]);
    if (!d.rows[0]) return res.status(404).json({ error: "Despesa não encontrada" });
    res.json(d.rows[0]);
  } catch (e) { next(e); }
});

router.delete("/despesas/:id", async (req, res, next) => {
  try {
    await pool.query(`DELETE FROM despesas WHERE id = $1`, [Number(req.params.id)]);
    res.status(204).end();
  } catch (e) { next(e); }
});

export default router;
