import { 
  useGetDashboardOverview, 
  useGetSalesTrend, 
  useGetTopProducts, 
  useGetRecentActivity,
  useListEstoque
} from "@workspace/api-client-react";
import { formatCurrency, formatDateTime } from "@/lib/format";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { 
  TrendingUp, 
  Package, 
  AlertTriangle, 
  Receipt,
  ArrowRightLeft,
  Calendar,
  Wallet,
  Layers,
  ArrowDownIcon,
  ArrowUpIcon
} from "lucide-react";
import { Link } from "wouter";
import { Badge } from "@/components/ui/badge";

export default function Dashboard() {
  const { data: overview, isLoading: loadingOverview } = useGetDashboardOverview();
  const { data: recentActivity, isLoading: loadingActivity } = useGetRecentActivity({ limit: 10 });
  const { data: lowStock, isLoading: loadingLowStock } = useListEstoque({ low: true });
  const { data: topProducts, isLoading: loadingTop } = useGetTopProducts({ days: 30, limit: 5 });

  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-3xl font-serif font-bold tracking-tight">Painel de Controle</h2>
        <p className="text-muted-foreground mt-1">Resumo das atividades da adega</p>
      </div>

      {/* KPI Cards */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        <KpiCard 
          title="Vendas Hoje" 
          value={overview?.vendas_hoje} 
          icon={TrendingUp} 
          loading={loadingOverview}
          isCurrency
        />
        <KpiCard 
          title="Vendas no Mês" 
          value={overview?.vendas_mes} 
          icon={Calendar} 
          loading={loadingOverview}
          isCurrency
        />
        <KpiCard 
          title="Produtos Baixo Estoque" 
          value={overview?.produtos_baixo_estoque} 
          icon={AlertTriangle} 
          loading={loadingOverview}
          alert={overview && overview.produtos_baixo_estoque > 0}
        />
        <KpiCard 
          title="Valor em Estoque (Custo)" 
          value={overview?.valor_estoque} 
          icon={Wallet} 
          loading={loadingOverview}
          isCurrency
        />
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Recent Activity */}
        <Card className="lg:col-span-2">
          <CardHeader>
            <CardTitle className="text-lg flex items-center gap-2">
              <ArrowRightLeft className="h-5 w-5 text-muted-foreground" />
              Atividades Recentes
            </CardTitle>
          </CardHeader>
          <CardContent>
            {loadingActivity ? (
              <div className="space-y-4">
                {Array.from({ length: 5 }).map((_, i) => (
                  <Skeleton key={i} className="h-12 w-full" />
                ))}
              </div>
            ) : (
              <div className="space-y-4">
                {recentActivity?.map((activity) => (
                  <div key={activity.id} className="flex items-center justify-between border-b pb-4 last:border-0 last:pb-0">
                    <div className="flex items-center gap-4">
                      <div className="p-2 bg-muted rounded-full">
                        {activity.kind === 'venda' ? <TrendingUp className="h-4 w-4 text-primary" /> :
                         activity.kind === 'entrada' ? <Package className="h-4 w-4 text-emerald-600" /> :
                         activity.kind === 'combo' ? <Layers className="h-4 w-4 text-amber-500" /> :
                         <Receipt className="h-4 w-4 text-rose-500" />}
                      </div>
                      <div>
                        <p className="font-medium text-sm">{activity.title}</p>
                        <p className="text-xs text-muted-foreground">{activity.subtitle}</p>
                      </div>
                    </div>
                    <div className="text-right">
                      <p className="font-bold text-sm">
                        {activity.kind === 'despesa' ? '-' : ''}{formatCurrency(activity.amount)}
                      </p>
                      <p className="text-xs text-muted-foreground">
                        {formatDateTime(activity.when)}
                      </p>
                    </div>
                  </div>
                ))}
                {recentActivity?.length === 0 && (
                  <div className="text-center py-6 text-muted-foreground text-sm">
                    Nenhuma atividade recente.
                  </div>
                )}
              </div>
            )}
          </CardContent>
        </Card>

        {/* Low Stock Alerts */}
        <Card className="border-rose-200 dark:border-rose-900">
          <CardHeader className="bg-rose-50 dark:bg-rose-950/20 pb-4">
            <CardTitle className="text-lg flex items-center gap-2 text-rose-700 dark:text-rose-400">
              <AlertTriangle className="h-5 w-5" />
              Alerta de Estoque
            </CardTitle>
          </CardHeader>
          <CardContent className="pt-4">
            {loadingLowStock ? (
              <div className="space-y-3">
                {Array.from({ length: 3 }).map((_, i) => (
                  <Skeleton key={i} className="h-10 w-full" />
                ))}
              </div>
            ) : (
              <div className="space-y-3">
                {lowStock?.slice(0, 6).map((item) => (
                  <div key={item.productid} className="flex justify-between items-center text-sm">
                    <span className="font-medium truncate pr-2" title={item.bebida}>{item.bebida}</span>
                    <Badge variant="destructive" className="shrink-0">
                      {item.quantidade} restam
                    </Badge>
                  </div>
                ))}
                {lowStock?.length === 0 && (
                  <p className="text-sm text-muted-foreground text-center py-4">
                    Estoque em dia. Nenhum alerta.
                  </p>
                )}
                {lowStock && lowStock.length > 6 && (
                  <div className="pt-2 text-center">
                    <Link href="/estoque?low=true" className="text-sm text-primary hover:underline font-medium">
                      Ver todos ({lowStock.length})
                    </Link>
                  </div>
                )}
              </div>
            )}
          </CardContent>
        </Card>
      </div>
    </div>
  );
}

function KpiCard({ title, value, icon: Icon, loading, isCurrency = false, alert = false }: any) {
  return (
    <Card className={alert ? "border-rose-300 dark:border-rose-800" : ""}>
      <CardHeader className="flex flex-row items-center justify-between pb-2">
        <CardTitle className="text-sm font-medium text-muted-foreground">
          {title}
        </CardTitle>
        <Icon className={`h-4 w-4 ${alert ? "text-rose-500" : "text-muted-foreground"}`} />
      </CardHeader>
      <CardContent>
        {loading ? (
          <Skeleton className="h-8 w-24" />
        ) : (
          <div className={`text-2xl font-bold ${alert ? "text-rose-600 dark:text-rose-400" : ""}`}>
            {isCurrency ? formatCurrency(value || 0) : (value || 0)}
          </div>
        )}
      </CardContent>
    </Card>
  );
}
