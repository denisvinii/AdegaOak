import { useEffect, useState } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";

export type ProductFormValues = {
  bebida: string;
  tamanho: string;
  material: string;
  valor: number;
  valor_venda: number;
  quantidade_caixa: number;
  valor_caixa: number;
  valor_atacado_caixa: number;
  estoque_minimo: number;
  quantidade_minima_atacado: number;
};

const empty: ProductFormValues = {
  bebida: "", tamanho: "", material: "Garrafa",
  valor: 0, valor_venda: 0,
  quantidade_caixa: 12, valor_caixa: 0, valor_atacado_caixa: 0,
  estoque_minimo: 5, quantidade_minima_atacado: 20,
};

export function ProductForm({
  initial, onSubmit, submitting, onCancel,
}: {
  initial?: Partial<ProductFormValues>;
  onSubmit: (v: ProductFormValues) => void;
  submitting?: boolean;
  onCancel?: () => void;
}) {
  const [v, setV] = useState<ProductFormValues>({ ...empty, ...initial });
  useEffect(() => { setV({ ...empty, ...initial }); }, [JSON.stringify(initial)]);
  const set = <K extends keyof ProductFormValues>(k: K, val: ProductFormValues[K]) => setV((s) => ({ ...s, [k]: val }));

  return (
    <form
      className="space-y-4"
      onSubmit={(e) => { e.preventDefault(); onSubmit(v); }}
    >
      <div className="grid grid-cols-2 gap-3">
        <div className="col-span-2">
          <Label>Bebida</Label>
          <Input value={v.bebida} onChange={(e) => set("bebida", e.target.value)} required />
        </div>
        <div>
          <Label>Tamanho</Label>
          <Input value={v.tamanho} onChange={(e) => set("tamanho", e.target.value)} placeholder="600ml" required />
        </div>
        <div>
          <Label>Material</Label>
          <Input value={v.material} onChange={(e) => set("material", e.target.value)} placeholder="Garrafa / Lata / PET" required />
        </div>
        <div>
          <Label>Custo (R$)</Label>
          <Input type="number" step="0.01" value={v.valor} onChange={(e) => set("valor", Number(e.target.value))} required />
        </div>
        <div>
          <Label>Venda Varejo (R$)</Label>
          <Input type="number" step="0.01" value={v.valor_venda} onChange={(e) => set("valor_venda", Number(e.target.value))} required />
        </div>
        <div>
          <Label>Qtd. por Caixa</Label>
          <Input type="number" value={v.quantidade_caixa} onChange={(e) => set("quantidade_caixa", Number(e.target.value))} />
        </div>
        <div>
          <Label>Valor da Caixa (R$)</Label>
          <Input type="number" step="0.01" value={v.valor_caixa} onChange={(e) => set("valor_caixa", Number(e.target.value))} />
        </div>
        <div>
          <Label>Valor Atacado/Caixa (R$)</Label>
          <Input type="number" step="0.01" value={v.valor_atacado_caixa} onChange={(e) => set("valor_atacado_caixa", Number(e.target.value))} />
        </div>
        <div>
          <Label>Estoque Mínimo</Label>
          <Input type="number" value={v.estoque_minimo} onChange={(e) => set("estoque_minimo", Number(e.target.value))} />
        </div>
        <div className="col-span-2">
          <Label>Mín. para Atacado</Label>
          <Input type="number" value={v.quantidade_minima_atacado} onChange={(e) => set("quantidade_minima_atacado", Number(e.target.value))} />
        </div>
      </div>
      <div className="flex justify-end gap-2 pt-2">
        {onCancel && <Button type="button" variant="ghost" onClick={onCancel}>Cancelar</Button>}
        <Button type="submit" disabled={submitting}>{submitting ? "Salvando..." : "Salvar"}</Button>
      </div>
    </form>
  );
}
