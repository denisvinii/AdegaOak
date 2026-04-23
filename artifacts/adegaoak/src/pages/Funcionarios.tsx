import { useState } from "react";
import {
  useListFuncionarios, useCreateFuncionario, useDeleteFuncionario,
  getListFuncionariosQueryKey,
} from "@workspace/api-client-react";
import { useQueryClient } from "@tanstack/react-query";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Badge } from "@/components/ui/badge";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from "@/components/ui/dialog";
import { useToast } from "@/hooks/use-toast";
import { Trash2 } from "lucide-react";

export default function Funcionarios() {
  const qc = useQueryClient();
  const { toast } = useToast();
  const [open, setOpen] = useState(false);
  const [username, setUsername] = useState("");
  const [confirmDelete, setConfirmDelete] = useState<number | null>(null);

  const { data: list, isLoading } = useListFuncionarios();
  const create = useCreateFuncionario();
  const del = useDeleteFuncionario();

  const invalidate = () => qc.invalidateQueries({ queryKey: getListFuncionariosQueryKey() });

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h2 className="text-3xl font-serif font-bold tracking-tight">Funcionários</h2>
          <p className="text-muted-foreground mt-1">Vendedores que aparecem no seletor do balcão</p>
        </div>
        <Button onClick={() => { setUsername(""); setOpen(true); }}>Novo Funcionário</Button>
      </div>

      <div className="border rounded-md bg-card">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Usuário</TableHead>
              <TableHead>Status</TableHead>
              <TableHead className="w-32" />
            </TableRow>
          </TableHeader>
          <TableBody>
            {isLoading ? (
              <TableRow><TableCell colSpan={3} className="text-center py-8 text-muted-foreground">Carregando...</TableCell></TableRow>
            ) : !list || list.length === 0 ? (
              <TableRow><TableCell colSpan={3} className="text-center py-8 text-muted-foreground">Nenhum funcionário.</TableCell></TableRow>
            ) : list.map((f) => (
              <TableRow key={f.id}>
                <TableCell className="font-medium">{f.username}</TableCell>
                <TableCell><Badge variant={f.ativo ? "secondary" : "outline"}>{f.ativo ? "Ativo" : "Inativo"}</Badge></TableCell>
                <TableCell className="text-right">
                  <Button variant="ghost" size="icon" onClick={() => setConfirmDelete(f.id)}><Trash2 className="h-4 w-4" /></Button>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>

      <Dialog open={open} onOpenChange={setOpen}>
        <DialogContent className="max-w-sm">
          <DialogHeader><DialogTitle>Novo funcionário</DialogTitle></DialogHeader>
          <form
            onSubmit={(e) => {
              e.preventDefault();
              create.mutate({ data: { username } }, {
                onSuccess: () => { invalidate(); setOpen(false); toast({ title: "Criado" }); },
                onError: (e: any) => toast({ title: "Erro", description: String(e?.message ?? e), variant: "destructive" }),
              });
            }}
            className="space-y-3"
          >
            <div>
              <Label>Usuário</Label>
              <Input value={username} onChange={(e) => setUsername(e.target.value)} required minLength={2} />
            </div>
            <DialogFooter>
              <Button variant="ghost" type="button" onClick={() => setOpen(false)}>Cancelar</Button>
              <Button type="submit" disabled={create.isPending}>Salvar</Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>

      <Dialog open={confirmDelete != null} onOpenChange={(o) => !o && setConfirmDelete(null)}>
        <DialogContent>
          <DialogHeader><DialogTitle>Remover funcionário?</DialogTitle></DialogHeader>
          <DialogFooter>
            <Button variant="ghost" onClick={() => setConfirmDelete(null)}>Cancelar</Button>
            <Button variant="destructive" disabled={del.isPending} onClick={() => {
              if (confirmDelete == null) return;
              del.mutate({ id: confirmDelete }, {
                onSuccess: () => { invalidate(); setConfirmDelete(null); toast({ title: "Removido" }); },
              });
            }}>Remover</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
