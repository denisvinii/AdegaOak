import { Link, useLocation } from "wouter";
import { 
  LayoutDashboard, 
  Package, 
  ArrowRightLeft, 
  Tags, 
  Layers, 
  ShoppingCart, 
  Receipt, 
  Wallet, 
  Users
} from "lucide-react";
import { cn } from "@/lib/utils";

const navItems = [
  { href: "/", label: "Painel", icon: LayoutDashboard },
  { href: "/estoque", label: "Estoque", icon: Package },
  { href: "/movimentacoes", label: "Movimentações", icon: ArrowRightLeft },
  { href: "/precos", label: "Tabela de Preços", icon: Tags },
  { href: "/combos", label: "Combos", icon: Layers },
  { href: "/vendas-combo", label: "Venda de Combos", icon: ShoppingCart },
  { href: "/despesas", label: "Despesas", icon: Receipt },
  { href: "/saldo", label: "Saldo", icon: Wallet },
  { href: "/funcionarios", label: "Funcionários", icon: Users },
];

export function Sidebar() {
  const [location] = useLocation();

  return (
    <aside className="w-64 border-r bg-card flex flex-col h-full">
      <div className="p-6 border-b">
        <h1 className="text-2xl font-serif font-bold text-primary tracking-tight">
          Adega Oak
        </h1>
        <p className="text-sm text-muted-foreground font-medium mt-1">
          Gestão e Vendas
        </p>
      </div>
      <nav className="flex-1 overflow-y-auto py-4 px-3 space-y-1">
        {navItems.map((item) => {
          const isActive = location === item.href;
          return (
            <Link key={item.href} href={item.href}>
              <div
                className={cn(
                  "flex items-center gap-3 px-3 py-2.5 rounded-md text-sm font-medium transition-colors cursor-pointer",
                  isActive
                    ? "bg-primary text-primary-foreground"
                    : "text-muted-foreground hover:bg-muted hover:text-foreground"
                )}
              >
                <item.icon className="h-4 w-4" />
                {item.label}
              </div>
            </Link>
          );
        })}
      </nav>
    </aside>
  );
}
