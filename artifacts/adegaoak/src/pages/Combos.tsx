import { useState } from "react";
import {
  useListCombos, useCreateCombo, useUpdateCombo, useDeleteCombo,
  useListEstoque, getListCombosQueryKey,
} from "@workspace/api-client-react";
import { useQueryClient } from "@tanstack/react-query";
import { formatCurrency } from "@/lib/format";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Switch } from "@/components/ui/switch";
import { Textarea } from "@/components/ui/textarea";
import { Badge } from "@/components/ui/badge";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from "@/components/ui/dialog";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { useToast } from "@/hooks/use-toast";
import { Pencil, Plus, Trash2, X } from "lucide-react";

type ComponentRow = { product_id: number; quantidade: number; unidade: string; debita_estoque: boolean };
type ComboForm = {
  nome: string; descricao: string; preco_venda: number; ativo: boolean;
  composicao: ComponentRow[];
};
const empty: ComboForm = { nome: "", descricao: "", preco_venda: 0, ativo: true, composicao: [] };

export default function Combos() {
  const qc = useQueryClient();
  const { toast } = useToast();
  const [open, setOpen] = useState(false);
  const [editId, setEditId] = useState<number | null>(null);
  const [form, setForm] = useState<ComboForm>(empty);
  const [confirmDelete, setConfirmDelete] = useState<number | null>(null);

  const { data: combos, isLoading } = useListCombos();
  const { data: products } = useListEstoque();
  const create = useCreateCombo();
  const update = useUpdateCombo();
  const del = useDeleteCombo();
  const invalidate = () => qc.invalidateQueries({ queryKey: getListCombosQueryKey() });

  const startCreate = () => { setForm(empty); setEditId(null); setOpen(true); };
  const startEdit = (id: number) => {
    const c = combos?.find((x) => x.combo_id === id); if (!c) return;
    setForm({
      nome: c.nome, descricao: c.descricao ?? "", preco_venda: c.preco_venda, ativo: c.ativo,
      composicao: c.composicao.map((x) => ({
        product_id: x.product_id, quantidade: Number(x.quantidade), unidade: x.unidade ?? "un", debita_estoque: x.debita_estoque !== false,
      })),
    });
    setEditId(id); setOpen(true);
  };

  const addComp = () => setForm((f) => ({ ...f, composicao: [...f.composicao, { product_id: products?.[0]?.productid ?? 0, quantidade: 1, unidade: "un", debita_estoque: true }] }));
  const updateComp = (idx: number, patch: Partial<ComponentRow>) =>
    setForm((f) => ({ ...f, composicao: f.composicao.map((c, i) => i === idx ? { ...c, ...patch } : c) }));
  const removeComp = (idx: number) =>
    setForm((f) => ({ ...f, composicao: f.composicao.filter((_, i) => i !== idx) }));

  const submit = (e: React.FormEvent) => {
    e.preventDefault();
    const data = {
      nome: form.nome, descricao: form.descricao || null,
      preco_venda: Number(form.preco_venda), ativo: form.ativo,
      composicao: form.composicao.filter((c) => c.product_id && c.quantidade > 0),
    };
    const onSuccess = () => { invalidate(); setOpen(false); toast({ title: "Salvo" }); };
    const onError = (e: any) => toast({ title: "Erro", description: String(e?.message ?? e), variant: "destructive" });
    if (editId != null) update.mutate({ comboId: editId, data: data as any }, { onSuccess, onError });
    else create.mutate({ data: data as any }, { onSuccess, onError });
  };

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h2 className="text-3xl font-serif font-bold tracking-tight">Combos</h2>
          <p className="text-muted-foreground mt-1">Combinações de produtos vendidas como pacote</p>
        </div>
        <Button onClick={startCreate}>Novo Combo</Button>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        {isLoading ? (
          <div className="text-muted-foreground">Carregando...</div>
        ) : !combos || combos.length === 0 ? (
          <div className="text-muted-foreground">Nenhum combo cadastrado.</div>
        ) : combos.map((c) => (
          <Card key={c.combo_id} className="flex flex-col">
            <CardHeader className="flex-row items-start justify-between gap-2">
              <div>
                <CardTitle className="text-base">{c.nome}</CardTitle>
                {c.descricao && <p className="text-xs text-muted-foreground mt-1">{c.descricao}</p>}
              </div>
              <Badge variant={c.ativo ? "secondary" : "outline"}>{c.ativo ? "Ativo" : "Inativo"}</Badge>
            </CardHeader>
            <CardContent className="flex-1 flex flex-col">
              <div className="flex items-baseline justify-between mb-3">
                <p className="text-2xl font-bold text-primary">{formatCurrency(c.preco_venda)}</p>
                <p className="text-xs text-muted-foreground">Custo: {formatCurrency(c.custo_total ?? 0)}</p>
              </div>
              <ul className="text-sm space-y-1 mb-4 flex-1">
                {c.composicao.map((x) => (
                  <li key={(x as any).composicao_id ?? x.product_id} className="flex justify-between">
                    <span>{x.produto || `Produto ${x.product_id}`}</span>
                    <span className="text-muted-foreground">{x.quantidade} {x.unidade}</span>
                  </li>
                ))}
              </ul>
              <div className="flex gap-2">
                <Button variant="outline" className="flex-1" onClick={() => startEdit(c.combo_id)}><Pencil className="h-4 w-4 mr-2" />Editar</Button>
                <Button variant="ghost" size="icon" onClick={() => setConfirmDelete(c.combo_id)}><Trash2 className="h-4 w-4" /></Button>
              </div>
            </CardContent>
          </Card>
        ))}
      </div>

      <Dialog open={open} onOpenChange={setOpen}>
        <DialogContent className="max-w-2xl max-h-[90vh] overflow-y-auto">
          <DialogHeader><DialogTitle>{editId != null ? "Editar combo" : "Novo combo"}</DialogTitle></DialogHeader>
          <form onSubmit={submit} className="space-y-3">
            <div className="grid grid-cols-2 gap-3">
              <div className="col-span-2"><Label>Nome</Label><Input value={form.nome} onChange={(e) => setForm({ ...form, nome: e.target.value })} required /></div>
              <div className="col-span-2"><Label>Descrição</Label><Textarea value={form.descricao} onChange={(e) => setForm({ ...form, descricao: e.target.value })} rows={2} /></div>
              <div><Label>Preço de venda (R$)</Label><Input type="number" step="0.01" value={form.preco_venda} onChange={(e) => setForm({ ...form, preco_venda: Number(e.target.value) })} required /></div>
              <div className="flex items-end gap-2">
                <Switch id="ativo" checked={form.ativo} onCheckedChange={(v) => setForm({ ...form, ativo: v })} />
                <Label htmlFor="ativo">Ativo</Label>
              </div>
            </div>

            <div className="border rounded-md p-3 space-y-2">
              <div className="flex justify-between items-center">
                <Label>Composição</Label>
                <Button type="button" variant="outline" size="sm" onClick={addComp}><Plus className="h-3.5 w-3.5 mr-1" />Adicionar</Button>
              </div>
              {form.composicao.length === 0 && <p className="text-xs text-muted-foreground">Adicione produtos que compõem este combo.</p>}
              {form.composicao.map((c, idx) => (
                <div key={idx} className="grid grid-cols-12 gap-2 items-end">
                  <div className="col-span-5">
                    <Select value={String(c.product_id)} onValueChange={(v) => updateComp(idx, { product_id: Number(v) })}>
                      <SelectTrigger><SelectValue placeholder="Produto" /></SelectTrigger>
                      <SelectContent>
                        {products?.map((p) => <SelectItem key={p.productid} value={String(p.productid)}>{p.bebida} {p.tamanho}</SelectItem>)}
                      </SelectContent>
                    </Select>
                  </div>
                  <div className="col-span-2"><Input type="number" step="0.01" min={0} value={c.quantidade} onChange={(e) => updateComp(idx, { quantidade: Number(e.target.value) })} /></div>
                  <div className="col-span-2"><Input value={c.unidade} onChange={(e) => updateComp(idx, { unidade: e.target.value })} placeholder="un" /></div>
                  <div className="col-span-2 flex items-center gap-1">
                    <Switch checked={c.debita_estoque} onCheckedChange={(v) => updateComp(idx, { debita_estoque: v })} />
                    <span className="text-xs">Debita</span>
                  </div>
                  <div className="col-span-1"><Button type="button" variant="ghost" size="icon" onClick={() => removeComp(idx)}><X className="h-4 w-4" /></Button></div>
                </div>
              ))}
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
          <DialogHeader><DialogTitle>Remover combo?</DialogTitle></DialogHeader>
          <DialogFooter>
            <Button variant="ghost" onClick={() => setConfirmDelete(null)}>Cancelar</Button>
            <Button variant="destructive" disabled={del.isPending} onClick={() => {
              if (confirmDelete == null) return;
              del.mutate({ comboId: confirmDelete }, {
                onSuccess: () => { invalidate(); setConfirmDelete(null); toast({ title: "Removido" }); },
              });
            }}>Remover</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
