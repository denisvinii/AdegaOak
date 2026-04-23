import { Router, type IRouter } from "express";
import { pool, asInt } from "../lib/db";

const router: IRouter = Router();

const SELECT_BASE = `
  SELECT
    m.id, m.data, m.tipo, m.tipo_venda, m.productid, m.produto, m.quantidade,
    m.responsavel, m.saida,
    m.valor_unitario::float8 AS valor_unitario,
    (m.valor_unitario * m.quantidade)::float8 AS valor_total,
    m.observacoes
  FROM movimentacoes m
`;

router.get("/movimentacoes", async (req, res, next) => {
  try {
    const tipo = req.query.tipo as string | undefined;
    const from = req.query.from as string | undefined;
    const to = req.query.to as string | undefined;
    const productid = asInt(req.query.productid);
    const limit = asInt(req.query.limit, 200) ?? 200;
    const where: string[] = []; const args: unknown[] = [];
    if (tipo && tipo !== "All") { args.push(tipo); where.push(`m.tipo = $${args.length}`); }
    if (from) { args.push(from); where.push(`m.data >= $${args.length}`); }
    if (to)   { args.push(to);   where.push(`m.data <= $${args.length}`); }
    if (productid !== undefined) { args.push(productid); where.push(`m.productid = $${args.length}`); }
    const sql = `${SELECT_BASE} ${where.length ? "WHERE " + where.join(" AND ") : ""} ORDER BY m.data DESC LIMIT ${limit}`;
    const r = await pool.query(sql, args);
    res.json(r.rows);
  } catch (e) { next(e); }
});

router.post("/movimentacoes", async (req, res, next) => {
  try {
    const b = req.body ?? {};
    const tipo = b.tipo;
    const tipo_venda = b.tipo_venda ?? "Varejo";
    const productid = Number(b.productid);
    const quantidade = Number(b.quantidade);
    const responsavel = String(b.responsavel ?? "");
    const valor_unitario = Number(b.valor_unitario);
    const saida = b.saida ?? null;
    const observacoes = b.observacoes ?? null;

    if (!["Entrada","Saída"].includes(tipo)) return res.status(400).json({ error: "tipo inválido" });
    if (!Number.isFinite(productid) || quantidade <= 0) return res.status(400).json({ error: "produto/quantidade inválidos" });

    const prod = await pool.query(`SELECT bebida, tamanho FROM estoque WHERE productid = $1`, [productid]);
    if (!prod.rows[0]) return res.status(404).json({ error: "Produto não encontrado" });
    const produto = `${prod.rows[0].bebida} ${prod.rows[0].tamanho}`.trim();

    const r = await pool.query(
      `INSERT INTO movimentacoes
        (data, tipo, tipo_venda, productid, produto, quantidade, responsavel, saida, valor_unitario, observacoes)
       VALUES (NOW(), $1,$2,$3,$4,$5,$6,$7,$8,$9) RETURNING id`,
      [tipo, tipo_venda, productid, produto, quantidade, responsavel, saida, valor_unitario, observacoes],
    );
    const detail = await pool.query(`${SELECT_BASE} WHERE m.id = $1`, [r.rows[0].id]);
    res.status(201).json(detail.rows[0]);
  } catch (e) { next(e); }
});

router.delete("/movimentacoes/:id", async (req, res, next) => {
  try {
    await pool.query(`DELETE FROM movimentacoes WHERE id = $1`, [Number(req.params.id)]);
    res.status(204).end();
  } catch (e) { next(e); }
});

export default router;
