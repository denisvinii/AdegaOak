'use client';

import { useEffect, useState } from 'react';
import api from '@/lib/api';
import { Package, AlertTriangle, Plus, Edit } from 'lucide-react';
import Modal from '@/components/Modal';
import LoadingButton from '@/components/LoadingButton';
import { useToast } from '@/components/Toast';
import { useAuthStore } from '@/store/authStore';
import type { ProdutoDto, CreateProdutoRequest, UpdatePrecosRequest } from '@/types/produto';

type FormData = {
  bebida: string;
  tamanho: string;
  material: string;
  valor: string;
  valorVenda: string;
  quantidadeCaixa: string;
  valorCaixa: string;
  valorAtacadoCaixa: string;
  estoqueMinimo: string;
};

const FORM_INITIAL: FormData = {
  bebida: '',
  tamanho: '',
  material: '',
  valor: '',
  valorVenda: '',
  quantidadeCaixa: '12',
  valorCaixa: '',
  valorAtacadoCaixa: '',
  estoqueMinimo: '10',
};

type EditFormData = {
  valor: string;
  valorVenda: string;
  valorCaixa: string;
  valorAtacadoCaixa: string;
};

export default function EstoquePage() {
  const toast = useToast();
  const [produtos, setProdutos] = useState<ProdutoDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [modalOpen, setModalOpen] = useState(false);
  const [editModalOpen, setEditModalOpen] = useState(false);
  const [produtoEditando, setProdutoEditando] = useState<ProdutoDto | null>(null);
  const [formData, setFormData] = useState<FormData>(FORM_INITIAL);
  const [editFormData, setEditFormData] = useState<EditFormData>({
    valor: '',
    valorVenda: '',
    valorCaixa: '',
    valorAtacadoCaixa: '',
  });
  const [saving, setSaving] = useState(false);

  const isAdmin = useAuthStore((state) => state.isAdmin);

  useEffect(() => {
    loadProdutos();
  }, []);

  const loadProdutos = async () => {
    try {
      const { data } = await api.get<ProdutoDto[]>('/produtos');
      setProdutos(Array.isArray(data) ? data : []);
    } catch {
      toast.error('Erro ao carregar produtos');
      setProdutos([]);
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    setSaving(true);
    try {
      const payload: CreateProdutoRequest = {
        bebida: formData.bebida,
        tamanho: formData.tamanho,
        material: formData.material,
        valor: parseFloat(formData.valor),
        valorVenda: parseFloat(formData.valorVenda),
        quantidadeCaixa: parseInt(formData.quantidadeCaixa),
        valorCaixa: parseFloat(formData.valorCaixa),
        valorAtacadoCaixa: parseFloat(formData.valorAtacadoCaixa),
        estoqueMinimo: parseInt(formData.estoqueMinimo),
      };
      await api.post('/produtos', payload);
      setFormData(FORM_INITIAL);
      setModalOpen(false);
      loadProdutos();
      toast.success('Produto cadastrado com sucesso!');
    } catch (error: unknown) {
      const msg =
        (error as { response?: { data?: { message?: string } } })?.response?.data?.message ??
        'Erro ao cadastrar produto';
      toast.error(msg);
    } finally {
      setSaving(false);
    }
  };

  const abrirModalEdicao = (produto: ProdutoDto) => {
    setProdutoEditando(produto);
    setEditFormData({
      valor: produto.valor.toString(),
      valorVenda: produto.valorVenda.toString(),
      valorCaixa: produto.valorCaixa.toString(),
      valorAtacadoCaixa: produto.valorAtacadoCaixa.toString(),
    });
    setEditModalOpen(true);
  };

  const handleEditSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    if (!produtoEditando) return;
    setSaving(true);
    try {
      const payload: UpdatePrecosRequest = {
        valor: parseFloat(editFormData.valor),
        valorVenda: parseFloat(editFormData.valorVenda),
        valorCaixa: parseFloat(editFormData.valorCaixa),
        valorAtacadoCaixa: parseFloat(editFormData.valorAtacadoCaixa),
      };
      await api.put(`/produtos/${produtoEditando.id}/precos`, payload);
      setEditModalOpen(false);
      setProdutoEditando(null);
      loadProdutos();
      toast.success('Preços atualizados com sucesso!');
    } catch (error: unknown) {
      const err = error as { response?: { data?: { message?: string; details?: string }; status?: number }; message?: string };
      let msg = 'Erro ao atualizar preços';
      if (err.response?.data?.message) msg = err.response.data.message;
      else if (err.response?.status === 401) msg = 'Não autorizado. Faça login novamente.';
      else if (err.response?.status === 403) msg = 'Apenas administradores podem editar preços.';
      else if (err.message === 'Network Error') msg = 'Erro de conexão com o servidor.';
      toast.error(msg);
    } finally {
      setSaving(false);
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center py-12">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-amber-600" />
      </div>
    );
  }

  const margemCalc =
    editFormData.valor && editFormData.valorVenda
      ? (
          ((parseFloat(editFormData.valorVenda) - parseFloat(editFormData.valor)) /
            parseFloat(editFormData.valor)) *
          100
        ).toFixed(1)
      : null;

  return (
    <div className="space-y-4 md:space-y-6">
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 className="text-2xl md:text-3xl font-bold text-gray-900 dark:text-white">Estoque</h1>
          <p className="text-sm md:text-base text-gray-600 dark:text-gray-400 mt-1 md:mt-2">
            Gerenciamento de produtos
          </p>
        </div>
        <button
          onClick={() => setModalOpen(true)}
          className="bg-amber-600 hover:bg-amber-700 text-white px-4 md:px-6 py-2 md:py-3 rounded-lg flex items-center justify-center gap-2 transition text-sm md:text-base"
        >
          <Plus size={18} />
          <span className="hidden sm:inline">Novo Produto</span>
          <span className="sm:hidden">Novo</span>
        </button>
      </div>

      <div className="bg-white dark:bg-gray-800 rounded-lg md:rounded-xl shadow-sm border border-gray-100 dark:border-gray-700 overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full min-w-[800px]">
            <thead className="bg-gray-50 dark:bg-gray-900 border-b border-gray-200 dark:border-gray-700">
              <tr>
                {['Produto', 'Tipo', 'Tamanho', 'Qtd/Caixa', 'Estoque', 'Custo', 'Venda', 'Caixa', 'Atacado', ...(isAdmin() ? ['Ações'] : [])].map(
                  (h) => (
                    <th
                      key={h}
                      className="px-3 md:px-6 py-2 md:py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase"
                    >
                      {h}
                    </th>
                  )
                )}
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200 dark:divide-gray-700">
              {produtos.map((produto) => (
                <tr key={produto.id} className="hover:bg-gray-50 dark:hover:bg-gray-700">
                  <td className="px-3 md:px-6 py-3 md:py-4">
                    <div className="flex items-center gap-2">
                      <Package size={14} className="text-gray-400 flex-shrink-0" />
                      <span className="font-medium text-xs md:text-sm text-gray-900 dark:text-white truncate">
                        {produto.bebida}
                      </span>
                    </div>
                  </td>
                  <td className="px-3 md:px-6 py-3 md:py-4 text-xs md:text-sm text-gray-600 dark:text-gray-400">
                    {produto.material}
                  </td>
                  <td className="px-3 md:px-6 py-3 md:py-4 text-xs md:text-sm text-gray-600 dark:text-gray-400">
                    {produto.tamanho}
                  </td>
                  <td className="px-3 md:px-6 py-3 md:py-4 text-xs md:text-sm text-gray-600 dark:text-gray-400">
                    {produto.quantidadeCaixa}
                  </td>
                  <td className="px-3 md:px-6 py-3 md:py-4">
                    <div className="flex items-center gap-2">
                      <span
                        className={`font-semibold text-xs md:text-sm ${
                          produto.estoqueBaixo
                            ? 'text-red-600 dark:text-red-400'
                            : 'text-gray-900 dark:text-white'
                        }`}
                      >
                        {produto.quantidade}
                      </span>
                      {produto.estoqueBaixo && <AlertTriangle size={14} className="text-red-500" />}
                    </div>
                  </td>
                  <td className="px-3 md:px-6 py-3 md:py-4 text-xs md:text-sm text-gray-900 dark:text-white whitespace-nowrap">
                    R$ {produto.valor.toFixed(2)}
                  </td>
                  <td className="px-3 md:px-6 py-3 md:py-4 text-xs md:text-sm text-gray-900 dark:text-white whitespace-nowrap">
                    R$ {produto.valorVenda.toFixed(2)}
                  </td>
                  <td className="px-3 md:px-6 py-3 md:py-4 text-xs md:text-sm text-gray-900 dark:text-white whitespace-nowrap">
                    R$ {produto.valorCaixa.toFixed(2)}
                  </td>
                  <td className="px-3 md:px-6 py-3 md:py-4 text-xs md:text-sm text-gray-900 dark:text-white whitespace-nowrap">
                    R$ {produto.valorAtacadoCaixa.toFixed(2)}
                  </td>
                  {isAdmin() && (
                    <td className="px-3 md:px-6 py-3 md:py-4">
                      <button
                        onClick={() => abrirModalEdicao(produto)}
                        className="text-amber-600 hover:text-amber-700 dark:text-amber-400 dark:hover:text-amber-300 transition"
                        title="Editar preços"
                      >
                        <Edit size={16} />
                      </button>
                    </td>
                  )}
                </tr>
              ))}
            </tbody>
          </table>
        </div>
        {produtos.length === 0 && (
          <div className="text-center py-8 md:py-12 text-sm md:text-base text-gray-500 dark:text-gray-400">
            Nenhum produto cadastrado
          </div>
        )}
      </div>

      {/* Modal Novo Produto */}
      <Modal isOpen={modalOpen} onClose={() => setModalOpen(false)} title="Novo Produto">
        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
              Tipo de Produto *
            </label>
            <select
              required
              value={formData.material}
              onChange={(e) => setFormData({ ...formData, material: e.target.value, tamanho: '' })}
              className="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-amber-500 outline-none bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
            >
              <option value="">Selecione o tipo</option>
              <option value="Copo">Copo</option>
              <option value="Garrafa">Garrafa</option>
              <option value="Lata">Lata</option>
              <option value="Long Neck">Long Neck</option>
              <option value="Barril">Barril</option>
              <option value="Outros">Outros (Cigarro, Isqueiro, etc)</option>
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
                className="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-amber-500 outline-none bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
              >
                <option value="">Selecione o tamanho</option>
                {formData.material === 'Copo' && ['200ml','250ml','300ml','350ml','400ml','500ml','770ml'].map(s => <option key={s} value={s}>{s}</option>)}
                {formData.material === 'Garrafa' && ['600ml','1L','1.5L','2L'].map(s => <option key={s} value={s}>{s}</option>)}
                {formData.material === 'Lata' && ['269ml','310ml','350ml','473ml'].map(s => <option key={s} value={s}>{s}</option>)}
                {formData.material === 'Long Neck' && ['275ml','330ml','355ml'].map(s => <option key={s} value={s}>{s}</option>)}
                {formData.material === 'Barril' && ['5L','10L','20L','30L','50L'].map(s => <option key={s} value={s}>{s}</option>)}
                {formData.material === 'Outros' && ['0g','Unidade','Maço','Caixa'].map(s => <option key={s} value={s}>{s}</option>)}
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
              className="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-amber-500 outline-none bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
              placeholder="Ex: Heineken, Coca-Cola..."
            />
          </div>

          <div className="grid grid-cols-2 gap-4">
            {[
              { label: 'Valor de Custo *', key: 'valor', placeholder: '0.00' },
              { label: 'Valor de Venda *', key: 'valorVenda', placeholder: '0.00' },
            ].map(({ label, key, placeholder }) => (
              <div key={key}>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">{label}</label>
                <input
                  type="number"
                  step="0.01"
                  required
                  value={formData[key as keyof FormData]}
                  onChange={(e) => setFormData({ ...formData, [key]: e.target.value })}
                  className="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-amber-500 outline-none bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
                  placeholder={placeholder}
                />
              </div>
            ))}
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
              className="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-amber-500 outline-none bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
            />
          </div>

          <div className="grid grid-cols-2 gap-4">
            {[
              { label: 'Valor Caixa *', key: 'valorCaixa' },
              { label: 'Valor Atacado Caixa *', key: 'valorAtacadoCaixa' },
            ].map(({ label, key }) => (
              <div key={key}>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">{label}</label>
                <input
                  type="number"
                  step="0.01"
                  required
                  value={formData[key as keyof FormData]}
                  onChange={(e) => setFormData({ ...formData, [key]: e.target.value })}
                  className="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-amber-500 outline-none bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
                  placeholder="0.00"
                />
              </div>
            ))}
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
              className="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-amber-500 outline-none bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
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

      {/* Modal Editar Preços */}
      <Modal
        isOpen={editModalOpen}
        onClose={() => { setEditModalOpen(false); setProdutoEditando(null); }}
        title="Editar Preços"
      >
        {produtoEditando && (
          <form onSubmit={handleEditSubmit} className="space-y-3 md:space-y-4">
            <div className="bg-gray-50 dark:bg-gray-700 p-3 md:p-4 rounded-lg">
              <h3 className="font-bold text-sm md:text-base text-gray-900 dark:text-white">
                {produtoEditando.descricao}
              </h3>
              <p className="text-xs md:text-sm text-gray-600 dark:text-gray-400 mt-1">
                Estoque atual: {produtoEditando.quantidade} unidades
              </p>
            </div>

            {[
              { label: 'Valor de Custo (Unitário) *', key: 'valor', hint: 'Quanto você paga por unidade' },
              { label: 'Valor de Venda (Varejo) *', key: 'valorVenda', hint: 'Preço de venda unitário (mínimo 10% acima do custo)' },
              { label: 'Valor por Caixa *', key: 'valorCaixa', hint: `Preço de venda por caixa (${produtoEditando.quantidadeCaixa} unidades)` },
              { label: 'Valor Atacado (por Caixa) *', key: 'valorAtacadoCaixa', hint: 'Preço de atacado por caixa' },
            ].map(({ label, key, hint }) => (
              <div key={key}>
                <label className="block text-xs md:text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                  {label}
                </label>
                <input
                  type="number"
                  step="0.01"
                  required
                  min="0"
                  value={editFormData[key as keyof EditFormData]}
                  onChange={(e) => setEditFormData({ ...editFormData, [key]: e.target.value })}
                  className="w-full px-3 md:px-4 py-2 text-sm md:text-base border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-amber-500 outline-none bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
                  placeholder="0.00"
                />
                <p className="text-xs text-gray-500 dark:text-gray-400 mt-1">{hint}</p>
              </div>
            ))}

            {margemCalc !== null && (
              <div className="bg-blue-50 dark:bg-blue-900/20 border border-blue-200 dark:border-blue-800 rounded-lg p-3">
                <p className="text-xs md:text-sm font-medium text-blue-900 dark:text-blue-200">
                  Margem de Lucro: {margemCalc}%
                </p>
                <p className="text-xs text-blue-700 dark:text-blue-300 mt-1">
                  Lucro por unidade: R${' '}
                  {(parseFloat(editFormData.valorVenda) - parseFloat(editFormData.valor)).toFixed(2)}
                </p>
              </div>
            )}

            <div className="flex gap-2 md:gap-3 pt-3 md:pt-4">
              <button
                type="button"
                onClick={() => { setEditModalOpen(false); setProdutoEditando(null); }}
                className="flex-1 px-3 md:px-4 py-2 text-sm md:text-base border border-gray-300 dark:border-gray-600 text-gray-700 dark:text-gray-300 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-700 transition"
              >
                Cancelar
              </button>
              <LoadingButton
                type="submit"
                loading={saving}
                loadingText="Salvando..."
                className="flex-1 px-3 md:px-4 py-2 text-sm md:text-base bg-amber-600 hover:bg-amber-700 text-white rounded-lg"
              >
                Atualizar
              </LoadingButton>
            </div>
          </form>
        )}
      </Modal>
    </div>
  );
}
