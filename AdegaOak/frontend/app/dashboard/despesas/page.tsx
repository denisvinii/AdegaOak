'use client';

import { useEffect, useState } from 'react';
import api from '@/lib/api';
import { Plus, DollarSign } from 'lucide-react';
import Modal from '@/components/Modal';
import ConfirmModal from '@/components/ConfirmModal';
import LoadingButton from '@/components/LoadingButton';
import { useToast } from '@/components/Toast';
import type { DespesaDto, CreateDespesaRequest, TipoDespesa } from '@/types/despesa';

export default function DespesasPage() {
  const toast = useToast();
  const [despesas, setDespesas] = useState<DespesaDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [modalOpen, setModalOpen] = useState(false);
  const [saving, setSaving] = useState(false);
  const [togglingId, setTogglingId] = useState<number | null>(null);
  const [formData, setFormData] = useState<{
    descricao: string;
    valor: string;
    tipo: TipoDespesa;
    pago: boolean;
  }>({
    descricao: '',
    valor: '',
    tipo: 'Fixa',
    pago: false,
  });

  useEffect(() => {
    loadDespesas();
  }, []);

  const loadDespesas = async () => {
    try {
      const { data } = await api.get<DespesaDto[]>('/despesas');
      setDespesas(Array.isArray(data) ? data : []);
    } catch {
      toast.error('Erro ao carregar despesas');
      setDespesas([]);
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    setSaving(true);
    try {
      const payload: CreateDespesaRequest = {
        descricao: formData.descricao,
        valor: parseFloat(formData.valor),
        tipo: formData.tipo,
        pago: formData.pago,
      };
      await api.post('/despesas', payload);
      setFormData({ descricao: '', valor: '', tipo: 'Fixa', pago: false });
      setModalOpen(false);
      loadDespesas();
      toast.success('Despesa cadastrada com sucesso!');
    } catch (error: unknown) {
      const msg =
        (error as { response?: { data?: { message?: string } } })?.response?.data?.message ??
        'Erro ao cadastrar despesa';
      toast.error(msg);
    } finally {
      setSaving(false);
    }
  };

  const togglePago = async (id: number, pago: boolean) => {
    setTogglingId(id);
    try {
      await api.patch(`/despesas/${id}/pagar`, pago);
      loadDespesas();
      toast.success(pago ? 'Despesa marcada como paga!' : 'Despesa marcada como pendente!');
    } catch (error: unknown) {
      const msg =
        (error as { response?: { data?: { message?: string } } })?.response?.data?.message ??
        'Erro ao atualizar despesa';
      toast.error(msg);
    } finally {
      setTogglingId(null);
    }
  };

  const totalDespesas = despesas.reduce((sum, d) => sum + d.valor, 0);
  const totalPagas = despesas.filter((d) => d.pago).reduce((sum, d) => sum + d.valor, 0);
  const totalPendentes = totalDespesas - totalPagas;

  if (loading) {
    return (
      <div className="flex items-center justify-center py-12">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-amber-600" />
      </div>
    );
  }

  return (
    <div className="space-y-4 md:space-y-6">
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 className="text-2xl md:text-3xl font-bold text-gray-900 dark:text-white">Despesas</h1>
          <p className="text-sm md:text-base text-gray-600 dark:text-gray-400 mt-1 md:mt-2">
            Controle de despesas e contas a pagar
          </p>
        </div>
        <button
          onClick={() => setModalOpen(true)}
          className="bg-amber-600 hover:bg-amber-700 text-white px-4 md:px-6 py-2 md:py-3 rounded-lg flex items-center justify-center gap-2 transition text-sm md:text-base"
        >
          <Plus size={18} />
          <span className="hidden sm:inline">Nova Despesa</span>
          <span className="sm:hidden">Nova</span>
        </button>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-1 sm:grid-cols-3 gap-3 md:gap-6">
        {[
          { label: 'Total Despesas', value: totalDespesas, color: 'bg-blue-500', textColor: 'text-gray-900 dark:text-white' },
          { label: 'Pagas', value: totalPagas, color: 'bg-green-500', textColor: 'text-green-600 dark:text-green-400' },
          { label: 'Pendentes', value: totalPendentes, color: 'bg-red-500', textColor: 'text-red-600 dark:text-red-400' },
        ].map((stat) => (
          <div
            key={stat.label}
            className="bg-white dark:bg-gray-800 rounded-lg md:rounded-xl shadow-sm p-4 md:p-6 border border-gray-100 dark:border-gray-700"
          >
            <div className="flex items-center justify-between">
              <div className="flex-1 min-w-0">
                <p className="text-xs md:text-sm text-gray-600 dark:text-gray-400 truncate">{stat.label}</p>
                <p className={`text-xl md:text-2xl font-bold mt-1 md:mt-2 truncate ${stat.textColor}`}>
                  R$ {stat.value.toFixed(2)}
                </p>
              </div>
              <div className={`${stat.color} p-2 md:p-3 rounded-lg flex-shrink-0`}>
                <DollarSign className="text-white" size={20} />
              </div>
            </div>
          </div>
        ))}
      </div>

      {/* Table */}
      <div className="bg-white dark:bg-gray-800 rounded-lg md:rounded-xl shadow-sm border border-gray-100 dark:border-gray-700 overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full min-w-[700px]">
            <thead className="bg-gray-50 dark:bg-gray-900">
              <tr>
                {['Data', 'Descrição', 'Tipo', 'Valor', 'Status', 'Ações'].map((h) => (
                  <th
                    key={h}
                    className="px-3 md:px-6 py-2 md:py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider"
                  >
                    {h}
                  </th>
                ))}
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200 dark:divide-gray-700">
              {despesas.map((despesa) => (
                <tr key={despesa.id} className="hover:bg-gray-50 dark:hover:bg-gray-700">
                  <td className="px-3 md:px-6 py-3 md:py-4 whitespace-nowrap text-xs md:text-sm text-gray-900 dark:text-gray-100">
                    {new Date(despesa.data).toLocaleDateString('pt-BR')}
                  </td>
                  <td className="px-3 md:px-6 py-3 md:py-4 text-xs md:text-sm text-gray-900 dark:text-gray-100">
                    <span className="line-clamp-2">{despesa.descricao}</span>
                  </td>
                  <td className="px-3 md:px-6 py-3 md:py-4 whitespace-nowrap text-xs md:text-sm text-gray-600 dark:text-gray-400">
                    {despesa.tipoNome ?? despesa.tipo}
                  </td>
                  <td className="px-3 md:px-6 py-3 md:py-4 whitespace-nowrap text-xs md:text-sm font-semibold text-gray-900 dark:text-gray-100">
                    R$ {despesa.valor.toFixed(2)}
                  </td>
                  <td className="px-3 md:px-6 py-3 md:py-4 whitespace-nowrap">
                    <span
                      className={`px-2 py-1 text-xs font-semibold rounded-full ${
                        despesa.pago
                          ? 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200'
                          : 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-200'
                      }`}
                    >
                      {despesa.pago ? 'Paga' : 'Pendente'}
                    </span>
                  </td>
                  <td className="px-3 md:px-6 py-3 md:py-4 whitespace-nowrap">
                    <LoadingButton
                      loading={togglingId === despesa.id}
                      onClick={() => togglePago(despesa.id, !despesa.pago)}
                      className={`px-2 md:px-3 py-1 text-xs font-semibold rounded-lg ${
                        despesa.pago
                          ? 'bg-yellow-100 text-yellow-800 hover:bg-yellow-200 dark:bg-yellow-900 dark:text-yellow-200'
                          : 'bg-green-100 text-green-800 hover:bg-green-200 dark:bg-green-900 dark:text-green-200'
                      }`}
                    >
                      <span className="hidden sm:inline">{despesa.pago ? 'Marcar Pendente' : 'Marcar Paga'}</span>
                      <span className="sm:hidden">{despesa.pago ? 'Pendente' : 'Pagar'}</span>
                    </LoadingButton>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
        {despesas.length === 0 && (
          <div className="text-center py-8 md:py-12 text-sm md:text-base text-gray-500 dark:text-gray-400">
            Nenhuma despesa cadastrada
          </div>
        )}
      </div>

      {/* Modal Nova Despesa */}
      <Modal isOpen={modalOpen} onClose={() => setModalOpen(false)} title="Nova Despesa">
        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
              Descrição *
            </label>
            <input
              type="text"
              required
              value={formData.descricao}
              onChange={(e) => setFormData({ ...formData, descricao: e.target.value })}
              className="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-amber-500 focus:border-transparent outline-none bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
              placeholder="Ex: Aluguel, Energia, Água..."
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
              Valor *
            </label>
            <input
              type="number"
              step="0.01"
              required
              min="0"
              value={formData.valor}
              onChange={(e) => setFormData({ ...formData, valor: e.target.value })}
              className="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-amber-500 focus:border-transparent outline-none bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
              placeholder="0.00"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
              Tipo *
            </label>
            <select
              required
              value={formData.tipo}
              onChange={(e) => setFormData({ ...formData, tipo: e.target.value as TipoDespesa })}
              className="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-amber-500 focus:border-transparent outline-none bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
            >
              <option value="Fixa">Fixa</option>
              <option value="Variavel">Variável</option>
              <option value="Eventual">Eventual</option>
            </select>
          </div>

          <div className="flex items-center gap-2">
            <input
              type="checkbox"
              id="pago"
              checked={formData.pago}
              onChange={(e) => setFormData({ ...formData, pago: e.target.checked })}
              className="w-4 h-4 text-amber-600 border-gray-300 rounded focus:ring-amber-500"
            />
            <label htmlFor="pago" className="text-sm text-gray-700 dark:text-gray-300">
              Marcar como paga
            </label>
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
              className="flex-1 px-4 py-2 bg-amber-600 hover:bg-amber-700 text-white rounded-lg"
            >
              Salvar
            </LoadingButton>
          </div>
        </form>
      </Modal>
    </div>
  );
}
