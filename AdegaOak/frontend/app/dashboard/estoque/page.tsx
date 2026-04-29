'use client';

import { useEffect, useState } from 'react';
import api from '@/lib/api';
import { Package, AlertTriangle, Plus } from 'lucide-react';
import Modal from '@/components/Modal';

export default function EstoquePage() {
  const [produtos, setProdutos] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [modalOpen, setModalOpen] = useState(false);
  const [formData, setFormData] = useState({
    bebida: '',
    tamanho: '',
    material: '',
    valor: '',
    valorVenda: '',
    quantidadeCaixa: '12',
    valorCaixa: '',
    valorAtacadoCaixa: '',
    estoqueMinimo: '10',
  });
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    loadProdutos();
  }, []);

  const loadProdutos = async () => {
    try {
      const { data } = await api.get('/produtos');
      setProdutos(Array.isArray(data) ? data : []);
    } catch (error) {
      console.error('Erro ao carregar produtos:', error);
      setProdutos([]);
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSaving(true);

    try {
      await api.post('/produtos', {
        bebida: formData.bebida,
        tamanho: formData.tamanho,
        material: formData.material,
        valor: parseFloat(formData.valor),
        valorVenda: parseFloat(formData.valorVenda),
        quantidadeCaixa: parseInt(formData.quantidadeCaixa),
        valorCaixa: parseFloat(formData.valorCaixa),
        valorAtacadoCaixa: parseFloat(formData.valorAtacadoCaixa),
        estoqueMinimo: parseInt(formData.estoqueMinimo),
      });

      // Reset form
      setFormData({
        bebida: '',
        tamanho: '',
        material: '',
        valor: '',
        valorVenda: '',
        quantidadeCaixa: '12',
        valorCaixa: '',
        valorAtacadoCaixa: '',
        estoqueMinimo: '10',
      });

      setModalOpen(false);
      loadProdutos(); // Reload list
      alert('Produto cadastrado com sucesso!');
    } catch (error: any) {
      console.error('Erro ao cadastrar produto:', error);
      alert(error.response?.data?.message || 'Erro ao cadastrar produto');
    } finally {
      setSaving(false);
    }
  };

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
          <h1 className="text-3xl font-bold text-gray-900 dark:text-white">Estoque</h1>
          <p className="text-gray-600 dark:text-gray-400 mt-2">Gerenciamento de produtos</p>
        </div>
        <button
          onClick={() => setModalOpen(true)}
          className="bg-amber-600 hover:bg-amber-700 text-white px-6 py-3 rounded-lg flex items-center gap-2 transition"
        >
          <Plus size={20} />
          Novo Produto
        </button>
      </div>

      <div className="bg-white dark:bg-gray-800 rounded-xl shadow-sm border border-gray-100 dark:border-gray-700 overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead className="bg-gray-50 dark:bg-gray-900 border-b border-gray-200 dark:border-gray-700">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase">
                  Produto
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase">
                  Tipo
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase">
                  Tamanho
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase">
                  Qtd/Caixa
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase">
                  Estoque
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase">
                  Custo
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase">
                  Venda
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase">
                  Caixa
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase">
                  Atacado
                </th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200 dark:divide-gray-700">
              {(produtos || []).map((produto) => (
                <tr key={produto?.id || Math.random()} className="hover:bg-gray-50 dark:hover:bg-gray-700">
                  <td className="px-6 py-4">
                    <div className="flex items-center gap-2">
                      <Package size={16} className="text-gray-400" />
                      <span className="font-medium text-gray-900 dark:text-white">
                        {produto?.bebida || 'Produto'}
                      </span>
                    </div>
                  </td>
                  <td className="px-6 py-4 text-sm text-gray-600 dark:text-gray-400">
                    {produto?.material || '-'}
                  </td>
                  <td className="px-6 py-4 text-sm text-gray-600 dark:text-gray-400">
                    {produto?.tamanho || '-'}
                  </td>
                  <td className="px-6 py-4 text-sm text-gray-600 dark:text-gray-400">
                    {produto?.quantidadeCaixa || '-'}
                  </td>
                  <td className="px-6 py-4">
                    <div className="flex items-center gap-2">
                      <span
                        className={`font-semibold ${
                          produto?.estoqueBaixo
                            ? 'text-red-600 dark:text-red-400'
                            : 'text-gray-900 dark:text-white'
                        }`}
                      >
                        {produto?.quantidade || 0}
                      </span>
                      {produto?.estoqueBaixo && (
                        <AlertTriangle size={16} className="text-red-500" />
                      )}
                    </div>
                  </td>
                  <td className="px-6 py-4 text-gray-900 dark:text-white">
                    R$ {(produto?.valor || 0).toFixed(2)}
                  </td>
                  <td className="px-6 py-4 text-gray-900 dark:text-white">
                    R$ {(produto?.valorVenda || 0).toFixed(2)}
                  </td>
                  <td className="px-6 py-4 text-gray-900 dark:text-white">
                    R$ {(produto?.valorCaixa || 0).toFixed(2)}
                  </td>
                  <td className="px-6 py-4 text-gray-900 dark:text-white">
                    R$ {(produto?.valorAtacadoCaixa || 0).toFixed(2)}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
        {(produtos || []).length === 0 && (
          <div className="text-center py-12 text-gray-500 dark:text-gray-400">
            Nenhum produto cadastrado
          </div>
        )}
      </div>

      {/* Modal Novo Produto */}
      <Modal
        isOpen={modalOpen}
        onClose={() => setModalOpen(false)}
        title="Novo Produto"
      >
        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
              Tipo de Produto *
            </label>
            <select
              required
              value={formData.material}
              onChange={(e) => {
                setFormData({ ...formData, material: e.target.value, tamanho: '' });
              }}
              className="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-amber-500 focus:border-transparent outline-none bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
            >
              <option value="">Selecione o tipo</option>
              <option value="Copo">Copo</option>
              <option value="Garrafa">Garrafa</option>
              <option value="Lata">Lata</option>
              <option value="Long Neck">Long Neck</option>
              <option value="Barril">Barril</option>
            </select>
          </div>

          {formData.material && (
            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                Tamanho *
              </label>
              <select
                required
                value={formData.tamanho}
                onChange={(e) => setFormData({ ...formData, tamanho: e.target.value })}
                className="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-amber-500 focus:border-transparent outline-none bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
              >
                <option value="">Selecione o tamanho</option>
                {formData.material === 'Copo' && (
                  <>
                    <option value="200ml">200ml</option>
                    <option value="250ml">250ml</option>
                    <option value="300ml">300ml</option>
                    <option value="350ml">350ml</option>
                    <option value="400ml">400ml</option>
                    <option value="500ml">500ml</option>
                  </>
                )}
                {formData.material === 'Garrafa' && (
                  <>
                    <option value="600ml">600ml</option>
                    <option value="1L">1L</option>
                    <option value="1.5L">1.5L</option>
                    <option value="2L">2L</option>
                  </>
                )}
                {formData.material === 'Lata' && (
                  <>
                    <option value="269ml">269ml</option>
                    <option value="310ml">310ml</option>
                    <option value="350ml">350ml</option>
                    <option value="473ml">473ml</option>
                  </>
                )}
                {formData.material === 'Long Neck' && (
                  <>
                    <option value="275ml">275ml</option>
                    <option value="330ml">330ml</option>
                    <option value="355ml">355ml</option>
                  </>
                )}
                {formData.material === 'Barril' && (
                  <>
                    <option value="5L">5L</option>
                    <option value="10L">10L</option>
                    <option value="20L">20L</option>
                    <option value="30L">30L</option>
                    <option value="50L">50L</option>
                  </>
                )}
              </select>
            </div>
          )}

          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
              Nome da Bebida *
            </label>
            <input
              type="text"
              required
              value={formData.bebida}
              onChange={(e) => setFormData({ ...formData, bebida: e.target.value })}
              className="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-amber-500 focus:border-transparent outline-none bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
              placeholder="Ex: Heineken, Coca-Cola, Skol..."
            />
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                Valor de Custo *
              </label>
              <input
                type="number"
                step="0.01"
                required
                value={formData.valor}
                onChange={(e) => setFormData({ ...formData, valor: e.target.value })}
                className="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-amber-500 focus:border-transparent outline-none bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
                placeholder="0.00"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                Valor de Venda *
              </label>
              <input
                type="number"
                step="0.01"
                required
                value={formData.valorVenda}
                onChange={(e) => setFormData({ ...formData, valorVenda: e.target.value })}
                className="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-amber-500 focus:border-transparent outline-none bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
                placeholder="0.00"
              />
            </div>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
              Quantidade por Caixa *
            </label>
            <input
              type="number"
              required
              min="1"
              value={formData.quantidadeCaixa}
              onChange={(e) => setFormData({ ...formData, quantidadeCaixa: e.target.value })}
              className="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-amber-500 focus:border-transparent outline-none bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
              placeholder="12"
            />
            <p className="text-xs text-gray-500 dark:text-gray-400 mt-1">
              Quantas unidades vêm em uma caixa deste produto
            </p>
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                Valor Caixa *
              </label>
              <input
                type="number"
                step="0.01"
                required
                value={formData.valorCaixa}
                onChange={(e) => setFormData({ ...formData, valorCaixa: e.target.value })}
                className="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-amber-500 focus:border-transparent outline-none bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
                placeholder="0.00"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                Valor Atacado Caixa *
              </label>
              <input
                type="number"
                step="0.01"
                required
                value={formData.valorAtacadoCaixa}
                onChange={(e) => setFormData({ ...formData, valorAtacadoCaixa: e.target.value })}
                className="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-amber-500 focus:border-transparent outline-none bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
                placeholder="0.00"
              />
            </div>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
              Estoque Mínimo *
            </label>
            <input
              type="number"
              required
              value={formData.estoqueMinimo}
              onChange={(e) => setFormData({ ...formData, estoqueMinimo: e.target.value })}
              className="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-amber-500 focus:border-transparent outline-none bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
              placeholder="10"
            />
          </div>

          {formData.bebida && formData.material && formData.tamanho && (
            <div className="bg-amber-50 dark:bg-amber-900/20 border border-amber-200 dark:border-amber-800 rounded-lg p-3">
              <p className="text-sm font-medium text-amber-900 dark:text-amber-200">
                Produto: {formData.bebida} - {formData.tamanho} - {formData.material}
              </p>
            </div>
          )}

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
