'use client';

import { useEffect, useState } from 'react';
import api from '@/lib/api';
import { TrendingUp, TrendingDown, Package, DollarSign, AlertTriangle, RefreshCw, Percent } from 'lucide-react';
import type { DashboardDto, FiltrosDashboardRequest, ProdutoMargemDto } from '@/types/dashboard';

export default function DashboardPage() {
  const [dashboard, setDashboard] = useState<DashboardDto | null>(null);
  const [produtosMargem, setProdutosMargem] = useState<ProdutoMargemDto[]>([]);
  const [filtroMargem, setFiltroMargem] = useState<'menor' | 'maior'>('menor');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadDashboard();
    loadProdutosMargem();
  }, []);

  useEffect(() => {
    loadProdutosMargem();
  }, [filtroMargem]);

  const loadDashboard = async () => {
    try {
      setLoading(true);
      setError(null);
      
      const filtros: FiltrosDashboardRequest = {
        dataInicio: new Date(Date.now() - 30 * 24 * 60 * 60 * 1000).toISOString(),
        dataFim: new Date().toISOString(),
      };
      
      const { data } = await api.post<DashboardDto>('/dashboard', filtros);
      
      setDashboard(data);
    } catch (err: any) {
      console.error('Erro ao carregar dashboard:', err);
      
      let errorMessage = 'Erro ao carregar dados do dashboard';
      
      if (err.response) {
        const status = err.response.status;
        if (status === 401) {
          errorMessage = 'Não autorizado. Por favor, faça login novamente.';
        } else if (status === 403) {
          errorMessage = 'Acesso negado. Você precisa ser administrador para acessar o dashboard.';
        } else if (status === 500) {
          errorMessage = err.response.data?.message || err.response.data || 'Erro interno do servidor. Verifique se o banco de dados está configurado corretamente.';
        } else {
          errorMessage = err.response.data?.message || err.response.data || errorMessage;
        }
      } else if (err.request) {
        errorMessage = 'Não foi possível conectar ao servidor. Verifique se o backend está rodando.';
      } else {
        errorMessage = err.message || errorMessage;
      }
      
      setError(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  const loadProdutosMargem = async () => {
    try {
      const { data } = await api.get<ProdutoMargemDto[]>(`/dashboard/produtos-margem?ordenacao=${filtroMargem}`);
      setProdutosMargem(data);
    } catch (err) {
      console.error('Erro ao carregar produtos por margem:', err);
    }
  };

  if (loading) {
    return (
      <div className="space-y-6">
        <div>
          <h1 className="text-3xl font-bold text-gray-900 dark:text-white">Dashboard</h1>
          <p className="text-gray-600 dark:text-gray-400 mt-2">Visão geral do negócio</p>
        </div>
        <div className="flex items-center justify-center py-20">
          <div className="text-center">
            <div className="animate-spin rounded-full h-16 w-16 border-b-4 border-amber-600 mx-auto mb-4"></div>
            <p className="text-gray-600 dark:text-gray-400">Carregando dados...</p>
          </div>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="space-y-6">
        <div>
          <h1 className="text-3xl font-bold text-gray-900 dark:text-white">Dashboard</h1>
          <p className="text-gray-600 dark:text-gray-400 mt-2">Visão geral do negócio</p>
        </div>
        <div className="bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-xl p-6">
          <div className="flex items-start gap-4">
            <AlertTriangle className="text-red-600 dark:text-red-400 flex-shrink-0" size={24} />
            <div className="flex-1">
              <h3 className="font-semibold text-red-900 dark:text-red-200 mb-2">
                Erro ao Carregar Dashboard
              </h3>
              <p className="text-sm text-red-800 dark:text-red-300 mb-4">
                {error}
              </p>
              <button
                onClick={loadDashboard}
                className="inline-flex items-center gap-2 px-4 py-2 bg-red-600 hover:bg-red-700 text-white rounded-lg text-sm transition-colors"
              >
                <RefreshCw size={16} />
                Tentar Novamente
              </button>
            </div>
          </div>
        </div>
      </div>
    );
  }

  if (!dashboard) {
    return (
      <div className="space-y-6">
        <div>
          <h1 className="text-3xl font-bold text-gray-900 dark:text-white">Dashboard</h1>
          <p className="text-gray-600 dark:text-gray-400 mt-2">Visão geral do negócio</p>
        </div>
        <div className="bg-gray-50 dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-xl p-8 text-center">
          <Package className="mx-auto text-gray-400 dark:text-gray-600 mb-4" size={48} />
          <p className="text-gray-600 dark:text-gray-400 mb-4">Nenhum dado disponível</p>
          <button
            onClick={loadDashboard}
            className="inline-flex items-center gap-2 px-4 py-2 bg-amber-600 hover:bg-amber-700 text-white rounded-lg text-sm transition-colors"
          >
            <RefreshCw size={16} />
            Recarregar
          </button>
        </div>
      </div>
    );
  }

  const formatCurrency = (value: number): string => {
    return new Intl.NumberFormat('pt-BR', {
      style: 'currency',
      currency: 'BRL',
    }).format(value);
  };

  const stats = [
    {
      label: 'Receita do Mês',
      value: formatCurrency(dashboard.receitaMes),
      icon: TrendingUp,
      color: 'bg-green-500',
      textColor: 'text-green-600 dark:text-green-400',
    },
    {
      label: 'Despesas do Mês',
      value: formatCurrency(dashboard.despesasMes),
      icon: TrendingDown,
      color: 'bg-red-500',
      textColor: 'text-red-600 dark:text-red-400',
    },
    {
      label: 'Saldo Atual',
      value: formatCurrency(dashboard.saldo.saldo),
      icon: DollarSign,
      color: 'bg-blue-500',
      textColor: 'text-blue-600 dark:text-blue-400',
    },
    {
      label: 'Movimentações',
      value: dashboard.totalMovimentacoesMes.toString(),
      icon: Package,
      color: 'bg-amber-500',
      textColor: 'text-amber-600 dark:text-amber-400',
    },
  ];

  return (
    <div className="space-y-4 md:space-y-6 lg:space-y-8">
      <div>
        <h1 className="text-2xl md:text-3xl font-bold text-gray-900 dark:text-white">Dashboard</h1>
        <p className="text-sm md:text-base text-gray-600 dark:text-gray-400 mt-1 md:mt-2">
          Visão geral do negócio
        </p>
      </div>

      {/* Stats Grid */}
      <div className="grid grid-cols-2 lg:grid-cols-4 gap-3 md:gap-4 lg:gap-6">
        {stats.map((stat) => {
          const Icon = stat.icon;
          return (
            <div
              key={stat.label}
              className="bg-white dark:bg-gray-800 rounded-lg md:rounded-xl shadow-sm p-4 md:p-6 border border-gray-100 dark:border-gray-700 hover:shadow-md transition-shadow"
            >
              <div className="flex flex-col md:flex-row md:items-center md:justify-between gap-3">
                <div className="flex-1 min-w-0">
                  <p className="text-xs md:text-sm font-medium text-gray-600 dark:text-gray-400 mb-1 md:mb-2 truncate">
                    {stat.label}
                  </p>
                  <p className={`text-lg md:text-2xl font-bold ${stat.textColor} truncate`}>
                    {stat.value}
                  </p>
                </div>
                <div className={`${stat.color} p-2 md:p-3 rounded-lg flex-shrink-0 self-start md:self-auto`}>
                  <Icon className="text-white" size={20} />
                </div>
              </div>
            </div>
          );
        })}
      </div>

      {/* Top Products */}
      <div className="bg-white dark:bg-gray-800 rounded-lg md:rounded-xl shadow-sm p-4 md:p-6 border border-gray-100 dark:border-gray-700">
        <h2 className="text-lg md:text-xl font-bold text-gray-900 dark:text-white mb-4 md:mb-6">
          🏆 Top 10 Produtos Mais Vendidos
        </h2>
        {dashboard.topProdutos.length > 0 ? (
          <div className="space-y-2 md:space-y-3">
            {dashboard.topProdutos.map((produto, index) => (
              <div
                key={produto.produtoId}
                className="flex items-center justify-between p-3 md:p-4 bg-gray-50 dark:bg-gray-700/50 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors"
              >
                <div className="flex items-center gap-2 md:gap-4 flex-1 min-w-0">
                  <div className="flex items-center justify-center w-6 h-6 md:w-8 md:h-8 rounded-full bg-amber-100 dark:bg-amber-900/30 flex-shrink-0">
                    <span className="font-bold text-xs md:text-sm text-amber-600 dark:text-amber-400">
                      {index + 1}
                    </span>
                  </div>
                  <span className="font-medium text-sm md:text-base text-gray-900 dark:text-white truncate">
                    {produto.descricao}
                  </span>
                </div>
                <div className="text-right flex-shrink-0 ml-2">
                  <p className="font-bold text-sm md:text-base text-gray-900 dark:text-white">
                    {formatCurrency(produto.valorTotal)}
                  </p>
                  <p className="text-xs md:text-sm text-gray-600 dark:text-gray-400">
                    {produto.quantidadeVendida} {produto.quantidadeVendida === 1 ? 'un' : 'uns'}
                  </p>
                </div>
              </div>
            ))}
          </div>
        ) : (
          <div className="text-center py-8 md:py-12">
            <Package className="mx-auto text-gray-400 dark:text-gray-600 mb-4" size={40} />
            <p className="text-sm md:text-base text-gray-500 dark:text-gray-400 mb-2 font-medium">
              Nenhuma venda registrada
            </p>
            <p className="text-xs md:text-sm text-gray-400 dark:text-gray-500">
              Comece registrando movimentações de saída
            </p>
          </div>
        )}
      </div>

      {/* Produtos por Margem */}
      <div className="bg-white dark:bg-gray-800 rounded-lg md:rounded-xl shadow-sm p-4 md:p-6 border border-gray-100 dark:border-gray-700">
        <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3 mb-4 md:mb-6">
          <div className="flex items-center gap-2">
            <Percent className="text-blue-600 dark:text-blue-400 flex-shrink-0" size={20} />
            <h2 className="text-lg md:text-xl font-bold text-gray-900 dark:text-white">
              Produtos por Margem de Lucro
            </h2>
          </div>
          <div className="flex gap-2">
            <button
              onClick={() => setFiltroMargem('menor')}
              className={`px-3 md:px-4 py-2 rounded-lg text-xs md:text-sm font-medium transition ${
                filtroMargem === 'menor'
                  ? 'bg-red-600 text-white'
                  : 'bg-gray-100 dark:bg-gray-700 text-gray-700 dark:text-gray-300 hover:bg-gray-200 dark:hover:bg-gray-600'
              }`}
            >
              Menor Margem
            </button>
            <button
              onClick={() => setFiltroMargem('maior')}
              className={`px-3 md:px-4 py-2 rounded-lg text-xs md:text-sm font-medium transition ${
                filtroMargem === 'maior'
                  ? 'bg-green-600 text-white'
                  : 'bg-gray-100 dark:bg-gray-700 text-gray-700 dark:text-gray-300 hover:bg-gray-200 dark:hover:bg-gray-600'
              }`}
            >
              Maior Margem
            </button>
          </div>
        </div>

        {produtosMargem.length > 0 ? (
          <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 gap-3 md:gap-4">
            {produtosMargem.map((produto) => (
              <div
                key={produto.produtoId}
                className={`p-3 md:p-4 rounded-lg border-2 transition-all hover:shadow-md ${
                  produto.margemPercentual < 20
                    ? 'bg-red-50 dark:bg-red-900/20 border-red-200 dark:border-red-800'
                    : produto.margemPercentual < 40
                    ? 'bg-yellow-50 dark:bg-yellow-900/20 border-yellow-200 dark:border-yellow-800'
                    : 'bg-green-50 dark:bg-green-900/20 border-green-200 dark:border-green-800'
                }`}
              >
                <div className="mb-2">
                  <p className="font-semibold text-xs md:text-sm text-gray-900 dark:text-white line-clamp-2 min-h-[2.5rem]">
                    {produto.descricao}
                  </p>
                </div>
                <div className="space-y-1">
                  <div className="flex justify-between text-xs">
                    <span className="text-gray-600 dark:text-gray-400">Custo:</span>
                    <span className="font-medium text-gray-900 dark:text-white">
                      R$ {produto.valor.toFixed(2)}
                    </span>
                  </div>
                  <div className="flex justify-between text-xs">
                    <span className="text-gray-600 dark:text-gray-400">Venda:</span>
                    <span className="font-medium text-gray-900 dark:text-white">
                      R$ {produto.valorVenda.toFixed(2)}
                    </span>
                  </div>
                  <div className="pt-2 mt-2 border-t border-gray-200 dark:border-gray-600">
                    <div className="flex items-center justify-between">
                      <span className="text-xs font-medium text-gray-600 dark:text-gray-400">Margem:</span>
                      <span
                        className={`text-sm md:text-base font-bold ${
                          produto.margemPercentual < 20
                            ? 'text-red-600 dark:text-red-400'
                            : produto.margemPercentual < 40
                            ? 'text-yellow-600 dark:text-yellow-400'
                            : 'text-green-600 dark:text-green-400'
                        }`}
                      >
                        {produto.margemPercentual.toFixed(1)}%
                      </span>
                    </div>
                    <p className="text-xs text-gray-500 dark:text-gray-400 mt-1">
                      +R$ {produto.margemLucro.toFixed(2)}
                    </p>
                  </div>
                </div>
              </div>
            ))}
          </div>
        ) : (
          <div className="text-center py-8 md:py-12">
            <Percent className="mx-auto text-gray-400 dark:text-gray-600 mb-4" size={40} />
            <p className="text-sm md:text-base text-gray-500 dark:text-gray-400 mb-2 font-medium">
              Nenhum produto com margem calculada
            </p>
            <p className="text-xs md:text-sm text-gray-400 dark:text-gray-500">
              Cadastre produtos com valores de custo e venda
            </p>
          </div>
        )}
      </div>

      {/* Low Stock Alert */}
      {dashboard.estoqueBaixo.length > 0 && (
        <div className="bg-red-50 dark:bg-red-900/20 border-2 border-red-200 dark:border-red-800 rounded-lg md:rounded-xl p-4 md:p-6">
          <div className="flex items-center gap-2 md:gap-3 mb-4 md:mb-6">
            <AlertTriangle className="text-red-600 dark:text-red-400 flex-shrink-0" size={20} />
            <h2 className="text-lg md:text-xl font-bold text-red-900 dark:text-red-200">
              Produtos com Estoque Baixo
            </h2>
          </div>
          <div className="space-y-2 md:space-y-3">
            {dashboard.estoqueBaixo.map((item) => (
              <div
                key={item.produtoId}
                className="flex items-center justify-between p-3 md:p-4 bg-white dark:bg-gray-800 rounded-lg border border-red-100 dark:border-red-900/50"
              >
                <div className="flex items-center gap-2 md:gap-3 flex-1 min-w-0">
                  <div className="w-2 h-2 rounded-full bg-red-500 animate-pulse flex-shrink-0"></div>
                  <span className="font-medium text-sm md:text-base text-gray-900 dark:text-white truncate">
                    {item.descricao}
                  </span>
                </div>
                <div className="text-right flex-shrink-0 ml-2">
                  <span className="text-red-600 dark:text-red-400 font-bold text-sm md:text-base">
                    {item.quantidade}
                  </span>
                  <span className="text-gray-600 dark:text-gray-400 text-xs md:text-sm">
                    {' '} / {item.estoqueMinimo}
                  </span>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Sales by User & Expenses by Type - Side by Side on Desktop, Stacked on Mobile */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-4 md:gap-6">
        {/* Sales by User */}
        {dashboard.vendasPorUsuario.length > 0 && (
          <div className="bg-white dark:bg-gray-800 rounded-lg md:rounded-xl shadow-sm p-4 md:p-6 border border-gray-100 dark:border-gray-700">
            <h2 className="text-lg md:text-xl font-bold text-gray-900 dark:text-white mb-4 md:mb-6">
              👥 Vendas por Usuário
            </h2>
            <div className="space-y-2 md:space-y-3">
              {dashboard.vendasPorUsuario.map((venda) => (
                <div
                  key={venda.usuarioId}
                  className="flex items-center justify-between p-3 md:p-4 bg-gray-50 dark:bg-gray-700/50 rounded-lg"
                >
                  <div className="flex-1 min-w-0 mr-3">
                    <p className="font-medium text-sm md:text-base text-gray-900 dark:text-white truncate">
                      {venda.nome}
                    </p>
                    <p className="text-xs md:text-sm text-gray-600 dark:text-gray-400">
                      {venda.quantidade} {venda.quantidade === 1 ? 'venda' : 'vendas'}
                    </p>
                  </div>
                  <p className="font-bold text-sm md:text-base text-green-600 dark:text-green-400 flex-shrink-0">
                    {formatCurrency(venda.total)}
                  </p>
                </div>
              ))}
            </div>
          </div>
        )}

        {/* Expenses by Type */}
        {dashboard.despesasPorTipo.length > 0 && (
          <div className="bg-white dark:bg-gray-800 rounded-lg md:rounded-xl shadow-sm p-4 md:p-6 border border-gray-100 dark:border-gray-700">
            <h2 className="text-lg md:text-xl font-bold text-gray-900 dark:text-white mb-4 md:mb-6">
              💰 Despesas por Tipo
            </h2>
            <div className="space-y-2 md:space-y-3">
              {dashboard.despesasPorTipo.map((despesa) => (
                <div
                  key={despesa.tipo}
                  className="flex items-center justify-between p-3 md:p-4 bg-gray-50 dark:bg-gray-700/50 rounded-lg"
                >
                  <div className="flex-1 min-w-0 mr-3">
                    <p className="font-medium text-sm md:text-base text-gray-900 dark:text-white truncate">
                      {despesa.tipo}
                    </p>
                    <p className="text-xs md:text-sm text-gray-600 dark:text-gray-400">
                      {despesa.quantidade} {despesa.quantidade === 1 ? 'despesa' : 'despesas'}
                    </p>
                  </div>
                  <p className="font-bold text-sm md:text-base text-red-600 dark:text-red-400 flex-shrink-0">
                    {formatCurrency(despesa.total)}
                  </p>
                </div>
              ))}
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
