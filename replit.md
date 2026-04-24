# Workspace

## Overview

pnpm workspace monorepo using TypeScript. Each package manages its own dependencies.

## Stack

- **Monorepo tool**: pnpm workspaces
- **Node.js version**: 24
- **Package manager**: pnpm
- **TypeScript version**: 5.9
- **API framework**: Express 5
- **Database**: PostgreSQL + Drizzle ORM
- **Validation**: Zod (`zod/v4`), `drizzle-zod`
- **API codegen**: Orval (from OpenAPI spec)
- **Build**: esbuild (CJS bundle)

## Key Commands

- `pnpm run typecheck` — full typecheck across all packages
- `pnpm run build` — typecheck + build all packages
- `pnpm --filter @workspace/api-spec run codegen` — regenerate API hooks and Zod schemas from OpenAPI spec
- `pnpm --filter @workspace/db run push` — push DB schema via Drizzle (dev only)
- `pnpm --filter @workspace/db run init` — bootstrap Postgres tables idempotently from `lib/db/sql/init.sql`
- `pnpm --filter @workspace/db run init:seed` — bootstrap + seed sample products from `lib/db/sql/seed.sql`
- `pnpm --filter @workspace/api-server run dev` — run API server locally
- `pnpm --filter @workspace/adegaoak run dev` — run frontend locally

See the `pnpm-workspace` skill for workspace structure, TypeScript setup, and package details.

## Adega Oak — App Notes

- Frontend: `artifacts/adegaoak` (React + Vite + shadcn/ui + recharts). Dark theme by default, sun/moon toggle in header (`src/hooks/use-theme.ts`).
- Backend: `artifacts/api-server` (Express + pino), routes split per domain in `src/routes/`. `noImplicitReturns` is disabled at the package level so Express handlers can return early via `return res.json(...)`.
- DB: Postgres (Supabase), connection from `SUPABASE_DATABASE_URL` (or `DATABASE_URL`). 7 tables: `estoque`, `movimentacoes`, `funcionarios`, `combos`, `combo_composicao`, `combo_vendas`, `despesas`. Idempotent bootstrap in `lib/db/sql/init.sql` + `lib/db/scripts/init.mjs`.
- Estoque é mantido em **unidades individuais** (Entrada − Saída de `movimentacoes.quantidade`). Vendas Atacado expandem `caixas × quantidade_caixa` no backend (`POST /movimentacoes`) e ajustam `valor_unitario` para que o total bruto seja preservado.
- A coluna `quantidade_caixa` em `estoque` é editável diretamente na Tabela de Preços (lote, junto com Custo / Venda Varejo / Valor Caixa / Atacado/Caixa).
- Senha do gerente vem de `ADMIN_DESCONTO_SENHA` (default `ADEGA2024`) e libera desconto manual em Movimentações e preço customizado em Venda de Combos.
- README.md na raiz traz instruções completas para `git clone` + setup local.

### Onde mora cada regra de negócio (backend Node + Express)

Toda a lógica de servidor está em `artifacts/api-server/src/routes/`, um arquivo por domínio. Para mudar uma regra basta editar o arquivo, salvar e reiniciar o workflow `artifacts/api-server: API Server`.

| Domínio                                           | Arquivo                                            |
|---------------------------------------------------|----------------------------------------------------|
| Estoque (CRUD, cálculo de quantidade)             | `routes/estoque.ts`                                |
| Movimentações + expansão de Atacado               | `routes/movimentacoes.ts`                          |
| Tabela de Preços (bulk update)                    | `routes/precos.ts`                                 |
| Combos (CRUD + composição)                        | `routes/combos.ts`                                 |
| Venda de combo (gera Saídas automáticas)          | `routes/vendas.ts`                                 |
| Despesas                                          | `routes/despesas.ts`                               |
| Funcionários                                      | `routes/funcionarios.ts`                           |
| Senha do gerente / desconto                       | `routes/auth.ts`                                   |
| Dashboard / Saldo / KPIs                          | `routes/dashboard.ts`                              |
| Pool Postgres                                     | `lib/db/src/index.ts`                              |
| Contrato OpenAPI (fonte da verdade)               | `lib/api-spec/openapi.yaml`                        |

Para mudanças que afetam o contrato REST (campos novos, endpoints novos): editar `openapi.yaml` → rodar `pnpm --filter @workspace/api-spec run codegen` → regera hooks React em `lib/api-client-react`.

### Cálculo de margens (Tabela de Preços)

São exibidas **três margens por produto**, todas relativas a `valor` (custo unitário):
- **Margem Varejo** = `(valor_venda − valor) / valor`
- **Margem Caixa (un.)** = `((valor_caixa / quantidade_caixa) − valor) / valor`
- **Margem Atacado (un.)** = `((valor_atacado_caixa / quantidade_caixa) − valor) / valor`

Cores: vermelho (< 0), âmbar (< 10%), verde (≥ 10%).
