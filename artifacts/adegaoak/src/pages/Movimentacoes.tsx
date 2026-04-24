import { useMemo, useState } from "react";
import {
  useListMovimentacoes, useCreateMovimentacao, useDeleteMovimentacao,
  useListEstoque, useVerifyDiscountPassword,
  getListMovimentacoesQueryKey, getListEstoqueQueryKey, getGetDashboardOverviewQueryKey,
  getGetSaldoQueryKey, getGetSalesTrendQueryKey, getGetTopProductsQueryKey,
  getGetRecentActivityQueryKey,
} from "@workspace/api-client-react";
import { useQueryClient } from "@tanstack/react-query";
import { formatCurrency, formatDateTime } from "@/lib/format";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from "@/components/ui/dialog";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Badge } from "@/components/ui/badge";
import { Textarea } from "@/components/ui/textarea";
import { useResponsavel } from "@/hooks/use-responsavel";
import { useToast } from "@/hooks/use-toast";
import { Trash2, ArrowDownToLine, ArrowUpFromLine, Lock } from "lucide-react";

type Tipo = "Entrada" | "Saída";

export default function Movimentacoes() {
  const qc = useQueryClient();
  const { toast } = useToast();
  const { responsavel } = useResponsavel();

  const [tipoFilter, setTipoFilter] = useState<"Entrada" | "Saída" | "All">("All");
  const [productFilter, setProductFilter] = useState<string>("all");
  const [openTipo, setOpenTipo] = useState<Tipo | null>(null);
  const [confirmDelete, setConfirmDelete] = useState<number | null>(null);

  const { data: products } = useListEstoque();
  const productid = productFilter === "all" ? undefined : Number(productFilter);
  const { data: rows, isLoading } = useListMovimentacoes({
    tipo: tipoFilter === "All" ? undefined : tipoFilter,
    productid,
    limit: 200,
  });
  const create = useCreateMovimentacao();
  const del = useDeleteMovimentacao();

  const invalidate = () => {
    qc.invalidateQueries({ queryKey: getListMovimentacoesQueryKey() });
    qc.invalidateQueries({ queryKey: getListEstoqueQueryKey() });
    qc.invalidateQueries({ queryKey: getGetDashboardOverviewQueryKey() });
    qc.invalidateQueries({ queryKey: getGetSaldoQueryKey() });
    qc.invalidateQueries({ queryKey: getGetSalesTrendQueryKey() });
    qc.invalidateQueries({ queryKey: getGetTopProductsQueryKey() });
    qc.invalidateQueries({ queryKey: getGetRecentActivityQueryKey() });
  };

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-start gap-4 flex-wrap">
        <div>
          <h2 className="text-3xl font-serif font-bold tracking-tight">Movimentações</h2>
          <p className="text-muted-foreground mt-1">Histórico completo de entradas e saídas</p>
        </div>
        <div className="flex gap-2">
          <Button variant="outline" onClick={() => setOpenTipo("Entrada")}>
            <ArrowDownToLine className="mr-2 h-4 w-4" />Nova Entrada
          </Button>
          <Button onClick={() => setOpenTipo("Saída")}>
            <ArrowUpFromLine className="mr-2 h-4 w-4" />Nova Venda
          </Button>
        </div>
      </div>

      <div className="flex flex-wrap gap-3 items-end">
        <div>
          <Label>Tipo</Label>
          <Select value={tipoFilter} onValueChange={(v) => setTipoFilter(v as any)}>
            <SelectTrigger className="w-40"><SelectValue /></SelectTrigger>
            <SelectContent>
              <SelectItem value="All">Todos</SelectItem>
              <SelectItem value="Entrada">Entradas</SelectItem>
              <SelectItem value="Saída">Saídas</SelectItem>
            </SelectContent>
          </Select>
        </div>
        <div>
          <Label>Produto</Label>
          <Select value={productFilter} onValueChange={setProductFilter}>
            <SelectTrigger className="w-72"><SelectValue /></SelectTrigger>
            <SelectContent>
              <SelectItem value="all">Todos os produtos</SelectItem>
              {products?.map((p) => (
                <SelectItem key={p.productid} value={String(p.productid)}>
                  {p.bebida} {p.tamanho}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>
      </div>

      <div className="border rounded-md bg-card">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Data</TableHead>
              <TableHead>Tipo</TableHead>
              <TableHead>Produto</TableHead>
              <TableHead>Responsável</TableHead>
              <TableHead className="text-right">Qtd</TableHead>
              <TableHead className="text-right">Valor unit.</TableHead>
              <TableHead className="text-right">Total</TableHead>
              <TableHead />
            </TableRow>
          </TableHeader>
          <TableBody>
            {isLoading ? (
              <TableRow><TableCell colSpan={8} className="text-center py-8 text-muted-foreground">Carregando...</TableCell></TableRow>
            ) : !rows || rows.length === 0 ? (
              <TableRow><TableCell colSpan={8} className="text-center py-8 text-muted-foreground">Nenhuma movimentação.</TableCell></TableRow>
            ) : rows.map((m) => (
              <TableRow key={m.id}>
                <TableCell>{formatDateTime(m.data)}</TableCell>
                <TableCell>
                  <Badge variant={m.tipo === "Entrada" ? "secondary" : "default"}>
                    {m.tipo}{m.tipo_venda ? ` • ${m.tipo_venda}` : ""}
                  </Badge>
                </TableCell>
                <TableCell className="font-medium">{m.produto}</TableCell>
                <TableCell>{m.responsavel}{m.saida ? <span className="text-muted-foreground"> • {m.saida}</span> : null}</TableCell>
                <TableCell className="text-right">{m.quantidade}</TableCell>
                <TableCell className="text-right">{formatCurrency(m.valor_unitario)}</TableCell>
                <TableCell className="text-right font-semibold">{formatCurrency(m.valor_total ?? 0)}</TableCell>
                <TableCell className="text-right">
                  <Button variant="ghost" size="icon" onClick={() => setConfirmDelete(m.id)}><Trash2 className="h-4 w-4" /></Button>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>

      {openTipo && (
        <MovimentacaoDialog
          tipo={openTipo}
          products={products ?? []}
          defaultResponsavel={responsavel}
          onClose={() => setOpenTipo(null)}
          onSubmit={(payload) => {
            create.mutate({ data: payload }, {
              onSuccess: () => { invalidate(); setOpenTipo(null); toast({ title: openTipo === "Entrada" ? "Entrada registrada" : "Venda registrada" }); },
              onError: (e: any) => toast({ title: "Erro", description: String(e?.message ?? e), variant: "destructive" }),
            });
          }}
          submitting={create.isPending}
        />
      )}

      <Dialog open={confirmDelete != null} onOpenChange={(o) => !o && setConfirmDelete(null)}>
        <DialogContent>
          <DialogHeader><DialogTitle>Remover movimentação?</DialogTitle></DialogHeader>
          <p className="text-sm text-muted-foreground">Esta ação ajusta o estoque retroativamente.</p>
          <DialogFooter>
            <Button variant="ghost" onClick={() => setConfirmDelete(null)}>Cancelar</Button>
            <Button variant="destructive" disabled={del.isPending} onClick={() => {
              if (confirmDelete == null) return;
              del.mutate({ id: confirmDelete }, {
                onSuccess: () => { invalidate(); setConfirmDelete(null); toast({ title: "Removida" }); },
                onError: (e: any) => toast({ title: "Erro", description: String(e?.message ?? e), variant: "destructive" }),
              });
            }}>Remover</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}

function MovimentacaoDialog({
  tipo, products, defaultResponsavel, onClose, onSubmit, submitting,
}: {
  tipo: Tipo;
  products: Array<{ productid: number; bebida: string; tamanho: string; valor: number; valor_venda: number; valor_caixa: number; valor_atacado_caixa: number; quantidade_caixa: number }>;
  defaultResponsavel: string;
  onClose: () => void;
  onSubmit: (p: any) => void;
  submitting?: boolean;
}) {
  const isSale = tipo === "Saída";
  const [productid, setProductid] = useState<string>("");
  const [tipoVenda, setTipoVenda] = useState<"Varejo" | "Atacado">("Varejo");
  const [quantidade, setQuantidade] = useState<number>(1);
  const [valorUnit, setValorUnit] = useState<number>(0);
  const [responsavel, setResponsavel] = useState<string>(defaultResponsavel);
  const [saida, setSaida] = useState<string>("");
  const [observacoes, setObservacoes] = useState<string>("");

  const [discount, setDiscount] = useState<number>(0);
  const [pwOpen, setPwOpen] = useState(false);
  const [pw, setPw] = useState("");
  const [discountUnlocked, setDiscountUnlocked] = useState(false);
  const verify = useVerifyDiscountPassword();
  const { toast } = useToast();

  const product = products.find((p) => String(p.productid) === productid);

  // auto-pick default unit price
  useMemo(() => {
    if (!product) return;
    if (isSale) {
      setValorUnit(tipoVenda === "Atacado" ? (product.valor_atacado_caixa || product.valor_caixa) : product.valor_venda);
    } else {
      setValorUnit(product.valor);
    }
  }, [product, tipoVenda, isSale]);

  const finalUnit = isSale && discountUnlocked ? Math.max(0, valorUnit - discount) : valorUnit;
  const total = finalUnit * quantidade;

  return (
    <>
      <Dialog open onOpenChange={(o) => !o && onClose()}>
        <DialogContent className="max-w-xl">
          <DialogHeader>
            <DialogTitle>{isSale ? "Nova Venda (Saída)" : "Nova Entrada (Compra)"}</DialogTitle>
          </DialogHeader>
          <form
            className="space-y-3"
            onSubmit={(e) => {
              e.preventDefault();
              if (!product) return;
              onSubmit({
                tipo, tipo_venda: isSale ? tipoVenda : "Varejo",
                productid: product.productid, quantidade,
                responsavel: responsavel || "—",
                saida: isSale ? (saida || null) : null,
                valor_unitario: finalUnit,
                observacoes: observacoes || null,
              });
            }}
          >
            <div>
              <Label>Produto</Label>
              <Select value={productid} onValueChange={setProductid}>
                <SelectTrigger><SelectValue placeholder="Selecione um produto" /></SelectTrigger>
                <SelectContent>
                  {products.map((p) => (
                    <SelectItem key={p.productid} value={String(p.productid)}>{p.bebida} {p.tamanho}</SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            {isSale && (
              <div>
                <Label>Tipo de venda</Label>
                <Select value={tipoVenda} onValueChange={(v) => setTipoVenda(v as any)}>
                  <SelectTrigger><SelectValue /></SelectTrigger>
                  <SelectContent>
                    <SelectItem value="Varejo">Varejo (unidade)</SelectItem>
                    <SelectItem value="Atacado">Atacado (caixa)</SelectItem>
                  </SelectContent>
                </Select>
              </div>
            )}
            <div className="grid grid-cols-2 gap-3">
              <div>
                <Label>{isSale && tipoVenda === "Atacado" ? "Caixas" : "Quantidade"}</Label>
                <Input type="number" min={1} value={quantidade} onChange={(e) => setQuantidade(Number(e.target.value) || 1)} />
                {isSale && tipoVenda === "Atacado" && product && (
                  <p className="text-xs text-muted-foreground mt-1">
                    Debita {quantidade * (product.quantidade_caixa || 1)} unid. do estoque ({product.quantidade_caixa} un./caixa)
                  </p>
                )}
              </div>
              <div>
                <Label>{isSale && tipoVenda === "Atacado" ? "Valor por caixa (R$)" : "Valor unitário (R$)"}</Label>
                <Input type="number" step="0.01" value={valorUnit} onChange={(e) => setValorUnit(Number(e.target.value))} />
              </div>
            </div>

            {isSale && (
              <div className="rounded-md border bg-muted/40 px-3 py-2 space-y-2">
                <div className="flex items-center justify-between">
                  <Label className="text-xs uppercase tracking-wide">Desconto por unidade</Label>
                  {!discountUnlocked ? (
                    <Button type="button" variant="ghost" size="sm" onClick={() => setPwOpen(true)}>
                      <Lock className="h-3.5 w-3.5 mr-1" /> Liberar
                    </Button>
                  ) : (
                    <Badge variant="secondary">Liberado</Badge>
                  )}
                </div>
                <Input
                  type="number" step="0.01" min={0} disabled={!discountUnlocked}
                  value={discount} onChange={(e) => setDiscount(Number(e.target.value) || 0)}
                />
              </div>
            )}

            <div className="grid grid-cols-2 gap-3">
              <div>
                <Label>Responsável</Label>
                <Input value={responsavel} onChange={(e) => setResponsavel(e.target.value)} placeholder="Vendedor" />
              </div>
              {isSale && (
                <div>
                  <Label>Cliente / Saída</Label>
                  <Input value={saida} onChange={(e) => setSaida(e.target.value)} placeholder="Mesa, balcão, cliente..." />
                </div>
              )}
            </div>

            <div>
              <Label>Observações</Label>
              <Textarea value={observacoes} onChange={(e) => setObservacoes(e.target.value)} rows={2} />
            </div>

            <div className="rounded-md bg-primary/10 px-4 py-3 flex items-center justify-between">
              <span className="text-sm text-muted-foreground">Total</span>
              <span className="text-2xl font-bold text-primary">{formatCurrency(total)}</span>
            </div>

            <DialogFooter>
              <Button type="button" variant="ghost" onClick={onClose}>Cancelar</Button>
              <Button type="submit" disabled={!product || quantidade < 1 || submitting}>
                {submitting ? "Salvando..." : "Confirmar"}
              </Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>

      <Dialog open={pwOpen} onOpenChange={setPwOpen}>
        <DialogContent className="max-w-sm">
          <DialogHeader><DialogTitle>Senha do gerente</DialogTitle></DialogHeader>
          <p className="text-sm text-muted-foreground">Informe a senha para liberar descontos manuais.</p>
          <Input type="password" value={pw} onChange={(e) => setPw(e.target.value)} autoFocus />
          <DialogFooter>
            <Button variant="ghost" onClick={() => { setPwOpen(false); setPw(""); }}>Cancelar</Button>
            <Button
              disabled={verify.isPending || !pw}
              onClick={() => verify.mutate({ data: { password: pw } }, {
                onSuccess: (r) => {
                  if (r.valid) { setDiscountUnlocked(true); setPwOpen(false); setPw(""); toast({ title: "Desconto liberado" }); }
                  else { toast({ title: "Senha incorreta", variant: "destructive" }); }
                },
                onError: () => toast({ title: "Falha ao validar", variant: "destructive" }),
              })}
            >Liberar</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </>
  );
}
