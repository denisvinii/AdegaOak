# Adega Oak — Gestão

Sistema web responsivo para a gestão diária de uma adega de bairro: estoque, tabela de preços, combos, vendas, despesas e visão financeira (saldo / lucro / margem). Reescrita do antigo aplicativo desktop WPF como aplicação React + Node/TypeScript, mantendo todas as funcionalidades originais.

## Estrutura do projeto

Monorepo gerenciado com **pnpm workspaces**.

```
adegaoak/
├── artifacts/
│   ├── adegaoak/         # Frontend React + Vite (Tabela de Preços, Estoque, Movimentações, Combos, Despesas, Saldo, Funcionários, Painel)
│   └── api-server/       # Backend Express (rotas REST por domínio em src/routes/*)
├── lib/
│   ├── api-spec/         # Especificação OpenAPI (lib/api-spec/openapi.yaml) e configuração do Orval (codegen)
│   ├── api-client-react/ # Hooks React Query gerados automaticamente a partir do OpenAPI
│   └── db/               # Conexão com Postgres + scripts de bootstrap do banco
│       ├── sql/init.sql      # CREATE TABLE IF NOT EXISTS (idempotente)
│       ├── sql/seed.sql      # Catálogo inicial opcional
│       └── scripts/init.mjs  # Roda o init (e opcionalmente o seed) via Node
└── pnpm-workspace.yaml
```

## Pré-requisitos

- Node.js 20+
- pnpm 10+
- Postgres 14+ (local ou hospedado, ex.: Supabase, Neon, Railway)

## Configuração

1. Clone o repositório e instale as dependências:
   ```bash
   pnpm install
   ```

2. Crie um arquivo `.env` na raiz com a string de conexão do banco e a senha do gerente para liberar descontos:
   ```env
   SUPABASE_DATABASE_URL=postgres://usuario:senha@host:5432/banco
   # ou: DATABASE_URL=postgres://...
   ADMIN_DESCONTO_SENHA=ADEGA2024
   PORT=8080
   ```

3. Crie o esquema do banco (idempotente, pode rodar a qualquer momento):
   ```bash
   pnpm --filter @workspace/db run init           # apenas as tabelas
   pnpm --filter @workspace/db run init:seed      # tabelas + catálogo de exemplo
   ```

4. Suba o backend e o frontend (em terminais separados, ou use o seu gerenciador de processos preferido):
   ```bash
   pnpm --filter @workspace/api-server dev
   pnpm --filter @workspace/adegaoak    dev
   ```

   - Backend: `http://localhost:8080`
   - Frontend: `http://localhost:5173` (consome `BASE_URL/api`)

## Funcionalidades

| Tela              | O que faz                                                                                  |
|-------------------|---------------------------------------------------------------------------------------------|
| Painel            | Vendas hoje/semana/mês, valor em estoque, alertas de baixo estoque, atividades recentes.   |
| Estoque           | Cadastro de produtos com custo, venda varejo, valor da caixa, valor atacado por caixa, mínimos; gaveta lateral com últimas movimentações por produto. Quantidade calculada de `movimentacoes` (Entrada − Saída). |
| Movimentações    | Registro de Entradas (compras) e Saídas (vendas) Varejo/Atacado. Atacado debita do estoque `quantidade × quantidade_caixa` (estoque é em unidades individuais). Desconto manual liberado por **senha do gerente**. |
| Tabela de Preços  | Edição em lote de Custo, Venda Varejo, Valor Caixa, Atacado/Caixa **e Quantidade por Caixa**. Margem destacada em verde/âmbar. |
| Combos            | Cadastro de combos com composição (produtos, quantidade, unidade, se debita estoque). Custo total automático.|
| Venda de Combos   | Quick-sell com vendedor, observações e preço customizado (também atrás de senha). Histórico ao lado. Cada venda gera Saídas automáticas para cada item do combo. |
| Despesas          | CRUD de contas com tipos (Operacional, Fornecedor, Salário, Outros), filtro pago/pendente, marcar como paga em um clique. |
| Saldo             | Total de Entradas, Total de Saídas, Investido pelo Admin, Capital, Lucro, Margem %, gráfico de tendência de 60 dias. |
| Funcionários      | Lista, adicionar e remover vendedores. O seletor de vendedor no topo grava a escolha no navegador. |

Tema **escuro por padrão** (cellar/oak profundo com âmbar) e botão sol/lua no topo para alternar pra claro. A escolha fica salva no navegador.

## Comandos úteis

| Comando                                              | Para que serve                                            |
|------------------------------------------------------|-----------------------------------------------------------|
| `pnpm install`                                       | Instala dependências de todos os pacotes do workspace.    |
| `pnpm --filter @workspace/db run init`               | Cria as tabelas (idempotente).                            |
| `pnpm --filter @workspace/db run init:seed`          | Cria as tabelas e popula com produtos de exemplo.         |
| `pnpm --filter @workspace/api-server dev`            | Sobe o backend em modo dev (recompila e reinicia).        |
| `pnpm --filter @workspace/adegaoak dev`              | Sobe o frontend em modo dev com hot reload.               |
| `pnpm --filter @workspace/api-spec run codegen`      | Regenera os hooks/typings a partir do `openapi.yaml`.     |
| `pnpm -w run typecheck`                              | Roda o `tsc` em todo o monorepo.                          |

## Variáveis de ambiente

| Variável                      | Obrigatório | Descrição                                                            |
|-------------------------------|-------------|-----------------------------------------------------------------------|
| `SUPABASE_DATABASE_URL`       | ✓ (ou `DATABASE_URL`) | String de conexão Postgres usada por `lib/db`.            |
| `DATABASE_URL`                | alternativa | Fallback se `SUPABASE_DATABASE_URL` não estiver definido.            |
| `ADMIN_DESCONTO_SENHA`        | ✓           | Senha que libera descontos manuais e preço customizado em combos.    |
| `PORT`                        | opcional    | Porta do backend (default 8080).                                     |
