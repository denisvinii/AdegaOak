import { Router, type IRouter } from "express";
import healthRouter from "./health";
import estoqueRouter from "./estoque";
import movimentacoesRouter from "./movimentacoes";
import precosRouter from "./precos";
import combosRouter from "./combos";
import vendasRouter from "./vendas";
import despesasRouter from "./despesas";
import dashboardRouter from "./dashboard";
import funcionariosRouter from "./funcionarios";
import authRouter from "./auth";

const router: IRouter = Router();

router.use(healthRouter);
router.use(estoqueRouter);
router.use(movimentacoesRouter);
router.use(precosRouter);
router.use(combosRouter);
router.use(vendasRouter);
router.use(despesasRouter);
router.use(dashboardRouter);
router.use(funcionariosRouter);
router.use(authRouter);

export default router;
