import { useEffect, useMemo, useState } from "react";
import {
  useListPrecos, useBulkUpdatePrecos,
  getListPrecosQueryKey, getListEstoqueQueryKey,
} from "@workspace/api-client-react";
import { useQueryClient } from "@tanstack/react-query";
import { formatCurrency } from "@/lib/format";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import { useToast } from "@/hooks/use-toast";

type Edits = Record<number, { valor?: number; valor_venda?: number; valor_caixa?: number; valor_atacado_caixa?: number; quantidade_caixa?: number }>;

export default function Precos() {
  const qc = useQueryClient();
  const { toast } = useToast();
  const { data: precos, isLoading } = useListPrecos();
  const update = useBulkUpdatePrecos();
  const [edits, setEdits] = useState<Edits>({});

  useEffect(() => { setEdits({}); }, [precos?.length]);

  const setEdit = (id: number, key: keyof Edits[number], value: number) =>
    setEdits((e) => ({ ...e, [id]: { ...(e[id] ?? {}), [key]: value } }));

  const dirty = useMemo(() => Object.entries(edits).filter(([, v]) => Object.keys(v).length > 0), [edits]);

  const save = () => {
    if (!precos) return;
    const items = dirty.map(([id, fields]) => {
      const orig = precos.find((p) => p.productid === Number(id));
      const out: any = { productid: Number(id) };
      for (const k of ["valor","valor_venda","valor_caixa","valor_atacado_caixa","quantidade_caixa"] as const) {
        if (fields[k] !== undefined && fields[k] !== (orig as any)?.[k]) out[k] = fields[k];
      }
      return out;
    }).filter((it) => Object.keys(it).length > 1);
    if (items.length === 0) { toast({ title: "Nada para salvar" }); return; }
    update.mutate({ data: { items } }, {
      onSuccess: (r) => {
        qc.invalidateQueries({ queryKey: getListPrecosQueryKey() });
        qc.invalidateQueries({ queryKey: getListEstoqueQueryKey() });
        toast({ title: `${r.updated} preço(s) atualizado(s)` });
        setEdits({});
      },
      onError: (e: any) => toast({ title: "Erro", description: String(e?.message ?? e), variant: "destructive" }),
    });
  };

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h2 className="text-3xl font-serif font-bold tracking-tight">Tabela de Preços</h2>
          <p className="text-muted-foreground mt-1">Edite os valores diretamente e salve em lote</p>
        </div>
        <div className="flex items-center gap-3">
          {dirty.length > 0 && <Badge variant="secondary">{dirty.length} alteração(ões)</Badge>}
          <Button onClick={save} disabled={update.isPending || dirty.length === 0}>
            {update.isPending ? "Salvando..." : "Salvar Alterações"}
          </Button>
        </div>
      </div>

      <div className="border rounded-md bg-card overflow-x-auto">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Produto</TableHead>
              <TableHead className="text-right">Un. por Caixa</TableHead>
              <TableHead className="text-right">Custo</TableHead>
              <TableHead className="text-right">Venda Varejo</TableHead>
              <TableHead className="text-right">Valor Caixa</TableHead>
              <TableHead className="text-right">Atacado Caixa</TableHead>
              <TableHead className="text-right">Margem</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {isLoading ? (
              <TableRow><TableCell colSpan={7} className="text-center py-8 text-muted-foreground">Carregando...</TableCell></TableRow>
            ) : precos?.map((p) => {
              const e = edits[p.productid] ?? {};
              const valor = e.valor ?? p.valor;
              const valor_venda = e.valor_venda ?? p.valor_venda;
              const margem = valor > 0 ? ((valor_venda - valor) / valor) * 100 : 0;
              return (
                <TableRow key={p.productid}>
                  <TableCell className="font-medium">{p.bebida} <span className="text-muted-foreground">{p.tamanho}</span></TableCell>
                  <TableCell><PriceCell integer min={1} value={e.quantidade_caixa ?? p.quantidade_caixa} onChange={(v) => setEdit(p.productid, "quantidade_caixa", v)} /></TableCell>
                  <TableCell><PriceCell value={valor} onChange={(v) => setEdit(p.productid, "valor", v)} /></TableCell>
                  <TableCell><PriceCell value={valor_venda} onChange={(v) => setEdit(p.productid, "valor_venda", v)} /></TableCell>
                  <TableCell><PriceCell value={e.valor_caixa ?? p.valor_caixa} onChange={(v) => setEdit(p.productid, "valor_caixa", v)} /></TableCell>
                  <TableCell><PriceCell value={e.valor_atacado_caixa ?? p.valor_atacado_caixa} onChange={(v) => setEdit(p.productid, "valor_atacado_caixa", v)} /></TableCell>
                  <TableCell className={`text-right font-semibold ${margem < 10 ? "text-amber-600" : margem < 0 ? "text-rose-600" : "text-emerald-600"}`}>{margem.toFixed(1)}%</TableCell>
                </TableRow>
              );
            })}
          </TableBody>
        </Table>
      </div>
      <p className="text-xs text-muted-foreground">Valor exibido em R$ (ex.: 12,50). A coluna “Un. por Caixa” é usada para debitar o estoque correto em vendas Atacado. As alterações ficam pendentes até clicar em Salvar.</p>
    </div>
  );
}

function PriceCell({ value, onChange, integer, min }: { value: number; onChange: (v: number) => void; integer?: boolean; min?: number }) {
  return (
    <div className="flex justify-end">
      <Input
        type="number"
        step={integer ? "1" : "0.01"}
        min={min}
        value={Number.isFinite(value) ? value : 0}
        onChange={(e) => {
          const n = integer ? Math.max(min ?? 0, Math.trunc(Number(e.target.value))) : Number(e.target.value);
          onChange(n);
        }}
        className="h-8 w-28 text-right"
      />
    </div>
  );
}
