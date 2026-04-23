import { Router, type IRouter } from "express";
import { pool } from "../lib/db";

const router: IRouter = Router();

router.post("/vendas/combo", async (req, res, next) => {
  const client = await pool.connect();
  try {
    const b = req.body ?? {};
    const combo_id = Number(b.combo_id);
    const quantidade = Number(b.quantidade);
    if (!Number.isFinite(combo_id) || quantidade <= 0) {
      return res.status(400).json({ error: "combo_id/quantidade inválidos" });
    }
    const responsavel = String(b.responsavel ?? "");
    const observacoes = b.observacoes ?? null;

    await client.query("BEGIN");
    const c = await client.query(`SELECT nome, preco_venda::float8 AS preco_venda FROM combos WHERE combo_id = $1`, [combo_id]);
    if (!c.rows[0]) { await client.query("ROLLBACK"); return res.status(404).json({ error: "Combo não encontrado" }); }
    const preco_unitario = b.preco_unitario != null ? Number(b.preco_unitario) : Number(c.rows[0].preco_venda);
    const preco_total = preco_unitario * quantidade;

    const v = await client.query(
      `INSERT INTO combo_vendas (combo_id, quantidade, preco_unitario, preco_total, data_venda, responsavel, observacoes, tipo_movimento)
       VALUES ($1,$2,$3,$4,NOW(),$5,$6,'Saida') RETURNING venda_id, data_venda`,
      [combo_id, quantidade, preco_unitario, preco_total, responsavel, observacoes],
    );

    const comp = await client.query(
      `SELECT cc.product_id, cc.quantidade::float8 AS quantidade, cc.debita_estoque,
              (e.bebida || ' ' || COALESCE(e.tamanho,'')) AS produto,
              e.valor_venda::float8 AS valor_venda
       FROM combo_composicao cc LEFT JOIN estoque e ON e.productid = cc.product_id
       WHERE cc.combo_id = $1`, [combo_id]);
    for (const it of comp.rows) {
      if (!it.debita_estoque) continue;
      const qty = Math.ceil(Number(it.quantidade) * quantidade);
      if (qty <= 0) continue;
      await client.query(
        `INSERT INTO movimentacoes (data, tipo, tipo_venda, productid, produto, quantidade, responsavel, saida, valor_unitario, observacoes)
         VALUES (NOW(),'Saída','Varejo',$1,$2,$3,$4,$5,$6,$7)`,
        [it.product_id, it.produto, qty, responsavel, `Combo #${combo_id}`, it.valor_venda ?? 0, `Venda combo ${c.rows[0].nome}`],
      );
    }

    await client.query("COMMIT");
    res.status(201).json({
      venda_id: v.rows[0].venda_id,
      combo_id, nome: c.rows[0].nome,
      quantidade, preco_unitario, preco_total,
      data_venda: v.rows[0].data_venda,
      responsavel, observacoes, tipo_movimento: "Saida",
    });
  } catch (e) {
    await client.query("ROLLBACK").catch(() => {});
    next(e);
  } finally { client.release(); }
});

router.get("/vendas/combo/list", async (req, res, next) => {
  try {
    const from = req.query.from as string | undefined;
    const to = req.query.to as string | undefined;
    const args: unknown[] = []; const where: string[] = [];
    if (from) { args.push(from); where.push(`v.data_venda >= $${args.length}`); }
    if (to)   { args.push(to);   where.push(`v.data_venda <= $${args.length}`); }
    const r = await pool.query(`
      SELECT v.venda_id, v.combo_id, c.nome, v.quantidade,
             v.preco_unitario::float8 AS preco_unitario,
             v.preco_total::float8 AS preco_total,
             v.data_venda, v.responsavel, v.observacoes, v.tipo_movimento
      FROM combo_vendas v LEFT JOIN combos c ON c.combo_id = v.combo_id
      ${where.length ? "WHERE " + where.join(" AND ") : ""}
      ORDER BY v.data_venda DESC LIMIT 500
    `, args);
    res.json(r.rows);
  } catch (e) { next(e); }
});

export default router;
