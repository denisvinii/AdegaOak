'use client';

import { useEffect, useState } from 'react';
import api from '@/lib/api';
import { Filter, Plus } from 'lucide-react';
import Modal from '@/components/Modal';

export default function MovimentacoesPage() {
  const [movimentacoes, setMovimentacoes] = useState<any[]>([]);
  const [produtos, setProdutos] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [filtroTipo, setFiltroTipo] = useState('Todos');
  const [filtroData, setFiltroData] = useState<'hoje' | '7dias' | '30dias' | 'todos'>('hoje');
  const [modalOpen, setModalOpen] = useState(false);
  const [saving, setSaving] = useState(false);
  const [formData, setFormData] = useState({
    produtoId: '',
    quantidade: '',
    valorUnitario: '',
  });

  useEffect(() => {
    loadMovimentacoes();
    loadProdutos();
  }, []);

  const loadMovimentacoes = async () => {
    try {
      const { data } = await api.get('/movimentacoes');
      setMovimentacoes(Array.isArray(data) ? data : []);
    } catch (error: any) {
      console.error('Erro ao carregar movimentações:', error);
      setMovimentacoes([]);
    } finally {
      setLoading(false);
    }
  };

  const loadProdutos = async () => {
    try {
      const { data } = await api.get('/produtos');
      setProdutos(Array.isArray(data) ? data : []);
    } catch (error) {
      console.error('Erro ao carregar produtos:', error);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSaving(true);

    try {
      await api.post('/movimentacoes', {
        tipo: 'Entrada',
        tipoVenda: 'Varejo',
        produtoId: parseInt(formData.produtoId),
        quantidade: parseInt(formData.quantidade),
        valorUnitario: parseFloat(formData.valorUnitario),
      });

      setFormData({
        produtoId: '',
        quantidade: '',
        valorUnitario: '',
      });

      setModalOpen(false);
      loadMovimentacoes();
      alert('Entrada registrada com sucesso!');
    } catch (error: any) {
      console.error('Erro ao registrar entrada:', error);
      alert(error.response?.data?.message || 'Erro ao registrar entrada');
    } finally {
      setSaving(false);
    }
  };

  const movimentacoesFiltradas = (movimentacoes || []).filter((m) => {
    // Filtro por tipo
    if (filtroTipo !== 'Todos' && m?.tipo !== filtroTipo) {
      return false;
    }

    // Filtro por data
    if (filtroData !== 'todos' && m?.data) {
      const dataMovimentacao = new Date(m.data);
      const hoje = new Date();
      hoje.setHours(0, 0, 0, 0);

      if (filtroData === 'hoje') {
        const amanha = new Date(hoje);
        amanha.setDate(amanha.getDate() + 1);
        return dataMovimentacao >= hoje && dataMovimentacao < amanha;
      } else if (filtroData === '7dias') {
        const seteDiasAtras = new Date(hoje);
        seteDiasAtras.setDate(seteDiasAtras.getDate() - 7);
        return dataMovimentacao >= seteDiasAtras;
      } else if (filtroData === '30dias') {
        const trintaDiasAtras = new Date(hoje);
        trintaDiasAtras.setDate(trintaDiasAtras.getDate() - 30);
        return dataMovimentacao >= trintaDiasAtras;
      }
    }

    return true;
  });

  if (loading) {
    return (
      <div className="flex items-center justify-center py-12">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-amber-600"></div>
      </div>
    );
  }

  return (
    <div className="space-y-4 md:space-y-6">
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 className="text-2xl md:text-3xl font-bold text-gray-900 dark:text-white">Movimentações</h1>
          <p className="text-sm md:text-base text-gray-600 dark:text-gray-400 mt-1 md:mt-2">
            Histórico de entradas e saídas de estoque
          </p>
        </div>
        <button
          onClick={() => setModalOpen(true)}
          className="bg-green-600 hover:bg-green-700 text-white px-4 md:px-6 py-2 md:py-3 rounded-lg flex items-center gap-2 transition text-sm md:text-base"
        >
          <Plus size={20} />
          Cadastrar Entrada
        </button>
      </div>

      {/* Filters */}
      <div className="bg-white dark:bg-gray-800 rounded-xl shadow-sm p-4 md:p-6 border border-gray-100 dark:border-gray-700 space-y-4">
        <div className="flex items-center gap-4">
          <Filter size={20} className="text-gray-500 dark:text-gray-400 flex-shrink-0" />
          <div className="flex flex-wrap gap-2">
            {['Todos', 'Entrada', 'Saída'].map((tipo) => (
              <button
                key={tipo}
                onClick={() => setFiltroTipo(tipo)}
                className={`px-3 md:px-4 py-2 rounded-lg transition text-sm md:text-base ${
                  filtroTipo === tipo
                    ? 'bg-amber-600 text-white'
                    : 'bg-gray-100 dark:bg-gray-700 text-gray-700 dark:text-gray-300 hover:bg-gray-200 dark:hover:bg-gray-600'
                }`}
              >
                {tipo}
              </button>
            ))}
          </div>
        </div>

        <div className="flex items-center gap-4">
          <span className="text-sm font-medium text-gray-700 dark:text-gray-300 flex-shrink-0">
            Período:
          </span>
          <div className="flex flex-wrap gap-2">
            {[
              { value: 'hoje', label: 'Hoje' },
              { value: '7dias', label: 'Últimos 7 dias' },
              { value: '30dias', label: 'Últimos 30 dias' },
              { value: 'todos', label: 'Todos' }
            ].map((opcao) => (
              <button
                key={opcao.value}
                onClick={() => setFiltroData(opcao.value as any)}
                className={`px-3 md:px-4 py-2 rounded-lg transition text-sm md:text-base ${
                  filtroData === opcao.value
                    ? 'bg-blue-600 text-white'
                    : 'bg-gray-100 dark:bg-gray-700 text-gray-700 dark:text-gray-300 hover:bg-gray-200 dark:hover:bg-gray-600'
                }`}
              >
                {opcao.label}
              </button>
            ))}
          </div>
        </div>
      </div>

      {/* Table */}
      <div className="bg-white dark:bg-gray-800 rounded-xl shadow-sm border border-gray-100 dark:border-gray-700 overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full min-w-[700px]">
            <thead className="bg-gray-50 dark:bg-gray-900">
              <tr>
                <th className="px-4 md:px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                  Data
                </th>
                <th className="px-4 md:px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                  Tipo
                </th>
                <th className="px-4 md:px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                  Produto
                </th>
                <th className="px-4 md:px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                  Quantidade
                </th>
                <th className="px-4 md:px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                  Valor Total
                </th>
                <th className="px-4 md:px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                  Responsável
                </th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200 dark:divide-gray-700">
              {movimentacoesFiltradas.map((mov) => (
                <tr key={mov.id} className="hover:bg-gray-50 dark:hover:bg-gray-700">
                  <td className="px-4 md:px-6 py-4 whitespace-nowrap text-xs md:text-sm text-gray-900 dark:text-gray-100">
                    {mov?.data ? new Date(mov.data).toLocaleDateString('pt-BR') : '-'}
                  </td>
                  <td className="px-4 md:px-6 py-4 whitespace-nowrap">
                    <span
                      className={`px-2 py-1 text-xs font-semibold rounded-full ${
                        mov?.tipo === 'Entrada'
                          ? 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200'
                          : 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200'
                      }`}
                    >
                      {mov?.tipo || '-'}
                    </span>
                  </td>
                  <td className="px-4 md:px-6 py-4 text-xs md:text-sm text-gray-900 dark:text-gray-100">
                    {mov?.produtoDescricao || '-'}
                  </td>
                  <td className="px-4 md:px-6 py-4 whitespace-nowrap text-xs md:text-sm text-gray-900 dark:text-gray-100">
                    {mov?.quantidade || 0}
                  </td>
                  <td className="px-4 md:px-6 py-4 whitespace-nowrap text-xs md:text-sm font-semibold text-gray-900 dark:text-gray-100">
                    R$ {(mov?.valorTotal || 0).toFixed(2)}
                  </td>
                  <td className="px-4 md:px-6 py-4 text-xs md:text-sm text-gray-600 dark:text-gray-400">
                    {mov?.responsavel || '-'}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
        {movimentacoesFiltradas.length === 0 && (
          <div className="text-center py-12 text-sm md:text-base text-gray-500 dark:text-gray-400">
            Nenhuma movimentação encontrada
          </div>
        )}
      </div>

      {/* Modal Cadastrar Entrada */}
      <Modal
        isOpen={modalOpen}
        onClose={() => setModalOpen(false)}
        title="Cadastrar Entrada de Estoque"
      >
        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
              Produto *
            </label>
            <select
              required
              value={formData.produtoId}
              onChange={(e) => setFormData({ ...formData, produtoId: e.target.value })}
              className="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-amber-500 focus:border-transparent outline-none bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
            >
              <option value="">Selecione um produto</option>
              {(produtos || []).map((produto) => (
                <option key={produto.id} value={produto.id}>
                  {produto?.descricao || 'Produto'}
                </option>
              ))}
            </select>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
              Quantidade *
            </label>
            <input
              type="number"
              required
              min="1"
              value={formData.quantidade}
              onChange={(e) => setFormData({ ...formData, quantidade: e.target.value })}
              className="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-amber-500 focus:border-transparent outline-none bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
              placeholder="0"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
              Valor de Custo (Unitário) *
            </label>
            <input
              type="number"
              step="0.01"
              required
              min="0"
              value={formData.valorUnitario}
              onChange={(e) => setFormData({ ...formData, valorUnitario: e.target.value })}
              className="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-amber-500 focus:border-transparent outline-none bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
              placeholder="0.00"
            />
          </div>

          <div className="bg-gray-50 dark:bg-gray-700 p-3 rounded-lg">
            <p className="text-sm text-gray-600 dark:text-gray-400">
              Valor Total: <span className="font-bold text-gray-900 dark:text-white">
                R$ {(parseFloat(formData.quantidade || '0') * parseFloat(formData.valorUnitario || '0')).toFixed(2)}
              </span>
            </p>
          </div>

          <div className="flex gap-3 pt-4">
            <button
              type="button"
              onClick={() => setModalOpen(false)}
              className="flex-1 px-4 py-2 border border-gray-300 dark:border-gray-600 text-gray-700 dark:text-gray-300 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-700 transition"
            >
              Cancelar
            </button>
            <button
              type="submit"
              disabled={saving}
              className="flex-1 px-4 py-2 bg-green-600 hover:bg-green-700 disabled:opacity-50 text-white rounded-lg transition"
            >
              {saving ? 'Salvando...' : 'Salvar Entrada'}
            </button>
          </div>
        </form>
      </Modal>
    </div>
  );
}
