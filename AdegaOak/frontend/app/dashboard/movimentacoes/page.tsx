'use client';

import { useEffect, useState, useCallback } from 'react';
import api from '@/lib/api';
import { Filter, Plus } from 'lucide-react';
import Modal from '@/components/Modal';
import LoadingButton from '@/components/LoadingButton';
import Pagination from '@/components/Pagination';
import { useToast } from '@/components/Toast';
import { useMovimentacoes } from '@/hooks/useMovimentacoes';
import type { ProdutoDto } from '@/types/produto';
import type { MovimentacaoFiltroRequest } from '@/types/movimentacao';

type FiltroTipo = 'Todos' | 'Entrada' | 'Saída';
type FiltroPeriodo = 'hoje' | '7dias' | '30dias' | 'todos';

function buildFiltro(tipo: FiltroTipo, periodo: FiltroPeriodo): MovimentacaoFiltroRequest {
  const filtro: MovimentacaoFiltroRequest = {};

  if (tipo !== 'Todos') filtro.tipo = tipo;

  const now = new Date();
  const startOfToday = new Date(now.getFullYear(), now.getMonth(), now.getDate());

  if (periodo === 'hoje') {
    filtro.dataInicio = startOfToday.toISOString();
    filtro.dataFim = new Date(startOfToday.getTime() + 86400000).toISOString();
  } else if (periodo === '7dias') {
    filtro.dataInicio = new Date(startOfToday.getTime() - 7 * 86400000).toISOString();
  } else if (periodo === '30dias') {
    filtro.dataInicio = new Date(startOfToday.getTime() - 30 * 86400000).toISOString();
  }

  return filtro;
}

export default function MovimentacoesPage() {
  const toast = useToast();
  const { movimentacoes, page, totalPages, totalCount, pageSize, loading, load } = useMovimentacoes();

  const [produtos, setProdutos] = useState<ProdutoDto[]>([]);
  const [filtroTipo, setFiltroTipo] = useState<FiltroTipo>('Todos');
  const [filtroData, setFiltroData] = useState<FiltroPeriodo>('hoje');
  const [modalOpen, setModalOpen] = useState(false);
  const [saving, setSaving] = useState(false);
  const [formData, setFormData] = useState({
    produtoId: '',
    quantidade: '',
    valorUnitario: '',
  });

  const reloadWithFilters = useCallback(
    (tipo: FiltroTipo, periodo: FiltroPeriodo, targetPage = 1) => {
      load(buildFiltro(tipo, periodo), targetPage);
    },
    [load]
  );

  useEffect(() => {
    reloadWithFilters(filtroTipo, filtroData);
    loadProdutos();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const loadProdutos = async () => {
    try {
      const { data } = await api.get<ProdutoDto[]>('/produtos');
      setProdutos(Array.isArray(data) ? data : []);
    } catch {
      console.error('Erro ao carregar produtos');
    }
  };

  const handleFiltroTipo = (tipo: FiltroTipo) => {
    setFiltroTipo(tipo);
    reloadWithFilters(tipo, filtroData);
  };

  const handleFiltroPeriodo = (periodo: FiltroPeriodo) => {
    setFiltroData(periodo);
    reloadWithFilters(filtroTipo, periodo);
  };

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
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

      setFormData({ produtoId: '', quantidade: '', valorUnitario: '' });
      setModalOpen(false);
      reloadWithFilters(filtroTipo, filtroData);
      toast.success('Entrada registrada com sucesso!');
    } catch (error: unknown) {
      const msg =
        error instanceof Error
          ? (error as { response?: { data?: { message?: string } } }).response?.data?.message ?? error.message
          : 'Erro ao registrar entrada';
      toast.error(msg);
    } finally {
      setSaving(false);
    }
  };

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
            {(['Todos', 'Entrada', 'Saída'] as FiltroTipo[]).map((tipo) => (
              <button
                key={tipo}
                onClick={() => handleFiltroTipo(tipo)}
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
            {(
              [
                { value: 'hoje', label: 'Hoje' },
                { value: '7dias', label: 'Últimos 7 dias' },
                { value: '30dias', label: 'Últimos 30 dias' },
                { value: 'todos', label: 'Todos' },
              ] as { value: FiltroPeriodo; label: string }[]
            ).map((opcao) => (
              <button
                key={opcao.value}
                onClick={() => handleFiltroPeriodo(opcao.value)}
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
        {loading ? (
          <div className="flex items-center justify-center py-16">
            <div className="animate-spin rounded-full h-10 w-10 border-b-2 border-amber-600" />
          </div>
        ) : (
          <>
            <div className="overflow-x-auto">
              <table className="w-full min-w-[700px]">
                <thead className="bg-gray-50 dark:bg-gray-900">
                  <tr>
                    {['Data', 'Tipo', 'Produto', 'Quantidade', 'Valor Total', 'Responsável'].map((h) => (
                      <th
                        key={h}
                        className="px-4 md:px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider"
                      >
                        {h}
                      </th>
                    ))}
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-200 dark:divide-gray-700">
                  {movimentacoes.map((mov) => (
                    <tr key={mov.id} className="hover:bg-gray-50 dark:hover:bg-gray-700">
                      <td className="px-4 md:px-6 py-4 whitespace-nowrap text-xs md:text-sm text-gray-900 dark:text-gray-100">
                        {new Date(mov.data).toLocaleDateString('pt-BR')}
                      </td>
                      <td className="px-4 md:px-6 py-4 whitespace-nowrap">
                        <span
                          className={`px-2 py-1 text-xs font-semibold rounded-full ${
                            mov.tipo === 'Entrada'
                              ? 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200'
                              : 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200'
                          }`}
                        >
                          {mov.tipo}
                        </span>
                      </td>
                      <td className="px-4 md:px-6 py-4 text-xs md:text-sm text-gray-900 dark:text-gray-100">
                        {mov.produtoDescricao}
                      </td>
                      <td className="px-4 md:px-6 py-4 whitespace-nowrap text-xs md:text-sm text-gray-900 dark:text-gray-100">
                        {mov.quantidade}
                      </td>
                      <td className="px-4 md:px-6 py-4 whitespace-nowrap text-xs md:text-sm font-semibold text-gray-900 dark:text-gray-100">
                        R$ {mov.valorTotal.toFixed(2)}
                      </td>
                      <td className="px-4 md:px-6 py-4 text-xs md:text-sm text-gray-600 dark:text-gray-400">
                        {mov.responsavel}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            {movimentacoes.length === 0 && (
              <div className="text-center py-12 text-sm md:text-base text-gray-500 dark:text-gray-400">
                Nenhuma movimentação encontrada
              </div>
            )}

            {/* Pagination */}
            <div className="px-4 md:px-6 py-3 border-t border-gray-200 dark:border-gray-700">
              <Pagination
                page={page}
                totalPages={totalPages}
                totalCount={totalCount}
                pageSize={pageSize}
                onPageChange={(p) => reloadWithFilters(filtroTipo, filtroData, p)}
              />
            </div>
          </>
        )}
      </div>

      {/* Modal Cadastrar Entrada */}
      <Modal isOpen={modalOpen} onClose={() => setModalOpen(false)} title="Cadastrar Entrada de Estoque">
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
              {produtos.map((produto) => (
                <option key={produto.id} value={produto.id}>
                  {produto.descricao}
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
              Valor Total:{' '}
              <span className="font-bold text-gray-900 dark:text-white">
                R${' '}
                {(
                  parseFloat(formData.quantidade || '0') * parseFloat(formData.valorUnitario || '0')
                ).toFixed(2)}
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
            <LoadingButton
              type="submit"
              loading={saving}
              loadingText="Salvando..."
              className="flex-1 px-4 py-2 bg-green-600 hover:bg-green-700 text-white rounded-lg"
            >
              Salvar Entrada
            </LoadingButton>
          </div>
        </form>
      </Modal>
    </div>
  );
}
