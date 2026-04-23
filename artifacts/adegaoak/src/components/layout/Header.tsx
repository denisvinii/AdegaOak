import { useListFuncionarios, useGetDashboardOverview } from "@workspace/api-client-react";
import { useResponsavel } from "@/hooks/use-responsavel";
import { useTheme } from "@/hooks/use-theme";
import { formatCurrency } from "@/lib/format";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Skeleton } from "@/components/ui/skeleton";
import { Button } from "@/components/ui/button";
import { Moon, Sun, User, Wallet } from "lucide-react";

export function Header() {
  const { data: funcionarios, isLoading: loadingFunc } = useListFuncionarios();
  const { data: dashboard, isLoading: loadingDash } = useGetDashboardOverview();
  const { responsavel, setResponsavel } = useResponsavel();
  const { theme, toggle } = useTheme();

  return (
    <header className="h-16 border-b bg-card px-6 flex items-center justify-between shrink-0">
      <div className="flex items-center gap-4">
        <div className="flex items-center gap-2 text-muted-foreground">
          <Wallet className="h-4 w-4" />
          <span className="text-sm font-medium">Vendas Hoje:</span>
        </div>
        {loadingDash ? (
          <Skeleton className="h-6 w-24" />
        ) : (
          <span className="text-lg font-bold text-primary">
            {formatCurrency(dashboard?.vendas_hoje || 0)}
          </span>
        )}
      </div>

      <div className="flex items-center gap-3">
        <Button variant="ghost" size="icon" onClick={toggle} aria-label="Alternar tema" title={theme === "dark" ? "Tema claro" : "Tema escuro"}>
          {theme === "dark" ? <Sun className="h-4 w-4" /> : <Moon className="h-4 w-4" />}
        </Button>
        <div className="flex items-center gap-2 text-muted-foreground">
          <User className="h-4 w-4" />
          <span className="text-sm font-medium">Vendedor:</span>
        </div>
        {loadingFunc ? (
          <Skeleton className="h-9 w-[180px]" />
        ) : (
          <Select value={responsavel} onValueChange={setResponsavel}>
            <SelectTrigger className="w-[180px]">
              <SelectValue placeholder="Selecione..." />
            </SelectTrigger>
            <SelectContent>
              {funcionarios?.filter(f => f.ativo).map((f) => (
                <SelectItem key={f.id} value={f.username}>
                  {f.username}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        )}
      </div>
    </header>
  );
}
