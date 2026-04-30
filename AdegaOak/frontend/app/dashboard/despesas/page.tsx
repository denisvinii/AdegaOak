'use client';

import { useEffect, useState } from 'react';
import api from '@/lib/api';
import { Plus, DollarSign } from 'lucide-react';
import Modal from '@/components/Modal';

export default function DespesasPage() {
  const [despesas, setDespesas] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [modalOpen, setModalOpen] = useState(false);
  const [saving, setSaving] = useState(false);
  const [formData, setFormData] = useState({
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
      const { data } = await api.get('/despesas');
      setDespesas(Array.isArray(data) ? data : []);
    } catch (error: any) {
      console.error('Erro ao carregar despesas:', error);
      setDespesas([]);
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSaving(true);

    try {
      await api.post('/despesas', {
        descricao: formData.descricao,
        valor: parseFloat(formData.valor),
        tipo: formData.tipo,
        pago: formData.pago,
      });

      setFormData({
        descricao: '',
        valor: '',
        tipo: 'Fixa',
        pago: false,
      });

      setModalOpen(false);
      loadDespesas();
      alert('Despesa cadastrada com sucesso!');
    } catch (error: any) {
      console.error('Erro ao cadastrar despesa:', error);
      alert(error.response?.data?.message || 'Erro ao cadastrar despesa');
    } finally {
      setSaving(false);
    }
  };

  const togglePago = async (id: number, pago: boolean) => {
    try {
      await api.patch(`/despesas/${id}/pagar`, pago);
      loadDespesas();
    } catch (error: any) {
      console.error('Erro ao atualizar despesa:', error);
      alert(error.response?.data?.message || 'Erro ao atualizar despesa');
    }
  };

  const totalDespesas = (despesas || []).reduce((sum, d) => sum + (d?.valor || 0), 0);
  const totalPagas = (despesas || []).filter((d) => d?.pago).reduce((sum, d) => sum + (d?.valor || 0), 0);
  const totalPendentes = totalDespesas - totalPagas;

  if (loading) {
    return (
      <div className="flex items-center justify-center py-12">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-amber-600"></div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-gray-900 dark:text-white">Despesas</h1>
          <p className="text-gray-600 dark:text-gray-400 mt-2">
            Controle de despesas e contas a pagar
          </p>
        </div>
        <button
          onClick={() => setModalOpen(true)}
          className="bg-amber-600 hover:bg-amber-700 text-white px-6 py-3 rounded-lg flex items-center gap-2 transition"
        >
          <Plus size={20} />
          Nova Despesa
        </button>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <div className="bg-white dark:bg-gray-800 rounded-xl shadow-sm p-6 border border-gray-100 dark:border-gray-700">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-gray-600 dark:text-gray-400">Total Despesas</p>
              <p className="text-2xl font-bold text-gray-900 dark:text-white mt-2">
                R$ {totalDespesas.toFixed(2)}
              </p>
            </div>
            <div className="bg-blue-500 p-3 rounded-lg">
              <DollarSign className="text-white" size={24} />
            </div>
          </div>
        </div>

        <div className="bg-white dark:bg-gray-800 rounded-xl shadow-sm p-6 border border-gray-100 dark:border-gray-700">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-gray-600 dark:text-gray-400">Pagas</p>
              <p className="text-2xl font-bold text-green-600 dark:text-green-400 mt-2">
                R$ {totalPagas.toFixed(2)}
              </p>
            </div>
            <div className="bg-green-500 p-3 rounded-lg">
              <DollarSign className="text-white" size={24} />
            </div>
          </div>
        </div>

        <div className="bg-white dark:bg-gray-800 rounded-xl shadow-sm p-6 border border-gray-100 dark:border-gray-700">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-gray-600 dark:text-gray-400">Pendentes</p>
              <p className="text-2xl font-bold text-red-600 dark:text-red-400 mt-2">
                R$ {totalPendentes.toFixed(2)}
              </p>
            </div>
            <div className="bg-red-500 p-3 rounded-lg">
              <DollarSign className="text-white" size={24} />
            </div>
          </div>
        </div>
      </div>

      {/* Table */}
      <div className="bg-white dark:bg-gray-800 rounded-xl shadow-sm border border-gray-100 dark:border-gray-700 overflow-hidden">
        <table className="w-full">
          <thead className="bg-gray-50 dark:bg-gray-900">
            <tr>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                Data
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                Descrição
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                Tipo
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                Valor
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                Status
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                Ações
              </th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-200 dark:divide-gray-700">
            {(despesas || []).map((despesa) => (
              <tr key={despesa.id} className="hover:bg-gray-50 dark:hover:bg-gray-700">
                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900 dark:text-gray-100">
                  {despesa?.data ? new Date(despesa.data).toLocaleDateString('pt-BR') : '-'}
                </td>
                <td className="px-6 py-4 text-sm text-gray-900 dark:text-gray-100">
                  {despesa?.descricao || '-'}
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600 dark:text-gray-400">
                  {despesa?.tipo || '-'}
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-sm font-semibold text-gray-900 dark:text-gray-100">
                  R$ {(despesa?.valor || 0).toFixed(2)}
                </td>
                <td className="px-6 py-4 whitespace-nowrap">
                  <span
                    className={`px-2 py-1 text-xs font-semibold rounded-full ${
                      despesa?.pago
                        ? 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200'
                        : 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-200'
                    }`}
                  >
                    {despesa?.pago ? 'Paga' : 'Pendente'}
                  </span>
                </td>
                <td className="px-6 py-4 whitespace-nowrap">
                  <button
                    onClick={() => togglePago(despesa?.id, !despesa?.pago)}
                    className={`px-3 py-1 text-xs font-semibold rounded-lg transition ${
                      despesa?.pago
                        ? 'bg-yellow-100 text-yellow-800 hover:bg-yellow-200 dark:bg-yellow-900 dark:text-yellow-200'
                        : 'bg-green-100 text-green-800 hover:bg-green-200 dark:bg-green-900 dark:text-green-200'
                    }`}
                  >
                    {despesa?.pago ? 'Marcar Pendente' : 'Marcar Paga'}
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
        {(despesas || []).length === 0 && (
          <div className="text-center py-12 text-gray-500 dark:text-gray-400">
            Nenhuma despesa cadastrada
          </div>
        )}
      </div>

      {/* Modal Nova Despesa */}
      <Modal
        isOpen={modalOpen}
        onClose={() => setModalOpen(false)}
        title="Nova Despesa"
      >
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
              onChange={(e) => setFormData({ ...formData, tipo: e.target.value })}
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
            <button
              type="submit"
              disabled={saving}
              className="flex-1 px-4 py-2 bg-amber-600 hover:bg-amber-700 disabled:opacity-50 text-white rounded-lg transition"
            >
              {saving ? 'Salvando...' : 'Salvar'}
            </button>
          </div>
        </form>
      </Modal>
    </div>
  );
}
