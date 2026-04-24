export { pool } from "@workspace/db";

export function asInt(v: unknown, fallback?: number): number | undefined {
  if (v === undefined || v === null || v === "") return fallback;
  const n = Number(v);
  if (Number.isNaN(n)) return fallback;
  return Math.trunc(n);
}

export function asNum(v: unknown, fallback?: number): number | undefined {
  if (v === undefined || v === null || v === "") return fallback;
  const n = Number(v);
  if (Number.isNaN(n)) return fallback;
  return n;
}

export function asBool(v: unknown): boolean | undefined {
  if (v === undefined || v === null || v === "") return undefined;
  if (typeof v === "boolean") return v;
  const s = String(v).toLowerCase();
  if (["true", "1", "yes", "sim"].includes(s)) return true;
  if (["false", "0", "no", "nao", "não"].includes(s)) return false;
  return undefined;
}

export function toIso(v: unknown): string | null {
  if (!v) return null;
  if (v instanceof Date) return v.toISOString();
  return new Date(String(v)).toISOString();
}
