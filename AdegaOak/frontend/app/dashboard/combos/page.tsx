'use client';

import { useEffect, useState } from 'react';
import api from '@/lib/api';
import { Plus, Wine, AlertCircle, X } from 'lucide-react';
import Modal from '@/components/Modal';

export default function CombosPage() {
  const [combos, setCombos] = useState<any[]>([]);
  const [produtos, setProdutos] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [modalOpen, setModalOpen] = useState(false);
  const [vendaModalOpen, setVendaModalOpen] = useState(false);
  const [selectedCombo, setSelectedCombo] = useState<any>(null);
  const [saving, setSaving] = useState(false);
  const [formData, setFormData] = useState({
    nome: '',
    descricao: '',
    preco: '',
    ehCopao: false,
    itens: [] as Array<{ produtoId: number; quantidade: number }>,
  });
  const [vendaData, setVendaData] = useState({
    quantidade: '1',
  });

  useEffect(() => {
    loadCombos();
    loadProdutos();
  }, []);

  const loadCombos = async () => {
    try {
      const { data } = await api.get('/combos');
      setCombos(Array.isArray(data) ? data : []);
      setError(null);
    } catch (error: any) {
      console.error('Erro ao carregar combos:', error);
      setError(error.response?.status === 404 ? 'Endpoint não implementado ainda' : 'Erro ao carregar combos');
      setCombos([]);
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
    if (formData.itens.length === 0) {
      alert('Adicione pelo menos um produto ao combo');
      return;
    }
    setSaving(true);

    try {
      await api.post('/combos', {
        nome: formData.nome,
        descricao: formData.descricao,
        preco: parseFloat(formData.preco),
        ehCopao: formData.ehCopao,
        itens: formData.itens,
      });

      setFormData({
        nome: '',
        descricao: '',
        preco: '',
        ehCopao: false,
        itens: [],
      });

      setModalOpen(false);
      loadCombos();
      alert('Combo cadastrado com sucesso!');
    } catch (error: any) {
      console.error('Erro ao cadastrar combo:', error);
      alert(error.response?.data?.message || 'Erro ao cadastrar combo');
    } finally {
      setSaving(false);
    }
  };

  const handleVendaSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSaving(true);

    try {
      await api.post('/combos/vendas', {
        comboId: selectedCombo.id,
        quantidade: parseInt(vendaData.quantidade),
      });

      setVendaData({ quantidade: '1' });
      setVendaModalOpen(false);
      setSelectedCombo(null);
      alert('Venda registrada com sucesso!');
    } catch (error: any) {
      console.error('Erro ao registrar venda:', error);
      alert(error.response?.data?.message || 'Erro ao registrar venda');
    } finally {
      setSaving(false);
    }
  };

  const addItem = () => {
    setFormData({
      ...formData,
      itens: [...formData.itens, { produtoId: 0, quantidade: 1 }],
    });
  };

  const removeItem = (index: number) => {
    setFormData({
      ...formData,
      itens: formData.itens.filter((_, i) => i !== index),
    });
  };

  const updateItem = (index: number, field: 'produtoId' | 'quantidade', value: number) => {
    const newItens = [...formData.itens];
    newItens[index][field] = value;
    setFormData({ ...formData, itens: newItens });
  };

  const openVendaModal = (combo: any) => {
    setSelectedCombo(combo);
    setVendaModalOpen(true);
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center py-12">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-amber-600"></div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="space-y-6">
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-3xl font-bold text-gray-900 dark:text-white">Combos</h1>
            <p className="text-gray-600 dark:text-gray-400 mt-2">
              Crie e gerencie combos de produtos
            </p>
          </div>
          <button className="bg-amber-600 hover:bg-amber-700 text-white px-6 py-3 rounded-lg flex items-center gap-2 transition">
            <Plus size={20} />
            Novo Combo
          </button>
        </div>

        <div className="bg-yellow-50 dark:bg-yellow-900/20 border border-yellow-200 dark:border-yellow-800 rounded-xl p-6">
          <div className="flex items-start gap-3">
            <AlertCircle className="text-yellow-600 dark:text-yellow-400 flex-shrink-0" size={24} />
            <div>
              <h3 className="font-semibold text-yellow-900 dark:text-yellow-200 mb-1">
                Funcionalidade em Desenvolvimento
              </h3>
              <p className="text-sm text-yellow-800 dark:text-yellow-300">
                {error}. Esta funcionalidade será implementada em breve.
              </p>
            </div>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-gray-900 dark:text-white">Combos</h1>
          <p className="text-gray-600 dark:text-gray-400 mt-2">
            Crie e gerencie combos de produtos
          </p>
        </div>
        <button
          onClick={() => setModalOpen(true)}
          className="bg-amber-600 hover:bg-amber-700 text-white px-6 py-3 rounded-lg flex items-center gap-2 transition"
        >
          <Plus size={20} />
          Novo Combo
        </button>
      </div>

      {/* Combos Grid */}
      {(combos || []).length > 0 ? (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {(combos || []).map((combo) => (
            <div
              key={combo?.id || Math.random()}
              className="bg-white dark:bg-gray-800 rounded-xl shadow-sm p-6 border border-gray-100 dark:border-gray-700 hover:shadow-md transition"
            >
              <div className="flex items-start justify-between mb-4">
                <div className="bg-amber-100 dark:bg-amber-900 p-3 rounded-lg">
                  <Wine className="text-amber-600 dark:text-amber-400" size={24} />
                </div>
                <span className="text-2xl font-bold text-amber-600 dark:text-amber-400">
                  R$ {(combo?.preco || 0).toFixed(2)}
                </span>
              </div>

              <h3 className="text-xl font-bold text-gray-900 dark:text-white mb-2">
                {combo?.nome || 'Sem nome'}
              </h3>

              {combo?.descricao && (
                <p className="text-sm text-gray-600 dark:text-gray-400 mb-4">
                  {combo.descricao}
                </p>
              )}

              <div className="border-t border-gray-200 dark:border-gray-700 pt-4">
                <p className="text-xs text-gray-500 dark:text-gray-400 uppercase font-semibold mb-2">
                  Produtos inclusos:
                </p>
                <ul className="space-y-1">
                  {(combo?.itens || []).map((item: any, index: number) => (
                    <li
                      key={index}
                      className="text-sm text-gray-700 dark:text-gray-300 flex items-center gap-2"
                    >
                      <span className="w-1.5 h-1.5 bg-amber-600 dark:bg-amber-400 rounded-full"></span>
                      {item?.quantidade || 0}x {item?.produtoDescricao || 'Produto'}
                    </li>
                  ))}
                </ul>
              </div>

              <div className="mt-4 pt-4 border-t border-gray-200 dark:border-gray-700">
                <button
                  onClick={() => openVendaModal(combo)}
                  className="w-full bg-amber-600 hover:bg-amber-700 text-white py-2 rounded-lg transition"
                >
                  Vender Combo
                </button>
              </div>
            </div>
          ))}
        </div>
      ) : (
        <div className="text-center py-12">
          <Wine className="mx-auto text-gray-400 dark:text-gray-600 mb-4" size={48} />
          <p className="text-gray-500 dark:text-gray-400 mb-2">Nenhum combo cadastrado</p>
          <p className="text-sm text-gray-400 dark:text-gray-500 mb-4">
            Crie combos de produtos para facilitar as vendas
          </p>
          <button 
            onClick={() => setModalOpen(true)}
            className="bg-amber-600 hover:bg-amber-700 text-white px-6 py-2 rounded-lg transition"
          >
            Criar Primeiro Combo
          </button>
        </div>
      )}

      {/* Modal Novo Combo */}
      <Modal
        isOpen={modalOpen}
        onClose={() => setModalOpen(false)}
        title="Novo Combo"
      >
        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
              Nome *
            </label>
            <input
              type="text"
              required
              value={formData.nome}
              onChange={(e) => setFormData({ ...formData, nome: e.target.value })}
              className="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-amber-500 focus:border-transparent outline-none bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
              placeholder="Ex: Combo Festa"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
              Descrição
            </label>
            <textarea
              value={formData.descricao}
              onChange={(e) => setFormData({ ...formData, descricao: e.target.value })}
              className="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-amber-500 focus:border-transparent outline-none bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
              placeholder="Descrição do combo"
              rows={3}
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
              Preço *
            </label>
            <input
              type="number"
              step="0.01"
              required
              min="0"
              value={formData.preco}
              onChange={(e) => setFormData({ ...formData, preco: e.target.value })}
              className="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-amber-500 focus:border-transparent outline-none bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
              placeholder="0.00"
            />
          </div>

          <div className="flex items-center gap-2">
            <input
              type="checkbox"
              id="ehCopao"
              checked={formData.ehCopao}
              onChange={(e) => setFormData({ ...formData, ehCopao: e.target.checked })}
              className="w-4 h-4 text-amber-600 border-gray-300 rounded focus:ring-amber-500"
            />
            <label htmlFor="ehCopao" className="text-sm text-gray-700 dark:text-gray-300">
              É Copão
            </label>
          </div>

          <div>
            <div className="flex items-center justify-between mb-2">
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">
                Produtos *
              </label>
              <button
                type="button"
                onClick={addItem}
                className="text-sm text-amber-600 hover:text-amber-700 font-medium"
              >
                + Adicionar Produto
              </button>
            </div>

            <div className="space-y-2">
              {formData.itens.map((item, index) => (
                <div key={index} className="flex gap-2">
                  <select
                    required
                    value={item.produtoId}
                    onChange={(e) => updateItem(index, 'produtoId', parseInt(e.target.value))}
                    className="flex-1 px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-amber-500 focus:border-transparent outline-none bg-white dark:bg-gray-700 text-gray-900 dark:text-white text-sm"
                  >
                    <option value="0">Selecione um produto</option>
                    {(produtos || []).map((produto) => (
                      <option key={produto?.id || Math.random()} value={produto?.id || 0}>
                        {produto?.descricao || 'Produto'}
                      </option>
                    ))}
                  </select>
                  <input
                    type="number"
                    required
                    min="1"
                    value={item.quantidade}
                    onChange={(e) => updateItem(index, 'quantidade', parseInt(e.target.value))}
                    className="w-20 px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-amber-500 focus:border-transparent outline-none bg-white dark:bg-gray-700 text-gray-900 dark:text-white text-sm"
                    placeholder="Qtd"
                  />
                  <button
                    type="button"
                    onClick={() => removeItem(index)}
                    className="p-2 text-red-600 hover:bg-red-50 dark:hover:bg-red-900/20 rounded-lg transition"
                  >
                    <X size={20} />
                  </button>
                </div>
              ))}
            </div>

            {formData.itens.length === 0 && (
              <p className="text-sm text-gray-500 dark:text-gray-400 text-center py-4">
                Nenhum produto adicionado
              </p>
            )}
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

      {/* Modal Vender Combo */}
      <Modal
        isOpen={vendaModalOpen}
        onClose={() => {
          setVendaModalOpen(false);
          setSelectedCombo(null);
        }}
        title="Vender Combo"
      >
        {selectedCombo && (
          <form onSubmit={handleVendaSubmit} className="space-y-4">
            <div className="bg-gray-50 dark:bg-gray-700 p-4 rounded-lg">
              <h3 className="font-bold text-gray-900 dark:text-white mb-2">
                {selectedCombo?.nome || 'Combo'}
              </h3>
              <p className="text-sm text-gray-600 dark:text-gray-400 mb-2">
                {selectedCombo?.descricao || ''}
              </p>
              <p className="text-2xl font-bold text-amber-600 dark:text-amber-400">
                R$ {(selectedCombo?.preco || 0).toFixed(2)}
              </p>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                Quantidade *
              </label>
              <input
                type="number"
                required
                min="1"
                value={vendaData.quantidade}
                onChange={(e) => setVendaData({ quantidade: e.target.value })}
                className="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-amber-500 focus:border-transparent outline-none bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
                placeholder="1"
              />
            </div>

            <div className="bg-gray-50 dark:bg-gray-700 p-3 rounded-lg">
              <p className="text-sm text-gray-600 dark:text-gray-400">
                Valor Total: <span className="font-bold text-gray-900 dark:text-white">
                  R$ {((selectedCombo?.preco || 0) * parseInt(vendaData.quantidade || '1')).toFixed(2)}
                </span>
              </p>
            </div>

            <div className="flex gap-3 pt-4">
              <button
                type="button"
                onClick={() => {
                  setVendaModalOpen(false);
                  setSelectedCombo(null);
                }}
                className="flex-1 px-4 py-2 border border-gray-300 dark:border-gray-600 text-gray-700 dark:text-gray-300 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-700 transition"
              >
                Cancelar
              </button>
              <button
                type="submit"
                disabled={saving}
                className="flex-1 px-4 py-2 bg-amber-600 hover:bg-amber-700 disabled:opacity-50 text-white rounded-lg transition"
              >
                {saving ? 'Processando...' : 'Confirmar Venda'}
              </button>
            </div>
          </form>
        )}
      </Modal>
    </div>
  );
}
