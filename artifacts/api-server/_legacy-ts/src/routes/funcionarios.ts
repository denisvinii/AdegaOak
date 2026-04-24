import { Router, type IRouter } from "express";
import { pool } from "../lib/db";

const router: IRouter = Router();

router.get("/funcionarios", async (_req, res, next) => {
  try {
    const r = await pool.query(`SELECT id, username, COALESCE(ativo, true) AS ativo FROM funcionarios WHERE COALESCE(ativo, true) = true ORDER BY username`);
    res.json(r.rows);
  } catch (e) { next(e); }
});

router.post("/funcionarios", async (req, res, next) => {
  try {
    const username = String(req.body?.username ?? "").trim();
    if (username.length < 2) return res.status(400).json({ error: "username inválido" });
    const r = await pool.query(
      `INSERT INTO funcionarios (username, ativo, criado_em) VALUES ($1, true, NOW()) RETURNING id, username, ativo`,
      [username],
    );
    res.status(201).json(r.rows[0]);
  } catch (e) { next(e); }
});

router.delete("/funcionarios/:id", async (req, res, next) => {
  try {
    await pool.query(`UPDATE funcionarios SET ativo = false WHERE id = $1`, [Number(req.params.id)]);
    res.status(204).end();
  } catch (e) { next(e); }
});

export default router;
