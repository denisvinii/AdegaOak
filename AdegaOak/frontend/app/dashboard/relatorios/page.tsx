'use client';

import { useState, useEffect } from 'react';
import api from '@/lib/api';

interface VendaMensal {
  mes: number;
  ano: number;
  mesNome: string;
  totalVendas: number;
  quantidadeVendas: number;
  ticketMedio: number;
}

interface VendaPorUsuario {
  usuarioId: number;
  nome: string;
  total: number;
  quantidade: number;
}

interface RelatorioVendas {
  vendasMensais: VendaMensal[];
  vendasPorUsuario: VendaPorUsuario[];
  totalGeral: number;
  quantidadeGeral: number;
  ticketMedioGeral: number;
}

interface Usuario {
  id: number;
  nome: string;
  username: string;
}

export default function RelatoriosPage() {
  const [relatorio, setRelatorio] = useState<RelatorioVendas | null>(null);
  const [usuarios, setUsuarios] = useState<Usuario[]>([]);
  const [loading, setLoading] = useState(true);
  
  // Filtros
  const [anoSelecionado, setAnoSelecionado] = useState(new Date().getFullYear());
  const [mesSelecionado, setMesSelecionado] = useState<number | null>(null);
  const [usuarioSelecionado, setUsuarioSelecionado] = useState<number | null>(null);
  const [visualizacao, setVisualizacao] = useState<'mensal' | 'usuario'>('mensal');

  const mesesNomes = [
    'Janeiro', 'Fevereiro', 'Março', 'Abril', 'Maio', 'Junho',
    'Julho', 'Agosto', 'Setembro', 'Outubro', 'Novembro', 'Dezembro'
  ];

  useEffect(() => {
    carregarUsuarios();
  }, []);

  useEffect(() => {
    carregarRelatorio();
  }, [anoSelecionado, mesSelecionado, usuarioSelecionado]);

  const carregarUsuarios = async () => {
    try {
      const { data } = await api.get('/usuarios');
      setUsuarios(data);
    } catch (error: any) {
      console.error('Erro ao carregar usuários:', error);
      // Se o endpoint não existir (404), apenas não mostra o filtro de usuários
      if (error.response?.status === 404) {
        console.warn('Endpoint /api/usuarios não disponível. Filtro por usuário desabilitado.');
      }
      setUsuarios([]);
    }
  };

  const carregarRelatorio = async () => {
    setLoading(true);
    try {
      const payload: any = {
        ano: anoSelecionado,
      };

      if (mesSelecionado) {
        payload.mes = mesSelecionado;
      }

      if (usuarioSelecionado) {
        payload.usuarioId = usuarioSelecionado;
      }

      console.log('Carregando relatório com payload:', payload);
      const { data } = await api.post('/dashboard/relatorio-vendas', payload);
      console.log('Relatório recebido:', data);
      setRelatorio(data);
    } catch (error: any) {
      console.error('Erro ao carregar relatório:', error);
      console.error('Detalhes do erro:', error.response?.data);
      // Definir relatório vazio em caso de erro
      setRelatorio({
        vendasMensais: [],
        vendasPorUsuario: [],
        totalGeral: 0,
        quantidadeGeral: 0,
        ticketMedioGeral: 0,
      });
    } finally {
      setLoading(false);
    }
  };

  const formatarMoeda = (valor: number) => {
    return new Intl.NumberFormat('pt-BR', {
      style: 'currency',
      currency: 'BRL',
    }).format(valor);
  };

  const limparFiltros = () => {
    setAnoSelecionado(new Date().getFullYear());
    setMesSelecionado(null);
    setUsuarioSelecionado(null);
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-amber-600" />
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-3xl font-bold text-gray-900 dark:text-white">
          Relatório de Vendas
        </h1>
        <button
          onClick={limparFiltros}
          className="px-4 py-2 text-sm font-medium text-gray-700 dark:text-gray-300 bg-white dark:bg-gray-700 border border-gray-300 dark:border-gray-600 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-600 transition-colors"
        >
          Limpar Filtros
        </button>
      </div>

      {/* Filtros */}
      <div className="bg-white dark:bg-gray-800 rounded-lg shadow-sm p-6 space-y-4">
        <h2 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">
          Filtros
        </h2>
        
        <div className={`grid grid-cols-1 gap-4 ${usuarios.length > 0 ? 'md:grid-cols-4' : 'md:grid-cols-3'}`}>
          {/* Ano */}
          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              Ano
            </label>
            <select
              value={anoSelecionado}
              onChange={(e) => setAnoSelecionado(Number(e.target.value))}
              className="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-amber-500 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
            >
              {[2023, 2024, 2025, 2026].map(ano => (
                <option key={ano} value={ano}>{ano}</option>
              ))}
            </select>
          </div>

          {/* Mês */}
          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              Mês
            </label>
            <select
              value={mesSelecionado || ''}
              onChange={(e) => setMesSelecionado(e.target.value ? Number(e.target.value) : null)}
              className="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-amber-500 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
            >
              <option value="">Todos os meses</option>
              {mesesNomes.map((mes, index) => (
                <option key={index + 1} value={index + 1}>
                  {mes}
                </option>
              ))}
            </select>
          </div>

          {/* Usuário */}
          {usuarios.length > 0 && (
            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                Vendedor
              </label>
              <select
                value={usuarioSelecionado || ''}
                onChange={(e) => setUsuarioSelecionado(e.target.value ? Number(e.target.value) : null)}
                className="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-amber-500 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
              >
                <option value="">Todos os vendedores</option>
                {usuarios.map(usuario => (
                  <option key={usuario.id} value={usuario.id}>
                    {usuario.nome}
                  </option>
                ))}
              </select>
            </div>
          )}

          {/* Visualização */}
          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              Visualização
            </label>
            <select
              value={visualizacao}
              onChange={(e) => setVisualizacao(e.target.value as 'mensal' | 'usuario')}
              className="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-amber-500 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
            >
              <option value="mensal">Por Mês</option>
              <option value="usuario">Por Vendedor</option>
            </select>
          </div>
        </div>
      </div>

      {/* Cards de Resumo */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <div className="bg-gradient-to-br from-green-500 to-green-600 rounded-lg shadow-lg p-6 text-white">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-green-100 text-sm font-medium">Total de Vendas</p>
              <p className="text-3xl font-bold mt-2">{formatarMoeda(relatorio?.totalGeral || 0)}</p>
            </div>
            <div className="bg-white/20 p-3 rounded-lg">
              <svg className="w-8 h-8" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
            </div>
          </div>
        </div>

        <div className="bg-gradient-to-br from-blue-500 to-blue-600 rounded-lg shadow-lg p-6 text-white">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-blue-100 text-sm font-medium">Quantidade</p>
              <p className="text-3xl font-bold mt-2">{relatorio?.quantidadeGeral || 0}</p>
            </div>
            <div className="bg-white/20 p-3 rounded-lg">
              <svg className="w-8 h-8" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M7 12l3-3 3 3 4-4M8 21l4-4 4 4M3 4h18M4 4h16v12a1 1 0 01-1 1H5a1 1 0 01-1-1V4z" />
              </svg>
            </div>
          </div>
        </div>

        <div className="bg-gradient-to-br from-purple-500 to-purple-600 rounded-lg shadow-lg p-6 text-white">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-purple-100 text-sm font-medium">Ticket Médio</p>
              <p className="text-3xl font-bold mt-2">{formatarMoeda(relatorio?.ticketMedioGeral || 0)}</p>
            </div>
            <div className="bg-white/20 p-3 rounded-lg">
              <svg className="w-8 h-8" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" />
              </svg>
            </div>
          </div>
        </div>
      </div>

      {/* Tabela de Dados */}
      <div className="bg-white dark:bg-gray-800 rounded-lg shadow-sm overflow-hidden">
        <div className="px-6 py-4 border-b border-gray-200 dark:border-gray-700">
          <h2 className="text-lg font-semibold text-gray-900 dark:text-white">
            {visualizacao === 'mensal' ? 'Vendas por Mês' : 'Vendas por Vendedor'}
          </h2>
        </div>

        <div className="overflow-x-auto">
          {visualizacao === 'mensal' ? (
            <table className="w-full">
              <thead className="bg-gray-50 dark:bg-gray-700">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                    Mês
                  </th>
                  <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                    Total Vendas
                  </th>
                  <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                    Quantidade
                  </th>
                  <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                    Ticket Médio
                  </th>
                </tr>
              </thead>
              <tbody className="bg-white dark:bg-gray-800 divide-y divide-gray-200 dark:divide-gray-700">
                {!relatorio || relatorio.vendasMensais.length === 0 ? (
                  <tr>
                    <td colSpan={4} className="px-6 py-8 text-center text-gray-500 dark:text-gray-400">
                      Nenhuma venda encontrada para o período selecionado
                    </td>
                  </tr>
                ) : (
                  relatorio.vendasMensais.map((venda) => (
                    <tr key={`${venda.ano}-${venda.mes}`} className="hover:bg-gray-50 dark:hover:bg-gray-700">
                      <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900 dark:text-white">
                        {venda.mesNome} {venda.ano}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-right text-gray-900 dark:text-white font-semibold">
                        {formatarMoeda(venda.totalVendas)}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-right text-gray-500 dark:text-gray-400">
                        {venda.quantidadeVendas}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-right text-gray-500 dark:text-gray-400">
                        {formatarMoeda(venda.ticketMedio)}
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          ) : (
            <table className="w-full">
              <thead className="bg-gray-50 dark:bg-gray-700">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                    Vendedor
                  </th>
                  <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                    Total Vendas
                  </th>
                  <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                    Quantidade
                  </th>
                  <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                    Ticket Médio
                  </th>
                </tr>
              </thead>
              <tbody className="bg-white dark:bg-gray-800 divide-y divide-gray-200 dark:divide-gray-700">
                {!relatorio || relatorio.vendasPorUsuario.length === 0 ? (
                  <tr>
                    <td colSpan={4} className="px-6 py-8 text-center text-gray-500 dark:text-gray-400">
                      Nenhuma venda encontrada para o período selecionado
                    </td>
                  </tr>
                ) : (
                  relatorio.vendasPorUsuario.map((venda) => (
                    <tr key={venda.usuarioId} className="hover:bg-gray-50 dark:hover:bg-gray-700">
                      <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900 dark:text-white">
                        {venda.nome}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-right text-gray-900 dark:text-white font-semibold">
                        {formatarMoeda(venda.total)}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-right text-gray-500 dark:text-gray-400">
                        {venda.quantidade}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-right text-gray-500 dark:text-gray-400">
                        {venda.quantidade > 0 ? formatarMoeda(venda.total / venda.quantidade) : formatarMoeda(0)}
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          )}
        </div>
      </div>
    </div>
  );
}
