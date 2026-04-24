#!/usr/bin/env node
// Bootstrap a Postgres database with the Adega Oak schema.
// Usage:
//   pnpm --filter @workspace/db init           (schema only)
//   pnpm --filter @workspace/db init --seed    (schema + sample products)
//
// Reads the connection string from SUPABASE_DATABASE_URL or DATABASE_URL.

import { readFileSync } from "node:fs";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";
import pg from "pg";

const here = dirname(fileURLToPath(import.meta.url));
const sqlDir = join(here, "..", "sql");

const url = process.env.SUPABASE_DATABASE_URL ?? process.env.DATABASE_URL;
if (!url) {
  console.error("ERRO: defina SUPABASE_DATABASE_URL ou DATABASE_URL no ambiente.");
  process.exit(1);
}

const withSeed = process.argv.includes("--seed");
const files = ["init.sql", ...(withSeed ? ["seed.sql"] : [])];

const pool = new pg.Pool({ connectionString: url, max: 2, connectionTimeoutMillis: 10_000 });

try {
  for (const f of files) {
    const sql = readFileSync(join(sqlDir, f), "utf8");
    process.stdout.write(`Executando ${f}... `);
    await pool.query(sql);
    process.stdout.write("OK\n");
  }
  console.log("Banco pronto.");
} catch (e) {
  console.error("Falha ao inicializar o banco:", e?.message ?? e);
  process.exitCode = 1;
} finally {
  await pool.end();
}
