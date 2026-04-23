import { useState } from "react";
import {
  useListEstoque, useCreateProduct, useUpdateProduct, useDeleteProduct,
  useListMovimentacoes,
  getListEstoqueQueryKey, getGetDashboardOverviewQueryKey,
} from "@workspace/api-client-react";
import { useQueryClient } from "@tanstack/react-query";
import { formatCurrency, formatDateTime } from "@/lib/format";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Switch } from "@/components/ui/switch";
import { Label } from "@/components/ui/label";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger, DialogFooter } from "@/components/ui/dialog";
import { Sheet, SheetContent, SheetHeader, SheetTitle } from "@/components/ui/sheet";
import { Badge } from "@/components/ui/badge";
import { ProductForm, type ProductFormValues } from "@/components/forms/ProductForm";
import { AlertTriangle, Pencil, Trash2 } from "lucide-react";
import { useToast } from "@/hooks/use-toast";

export default function Estoque() {
  const qc = useQueryClient();
  const { toast } = useToast();
  const [search, setSearch] = useState("");
  const [low, setLow] = useState(false);
  const [open, setOpen] = useState(false);
  const [editId, setEditId] = useState<number | null>(null);
  const [drawerId, setDrawerId] = useState<number | null>(null);
  const [confirmDelete, setConfirmDelete] = useState<number | null>(null);

  const { data: items, isLoading } = useListEstoque({ search: search || undefined, low: low || undefined });
  const create = useCreateProduct();
  const update = useUpdateProduct();
  const del = useDeleteProduct();

  const editing = items?.find((p) => p.productid === editId);
  const drawerItem = items?.find((p) => p.productid === drawerId);
  const { data: drawerMovs } = useListMovimentacoes(
    { productid: drawerId ?? undefined, limit: 25 },
    { query: { enabled: drawerId != null } },
  );

  const invalidateAll = () => {
    qc.invalidateQueries({ queryKey: getListEstoqueQueryKey() });
    qc.invalidateQueries({ queryKey: getGetDashboardOverviewQueryKey() });
  };

  const handleCreate = (v: ProductFormValues) =>
    create.mutate({ data: v }, {
      onSuccess: () => { invalidateAll(); setOpen(false); toast({ title: "Produto criado" }); },
      onError: (e: any) => toast({ title: "Erro ao criar", description: String(e?.message ?? e), variant: "destructive" }),
    });

  const handleEdit = (v: ProductFormValues) => {
    if (editId == null) return;
    update.mutate({ productid: editId, data: v }, {
      onSuccess: () => { invalidateAll(); setEditId(null); toast({ title: "Produto atualizado" }); },
      onError: (e: any) => toast({ title: "Erro", description: String(e?.message ?? e), variant: "destructive" }),
    });
  };

  const handleDelete = () => {
    if (confirmDelete == null) return;
    del.mutate({ productid: confirmDelete }, {
      onSuccess: () => { invalidateAll(); toast({ title: "Produto removido" }); setConfirmDelete(null); },
      onError: (e: any) => toast({ title: "Erro", description: String(e?.message ?? e), variant: "destructive" }),
    });
  };

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h2 className="text-3xl font-serif font-bold tracking-tight">Estoque</h2>
          <p className="text-muted-foreground mt-1">Gerencie os produtos e o inventário</p>
        </div>
        <Dialog open={open} onOpenChange={setOpen}>
          <DialogTrigger asChild><Button>Novo Produto</Button></DialogTrigger>
          <DialogContent className="max-w-2xl">
            <DialogHeader><DialogTitle>Novo Produto</DialogTitle></DialogHeader>
            <ProductForm onSubmit={handleCreate} submitting={create.isPending} onCancel={() => setOpen(false)} />
          </DialogContent>
        </Dialog>
      </div>

      <div className="flex gap-4 items-center">
        <Input placeholder="Buscar bebida, tamanho ou material..." value={search} onChange={(e) => setSearch(e.target.value)} className="max-w-sm" />
        <div className="flex items-center gap-2">
          <Switch id="low" checked={low} onCheckedChange={setLow} />
          <Label htmlFor="low">Apenas baixo estoque</Label>
        </div>
      </div>

      <div className="border rounded-md bg-card">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Produto</TableHead>
              <TableHead>Tamanho</TableHead>
              <TableHead>Material</TableHead>
              <TableHead className="text-right">Custo</TableHead>
              <TableHead className="text-right">Varejo</TableHead>
              <TableHead className="text-right">Caixa</TableHead>
              <TableHead className="text-right">Quantidade</TableHead>
              <TableHead className="w-32" />
            </TableRow>
          </TableHeader>
          <TableBody>
            {isLoading ? (
              <TableRow><TableCell colSpan={8} className="text-center py-8 text-muted-foreground">Carregando...</TableCell></TableRow>
            ) : !items || items.length === 0 ? (
              <TableRow><TableCell colSpan={8} className="text-center py-8 text-muted-foreground">Nenhum produto encontrado.</TableCell></TableRow>
            ) : items.map((p) => {
              const isLowStock = p.quantidade <= p.estoque_minimo;
              return (
                <TableRow key={p.productid} className="cursor-pointer" onClick={() => setDrawerId(p.productid)}>
                  <TableCell className="font-medium">{p.bebida}</TableCell>
                  <TableCell>{p.tamanho}</TableCell>
                  <TableCell>{p.material}</TableCell>
                  <TableCell className="text-right">{formatCurrency(p.valor)}</TableCell>
                  <TableCell className="text-right">{formatCurrency(p.valor_venda)}</TableCell>
                  <TableCell className="text-right">{formatCurrency(p.valor_caixa)}</TableCell>
                  <TableCell className="text-right">
                    <div className="inline-flex items-center gap-2">
                      {isLowStock && <AlertTriangle className="h-4 w-4 text-amber-500" />}
                      <span className={isLowStock ? "font-semibold text-amber-600" : "font-semibold"}>{p.quantidade}</span>
                    </div>
                  </TableCell>
                  <TableCell className="text-right" onClick={(e) => e.stopPropagation()}>
                    <Button variant="ghost" size="icon" onClick={() => setEditId(p.productid)}><Pencil className="h-4 w-4" /></Button>
                    <Button variant="ghost" size="icon" onClick={() => setConfirmDelete(p.productid)}><Trash2 className="h-4 w-4" /></Button>
                  </TableCell>
                </TableRow>
              );
            })}
          </TableBody>
        </Table>
      </div>

      <Dialog open={editId != null} onOpenChange={(o) => !o && setEditId(null)}>
        <DialogContent className="max-w-2xl">
          <DialogHeader><DialogTitle>Editar Produto</DialogTitle></DialogHeader>
          {editing && (
            <ProductForm
              initial={editing}
              onSubmit={handleEdit}
              submitting={update.isPending}
              onCancel={() => setEditId(null)}
            />
          )}
        </DialogContent>
      </Dialog>

      <Dialog open={confirmDelete != null} onOpenChange={(o) => !o && setConfirmDelete(null)}>
        <DialogContent>
          <DialogHeader><DialogTitle>Remover produto?</DialogTitle></DialogHeader>
          <p className="text-sm text-muted-foreground">O produto será desativado mas o histórico será preservado.</p>
          <DialogFooter>
            <Button variant="ghost" onClick={() => setConfirmDelete(null)}>Cancelar</Button>
            <Button variant="destructive" onClick={handleDelete} disabled={del.isPending}>Remover</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      <Sheet open={drawerId != null} onOpenChange={(o) => !o && setDrawerId(null)}>
        <SheetContent className="sm:max-w-lg">
          <SheetHeader>
            <SheetTitle>{drawerItem?.bebida} {drawerItem?.tamanho}</SheetTitle>
          </SheetHeader>
          {drawerItem && (
            <div className="mt-4 space-y-4">
              <div className="grid grid-cols-2 gap-3 text-sm">
                <Stat label="Custo" value={formatCurrency(drawerItem.valor)} />
                <Stat label="Venda Varejo" value={formatCurrency(drawerItem.valor_venda)} />
                <Stat label="Valor Caixa" value={formatCurrency(drawerItem.valor_caixa)} />
                <Stat label="Atacado Caixa" value={formatCurrency(drawerItem.valor_atacado_caixa)} />
                <Stat label="Em estoque" value={String(drawerItem.quantidade)} />
                <Stat label="Mínimo" value={String(drawerItem.estoque_minimo)} />
              </div>
              <div>
                <h4 className="text-sm font-semibold mb-2">Últimas movimentações</h4>
                <div className="space-y-2 max-h-[420px] overflow-y-auto pr-1">
                  {drawerMovs && drawerMovs.length > 0 ? drawerMovs.map((m) => (
                    <div key={m.id} className="flex items-center justify-between rounded-md border bg-card/40 px-3 py-2 text-sm">
                      <div>
                        <div className="flex items-center gap-2">
                          <Badge variant={m.tipo === "Entrada" ? "secondary" : "default"}>{m.tipo}</Badge>
                          <span className="text-muted-foreground">{formatDateTime(m.data)}</span>
                        </div>
                        <div className="text-xs text-muted-foreground mt-0.5">{m.responsavel}{m.saida ? ` • ${m.saida}` : ""}</div>
                      </div>
                      <div className="text-right">
                        <div className="font-medium">{m.quantidade} un</div>
                        <div className="text-xs text-muted-foreground">{formatCurrency(m.valor_total ?? 0)}</div>
                      </div>
                    </div>
                  )) : (
                    <p className="text-sm text-muted-foreground">Sem movimentações.</p>
                  )}
                </div>
              </div>
            </div>
          )}
        </SheetContent>
      </Sheet>
    </div>
  );
}

function Stat({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-md bg-muted/50 px-3 py-2">
      <div className="text-xs text-muted-foreground">{label}</div>
      <div className="font-medium">{value}</div>
    </div>
  );
}
