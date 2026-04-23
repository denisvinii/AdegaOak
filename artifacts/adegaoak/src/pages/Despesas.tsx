import { useState } from "react";
import {
  useListDespesas, useCreateDespesa, useUpdateDespesa, useDeleteDespesa,
  getListDespesasQueryKey, getGetDashboardOverviewQueryKey, getGetSaldoQueryKey,
} from "@workspace/api-client-react";
import { useQueryClient } from "@tanstack/react-query";
import { formatCurrency, formatDate } from "@/lib/format";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Badge } from "@/components/ui/badge";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from "@/components/ui/dialog";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Textarea } from "@/components/ui/textarea";
import { Switch } from "@/components/ui/switch";
import { useToast } from "@/hooks/use-toast";
import { Pencil, Trash2, Check } from "lucide-react";

const TIPOS = ["Operacional", "Fornecedor", "Salário", "Outros"];

type DespesaForm = {
  descricao: string; valor: number; data: string; tipo: number; pago: boolean; notas?: string | null;
};

const empty: DespesaForm = { descricao: "", valor: 0, data: new Date().toISOString().slice(0, 10), tipo: 0, pago: false, notas: "" };

export default function Despesas() {
  const qc = useQueryClient();
  const { toast } = useToast();
  const [filterPago, setFilterPago] = useState<"all" | "true" | "false">("all");
  const [open, setOpen] = useState(false);
  const [editId, setEditId] = useState<number | null>(null);
  const [confirmDelete, setConfirmDelete] = useState<number | null>(null);
  const [form, setForm] = useState<DespesaForm>(empty);

  const pagoParam = filterPago === "all" ? undefined : filterPago === "true";
  const { data: items, isLoading } = useListDespesas({ pago: pagoParam });
  const create = useCreateDespesa();
  const update = useUpdateDespesa();
  const del = useDeleteDespesa();

  const invalidate = () => {
    qc.invalidateQueries({ queryKey: getListDespesasQueryKey() });
    qc.invalidateQueries({ queryKey: getGetDashboardOverviewQueryKey() });
    qc.invalidateQueries({ queryKey: getGetSaldoQueryKey() });
  };

  const openCreate = () => { setForm(empty); setEditId(null); setOpen(true); };
  const openEdit = (id: number) => {
    const d = items?.find((x) => x.id === id); if (!d) return;
    setForm({
      descricao: d.descricao, valor: d.valor,
      data: new Date(d.data).toISOString().slice(0, 10),
      tipo: d.tipo, pago: d.pago, notas: d.notas ?? "",
    });
    setEditId(id); setOpen(true);
  };

  const submit = (e: React.FormEvent) => {
    e.preventDefault();
    const payload = {
      descricao: form.descricao, valor: Number(form.valor),
      data: new Date(form.data).toISOString(),
      tipo: Number(form.tipo), pago: form.pago,
      data_pagamento: form.pago ? new Date().toISOString() : null,
      notas: form.notas || null,
    };
    const onSuccess = () => { invalidate(); setOpen(false); toast({ title: "Salvo" }); };
    const onError = (e: any) => toast({ title: "Erro", description: String(e?.message ?? e), variant: "destructive" });
    if (editId != null) update.mutate({ id: editId, data: payload }, { onSuccess, onError });
    else create.mutate({ data: payload }, { onSuccess, onError });
  };

  const togglePago = (id: number, pago: boolean) => {
    update.mutate({
      id,
      data: { pago: !pago, data_pagamento: !pago ? new Date().toISOString() : null } as any,
    }, { onSuccess: invalidate });
  };

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h2 className="text-3xl font-serif font-bold tracking-tight">Despesas</h2>
          <p className="text-muted-foreground mt-1">Contas a pagar, fornecedores e salários</p>
        </div>
        <Button onClick={openCreate}>Nova Despesa</Button>
      </div>

      <div className="flex gap-3 items-end">
        <div>
          <Label>Status</Label>
          <Select value={filterPago} onValueChange={(v) => setFilterPago(v as any)}>
            <SelectTrigger className="w-40"><SelectValue /></SelectTrigger>
            <SelectContent>
              <SelectItem value="all">Todas</SelectItem>
              <SelectItem value="false">Pendentes</SelectItem>
              <SelectItem value="true">Pagas</SelectItem>
            </SelectContent>
          </Select>
        </div>
      </div>

      <div className="border rounded-md bg-card">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Vencimento</TableHead>
              <TableHead>Descrição</TableHead>
              <TableHead>Tipo</TableHead>
              <TableHead>Status</TableHead>
              <TableHead className="text-right">Valor</TableHead>
              <TableHead className="w-40" />
            </TableRow>
          </TableHeader>
          <TableBody>
            {isLoading ? (
              <TableRow><TableCell colSpan={6} className="text-center py-8 text-muted-foreground">Carregando...</TableCell></TableRow>
            ) : !items || items.length === 0 ? (
              <TableRow><TableCell colSpan={6} className="text-center py-8 text-muted-foreground">Nenhuma despesa.</TableCell></TableRow>
            ) : items.map((d) => (
              <TableRow key={d.id}>
                <TableCell>{formatDate(d.data)}</TableCell>
                <TableCell className="font-medium">{d.descricao}</TableCell>
                <TableCell>{TIPOS[d.tipo] ?? "—"}</TableCell>
                <TableCell>
                  <Badge variant={d.pago ? "secondary" : "destructive"}>{d.pago ? "Pago" : "Pendente"}</Badge>
                </TableCell>
                <TableCell className="text-right font-semibold">{formatCurrency(d.valor)}</TableCell>
                <TableCell className="text-right">
                  <Button variant="ghost" size="icon" title={d.pago ? "Marcar pendente" : "Marcar paga"} onClick={() => togglePago(d.id, d.pago)}>
                    <Check className={`h-4 w-4 ${d.pago ? "text-emerald-600" : "text-muted-foreground"}`} />
                  </Button>
                  <Button variant="ghost" size="icon" onClick={() => openEdit(d.id)}><Pencil className="h-4 w-4" /></Button>
                  <Button variant="ghost" size="icon" onClick={() => setConfirmDelete(d.id)}><Trash2 className="h-4 w-4" /></Button>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>

      <Dialog open={open} onOpenChange={setOpen}>
        <DialogContent>
          <DialogHeader><DialogTitle>{editId != null ? "Editar despesa" : "Nova despesa"}</DialogTitle></DialogHeader>
          <form onSubmit={submit} className="space-y-3">
            <div>
              <Label>Descrição</Label>
              <Input value={form.descricao} onChange={(e) => setForm({ ...form, descricao: e.target.value })} required />
            </div>
            <div className="grid grid-cols-2 gap-3">
              <div>
                <Label>Valor (R$)</Label>
                <Input type="number" step="0.01" value={form.valor} onChange={(e) => setForm({ ...form, valor: Number(e.target.value) })} required />
              </div>
              <div>
                <Label>Vencimento</Label>
                <Input type="date" value={form.data} onChange={(e) => setForm({ ...form, data: e.target.value })} required />
              </div>
              <div>
                <Label>Tipo</Label>
                <Select value={String(form.tipo)} onValueChange={(v) => setForm({ ...form, tipo: Number(v) })}>
                  <SelectTrigger><SelectValue /></SelectTrigger>
                  <SelectContent>
                    {TIPOS.map((t, i) => <SelectItem key={i} value={String(i)}>{t}</SelectItem>)}
                  </SelectContent>
                </Select>
              </div>
              <div className="flex items-end gap-2">
                <Switch id="pago" checked={form.pago} onCheckedChange={(v) => setForm({ ...form, pago: v })} />
                <Label htmlFor="pago">Pago</Label>
              </div>
            </div>
            <div>
              <Label>Notas</Label>
              <Textarea value={form.notas ?? ""} onChange={(e) => setForm({ ...form, notas: e.target.value })} rows={2} />
            </div>
            <DialogFooter>
              <Button variant="ghost" type="button" onClick={() => setOpen(false)}>Cancelar</Button>
              <Button type="submit" disabled={create.isPending || update.isPending}>Salvar</Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>

      <Dialog open={confirmDelete != null} onOpenChange={(o) => !o && setConfirmDelete(null)}>
        <DialogContent>
          <DialogHeader><DialogTitle>Remover despesa?</DialogTitle></DialogHeader>
          <DialogFooter>
            <Button variant="ghost" onClick={() => setConfirmDelete(null)}>Cancelar</Button>
            <Button variant="destructive" disabled={del.isPending} onClick={() => {
              if (confirmDelete == null) return;
              del.mutate({ id: confirmDelete }, {
                onSuccess: () => { invalidate(); setConfirmDelete(null); toast({ title: "Removida" }); },
              });
            }}>Remover</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
