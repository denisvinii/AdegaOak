import { useGetSaldo, useGetSalesTrend, useGetDashboardOverview } from "@workspace/api-client-react";
import { formatCurrency } from "@/lib/format";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { ResponsiveContainer, AreaChart, Area, XAxis, YAxis, Tooltip, CartesianGrid } from "recharts";

export default function Saldo() {
  const { data: saldo, isLoading } = useGetSaldo();
  const { data: trend } = useGetSalesTrend({ days: 60 });
  const { data: overview } = useGetDashboardOverview();

  const stats = [
    { label: "Total de Entradas (Compras)", value: saldo?.total_entradas ?? 0, hint: "Valor investido em estoque ao longo do tempo" },
    { label: "Total de Saídas (Vendas)", value: saldo?.total_saidas ?? 0, hint: "Receita acumulada" },
    { label: "Investido pelo Admin", value: saldo?.investido_admin ?? 0, hint: "Aporte registrado como Entrada por 'admin'" },
    { label: "Capital da Empresa", value: saldo?.capital_empresa ?? 0, hint: "Saídas - Entradas + Investimentos do admin" },
    { label: "Lucro Estimado", value: saldo?.lucro ?? 0, hint: "Receita menos custo de cada item vendido", accent: true },
    { label: "Margem", value: saldo?.margem_percentual ?? 0, hint: "Lucro / Receita", isPercent: true },
  ];

  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-3xl font-serif font-bold tracking-tight">Saldo & Caixa</h2>
        <p className="text-muted-foreground mt-1">Visão financeira consolidada</p>
      </div>

      <Card className="bg-gradient-to-br from-primary to-primary/80 text-primary-foreground border-0">
        <CardHeader><CardTitle className="text-sm font-medium uppercase tracking-wider opacity-80">Saldo atual</CardTitle></CardHeader>
        <CardContent>
          {isLoading ? <Skeleton className="h-12 w-64 bg-white/20" /> : (
            <div className="text-5xl font-serif font-bold">{formatCurrency(saldo?.saldo ?? 0)}</div>
          )}
          <p className="text-sm opacity-80 mt-2">Saídas - (Entradas - Investido pelo admin)</p>
        </CardContent>
      </Card>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        {stats.map((s) => (
          <Card key={s.label} className={s.accent ? "border-primary/40" : ""}>
            <CardHeader className="pb-2"><CardTitle className="text-sm font-medium text-muted-foreground">{s.label}</CardTitle></CardHeader>
            <CardContent>
              <div className={`text-2xl font-bold ${s.accent ? "text-primary" : ""}`}>
                {isLoading ? <Skeleton className="h-7 w-24" /> : (s.isPercent ? `${(s.value as number).toFixed(1)}%` : formatCurrency(s.value as number))}
              </div>
              <p className="text-xs text-muted-foreground mt-1">{s.hint}</p>
            </CardContent>
          </Card>
        ))}
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <Card><CardHeader className="pb-2"><CardTitle className="text-sm">Vendas Hoje</CardTitle></CardHeader><CardContent className="text-xl font-bold">{formatCurrency(overview?.vendas_hoje ?? 0)}</CardContent></Card>
        <Card><CardHeader className="pb-2"><CardTitle className="text-sm">Vendas na Semana</CardTitle></CardHeader><CardContent className="text-xl font-bold">{formatCurrency(overview?.vendas_semana ?? 0)}</CardContent></Card>
        <Card><CardHeader className="pb-2"><CardTitle className="text-sm">Vendas no Mês</CardTitle></CardHeader><CardContent className="text-xl font-bold">{formatCurrency(overview?.vendas_mes ?? 0)}</CardContent></Card>
      </div>

      <Card>
        <CardHeader><CardTitle>Tendência — últimos 60 dias</CardTitle></CardHeader>
        <CardContent className="h-72">
          {trend && (
            <ResponsiveContainer width="100%" height="100%">
              <AreaChart data={trend}>
                <defs>
                  <linearGradient id="vendas" x1="0" y1="0" x2="0" y2="1">
                    <stop offset="0%" stopColor="hsl(var(--primary))" stopOpacity={0.4} />
                    <stop offset="100%" stopColor="hsl(var(--primary))" stopOpacity={0} />
                  </linearGradient>
                  <linearGradient id="lucro" x1="0" y1="0" x2="0" y2="1">
                    <stop offset="0%" stopColor="hsl(var(--chart-2))" stopOpacity={0.4} />
                    <stop offset="100%" stopColor="hsl(var(--chart-2))" stopOpacity={0} />
                  </linearGradient>
                </defs>
                <CartesianGrid strokeDasharray="3 3" className="opacity-30" />
                <XAxis dataKey="date" tick={{ fontSize: 11 }} tickFormatter={(v) => v.slice(5)} />
                <YAxis tick={{ fontSize: 11 }} tickFormatter={(v) => `R$${Math.round(v)}`} />
                <Tooltip
                  formatter={(v: number) => formatCurrency(v)}
                  contentStyle={{ background: "hsl(var(--card))", border: "1px solid hsl(var(--border))", borderRadius: 8 }}
                />
                <Area type="monotone" dataKey="vendas" stroke="hsl(var(--primary))" fill="url(#vendas)" strokeWidth={2} />
                <Area type="monotone" dataKey="lucro" stroke="hsl(var(--chart-2))" fill="url(#lucro)" strokeWidth={2} />
              </AreaChart>
            </ResponsiveContainer>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
