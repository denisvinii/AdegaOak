import { Router, type IRouter } from "express";

const router: IRouter = Router();

router.post("/auth/verify-discount", (req, res) => {
  const expected = process.env.ADMIN_DESCONTO_SENHA ?? "ADEGA2024";
  const given = String(req.body?.password ?? "");
  res.json({ valid: given.length > 0 && given === expected });
});

export default router;
