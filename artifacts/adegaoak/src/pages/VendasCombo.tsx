import { useState } from "react";
import {
  useListComboVendas, useSellCombo, useListCombos, useVerifyDiscountPassword,
  getListComboVendasQueryKey, getListEstoqueQueryKey, getListMovimentacoesQueryKey,
  getGetDashboardOverviewQueryKey, getGetSaldoQueryKey, getGetRecentActivityQueryKey,
} from "@workspace/api-client-react";
import { useQueryClient } from "@tanstack/react-query";
import { formatCurrency, formatDateTime } from "@/lib/format";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Textarea } from "@/components/ui/textarea";
import { Badge } from "@/components/ui/badge";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from "@/components/ui/dialog";
import { useResponsavel } from "@/hooks/use-responsavel";
import { useToast } from "@/hooks/use-toast";
import { Lock } from "lucide-react";

export default function VendasCombo() {
  const qc = useQueryClient();
  const { toast } = useToast();
  const { responsavel } = useResponsavel();
  const { data: combos } = useListCombos();
  const { data: vendas, isLoading } = useListComboVendas();
  const sell = useSellCombo();
  const verify = useVerifyDiscountPassword();

  const [comboId, setComboId] = useState<string>("");
  const [quantidade, setQuantidade] = useState<number>(1);
  const [vendedor, setVendedor] = useState<string>(responsavel);
  const [obs, setObs] = useState<string>("");
  const [override, setOverride] = useState<number | "">("");
  const [pwOpen, setPwOpen] = useState(false);
  const [pw, setPw] = useState("");
  const [overrideUnlocked, setOverrideUnlocked] = useState(false);

  const combo = combos?.find((c) => String(c.combo_id) === comboId);
  const unit = override !== "" && overrideUnlocked ? Number(override) : combo?.preco_venda ?? 0;
  const total = unit * quantidade;

  const invalidate = () => {
    qc.invalidateQueries({ queryKey: getListComboVendasQueryKey() });
    qc.invalidateQueries({ queryKey: getListEstoqueQueryKey() });
    qc.invalidateQueries({ queryKey: getListMovimentacoesQueryKey() });
    qc.invalidateQueries({ queryKey: getGetDashboardOverviewQueryKey() });
    qc.invalidateQueries({ queryKey: getGetSaldoQueryKey() });
    qc.invalidateQueries({ queryKey: getGetRecentActivityQueryKey() });
  };

  const submit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!combo) return;
    sell.mutate({ data: {
      combo_id: combo.combo_id, quantidade,
      preco_unitario: overrideUnlocked && override !== "" ? Number(override) : null,
      responsavel: vendedor || "—",
      observacoes: obs || null,
    } as any }, {
      onSuccess: () => { invalidate(); toast({ title: "Venda registrada" }); setQuantidade(1); setObs(""); setOverride(""); setOverrideUnlocked(false); },
      onError: (e: any) => toast({ title: "Erro", description: String(e?.message ?? e), variant: "destructive" }),
    });
  };

  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-3xl font-serif font-bold tracking-tight">Venda de Combos</h2>
        <p className="text-muted-foreground mt-1">Registro rápido de combos vendidos no balcão</p>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-5 gap-6">
        <Card className="lg:col-span-2">
          <CardHeader><CardTitle>Registrar venda</CardTitle></CardHeader>
          <CardContent>
            <form onSubmit={submit} className="space-y-3">
              <div>
                <Label>Combo</Label>
                <Select value={comboId} onValueChange={setComboId}>
                  <SelectTrigger><SelectValue placeholder="Selecione um combo" /></SelectTrigger>
                  <SelectContent>
                    {combos?.filter((c) => c.ativo).map((c) => (
                      <SelectItem key={c.combo_id} value={String(c.combo_id)}>{c.nome} — {formatCurrency(c.preco_venda)}</SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
              <div className="grid grid-cols-2 gap-3">
                <div>
                  <Label>Quantidade</Label>
                  <Input type="number" min={1} value={quantidade} onChange={(e) => setQuantidade(Number(e.target.value) || 1)} />
                </div>
                <div>
                  <Label>Vendedor</Label>
                  <Input value={vendedor} onChange={(e) => setVendedor(e.target.value)} />
                </div>
              </div>
              <div className="rounded-md border bg-muted/40 px-3 py-2 space-y-2">
                <div className="flex items-center justify-between">
                  <Label className="text-xs uppercase tracking-wide">Preço unitário customizado</Label>
                  {!overrideUnlocked ? (
                    <Button type="button" variant="ghost" size="sm" onClick={() => setPwOpen(true)}>
                      <Lock className="h-3.5 w-3.5 mr-1" /> Liberar
                    </Button>
                  ) : <Badge variant="secondary">Liberado</Badge>}
                </div>
                <Input
                  type="number" step="0.01" disabled={!overrideUnlocked}
                  placeholder={combo ? String(combo.preco_venda) : "0,00"}
                  value={override} onChange={(e) => setOverride(e.target.value === "" ? "" : Number(e.target.value))}
                />
              </div>
              <div>
                <Label>Observações</Label>
                <Textarea value={obs} onChange={(e) => setObs(e.target.value)} rows={2} />
              </div>
              <div className="rounded-md bg-primary/10 px-4 py-3 flex items-center justify-between">
                <span className="text-sm text-muted-foreground">Total</span>
                <span className="text-2xl font-bold text-primary">{formatCurrency(total)}</span>
              </div>
              <Button type="submit" className="w-full" disabled={!combo || sell.isPending}>{sell.isPending ? "Registrando..." : "Registrar venda"}</Button>
            </form>
          </CardContent>
        </Card>

        <Card className="lg:col-span-3">
          <CardHeader><CardTitle>Histórico</CardTitle></CardHeader>
          <CardContent>
            <div className="border rounded-md">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Data</TableHead>
                    <TableHead>Combo</TableHead>
                    <TableHead>Vendedor</TableHead>
                    <TableHead className="text-right">Qtd</TableHead>
                    <TableHead className="text-right">Total</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {isLoading ? (
                    <TableRow><TableCell colSpan={5} className="text-center py-8 text-muted-foreground">Carregando...</TableCell></TableRow>
                  ) : !vendas || vendas.length === 0 ? (
                    <TableRow><TableCell colSpan={5} className="text-center py-8 text-muted-foreground">Nenhuma venda ainda.</TableCell></TableRow>
                  ) : vendas.map((v) => (
                    <TableRow key={v.venda_id}>
                      <TableCell>{formatDateTime(v.data_venda)}</TableCell>
                      <TableCell className="font-medium">{v.nome ?? `Combo #${v.combo_id}`}</TableCell>
                      <TableCell>{v.responsavel}</TableCell>
                      <TableCell className="text-right">{v.quantidade}</TableCell>
                      <TableCell className="text-right font-semibold">{formatCurrency(v.preco_total)}</TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </div>
          </CardContent>
        </Card>
      </div>

      <Dialog open={pwOpen} onOpenChange={setPwOpen}>
        <DialogContent className="max-w-sm">
          <DialogHeader><DialogTitle>Senha do gerente</DialogTitle></DialogHeader>
          <p className="text-sm text-muted-foreground">Necessária para alterar o preço de um combo.</p>
          <Input type="password" value={pw} onChange={(e) => setPw(e.target.value)} autoFocus />
          <DialogFooter>
            <Button variant="ghost" onClick={() => { setPwOpen(false); setPw(""); }}>Cancelar</Button>
            <Button disabled={verify.isPending || !pw} onClick={() => verify.mutate({ data: { password: pw } }, {
              onSuccess: (r) => {
                if (r.valid) { setOverrideUnlocked(true); setPwOpen(false); setPw(""); toast({ title: "Liberado" }); }
                else toast({ title: "Senha incorreta", variant: "destructive" });
              },
              onError: () => toast({ title: "Falha", variant: "destructive" }),
            })}>Liberar</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
