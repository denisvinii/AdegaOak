'use client';

import { useEffect, useState } from 'react';
import api from '@/lib/api';
import { Plus, Trash2, ShoppingCart, X, Minus, DollarSign, CreditCard, Smartphone } from 'lucide-react';
import Modal from '@/components/Modal';

interface Produto {
  id: number;
  bebida: string;
  tamanho: string;
  material: string;
  descricao: string;
  valorVenda: number;
  valorAtacadoCaixa: number;
  quantidadePorCaixa: number;
  ativo: boolean;
}

interface ItemCarrinho {
  produto: Produto;
  quantidade: number;
  tipoVenda: 'Varejo' | 'Atacado';
  valorUnitario: number;
  valorTotal: number;
}

export default function MovimentacoesPage() {
  const [produtos, setProdutos] = useState<Produto[]>([]);
  const [carrinho, setCarrinho] = useState<ItemCarrinho[]>([]);
  const [loading, setLoading] = useState(true);
  const [modalProdutoOpen, setModalProdutoOpen] = useState(false);
  const [modalPagamentoOpen, setModalPagamentoOpen] = useState(false);
  const [salvando, setSalvando] = useState(false);
  const [busca, setBusca] = useState('');
  
  // Dados do produto sendo adicionado
  const [produtoSelecionado, setProdutoSelecionado] = useState<Produto | null>(null);
  const [quantidade, setQuantidade] = useState(1);
  const [tipoVenda, setTipoVenda] = useState<'Varejo' | 'Atacado'>('Varejo');
  
  // Formas de pagamento
  const [valorDinheiro, setValorDinheiro] = useState('');
  const [valorCartao, setValorCartao] = useState('');
  const [valorPix, setValorPix] = useState('');
  const [observacao, setObservacao] = useState('');

  useEffect(() => {
    carregarProdutos();
  }, []);

  const carregarProdutos = async () => {
    try {
      const { data } = await api.get('/produtos');
      setProdutos(data.filter((p: Produto) => p.ativo));
    } catch (error) {
      console.error('Erro ao carregar produtos:', error);
    } finally {
      setLoading(false);
    }
  };

  const produtosFiltrados = produtos.filter(p =>
    p.descricao.toLowerCase().includes(busca.toLowerCase())
  );

  const abrirModalProduto = (produto: Produto) => {
    setProdutoSelecionado(produto);
    setQuantidade(1);
    setTipoVenda('Varejo');
    setModalProdutoOpen(true);
  };

  const adicionarAoCarrinho = () => {
    if (!produtoSelecionado) return;

    const valorUnitario = tipoVenda === 'Varejo' 
      ? produtoSelecionado.valorVenda 
      : produtoSelecionado.valorAtacadoCaixa;

    const itemExistente = carrinho.find(
      item => item.produto.id === produtoSelecionado.id && item.tipoVenda === tipoVenda
    );

    if (itemExistente) {
      setCarrinho(carrinho.map(item =>
        item.produto.id === produtoSelecionado.id && item.tipoVenda === tipoVenda
          ? { ...item, quantidade: item.quantidade + quantidade, valorTotal: (item.quantidade + quantidade) * valorUnitario }
          : item
      ));
    } else {
      setCarrinho([...carrinho, {
        produto: produtoSelecionado,
        quantidade,
        tipoVenda,
        valorUnitario,
        valorTotal: quantidade * valorUnitario
      }]);
    }

    setModalProdutoOpen(false);
    setProdutoSelecionado(null);
  };

  const removerDoCarrinho = (index: number) => {
    setCarrinho(carrinho.filter((_, i) => i !== index));
  };

  const alterarQuantidade = (index: number, novaQuantidade: number) => {
    if (novaQuantidade < 1) return;
    
    setCarrinho(carrinho.map((item, i) =>
      i === index
        ? { ...item, quantidade: novaQuantidade, valorTotal: novaQuantidade * item.valorUnitario }
        : item
    ));
  };

  const valorTotalCarrinho = carrinho.reduce((acc, item) => acc + item.valorTotal, 0);

  const abrirModalPagamento = () => {
    if (carrinho.length === 0) {
      alert('Adicione produtos ao carrinho primeiro!');
      return;
    }
    
    // Preencher automaticamente com o valor total em dinheiro
    setValorDinheiro(valorTotalCarrinho.toFixed(2));
    setValorCartao('0');
    setValorPix('0');
    setObservacao('');
    setModalPagamentoOpen(true);
  };

  const calcularTroco = () => {
    const totalPago = parseFloat(valorDinheiro || '0') + parseFloat(valorCartao || '0') + parseFloat(valorPix || '0');
    return totalPago - valorTotalCarrinho;
  };

  const finalizarVenda = async () => {
    const totalPago = parseFloat(valorDinheiro || '0') + parseFloat(valorCartao || '0') + parseFloat(valorPix || '0');
    
    if (Math.abs(totalPago - valorTotalCarrinho) > 0.01) {
      alert(`O valor pago (R$ ${totalPago.toFixed(2)}) não corresponde ao total da venda (R$ ${valorTotalCarrinho.toFixed(2)})`);
      return;
    }

    setSalvando(true);
    try {
      await api.post('/vendas', {
        itens: carrinho.map(item => ({
          produtoId: item.produto.id,
          quantidade: item.quantidade,
          valorUnitario: item.valorUnitario,
          tipoVenda: item.tipoVenda
        })),
        valorDinheiro: parseFloat(valorDinheiro || '0'),
        valorCartao: parseFloat(valorCartao || '0'),
        valorPix: parseFloat(valorPix || '0'),
        observacao: observacao || null
      });

      alert('Venda registrada com sucesso!');
      setCarrinho([]);
      setModalPagamentoOpen(false);
      setValorDinheiro('');
      setValorCartao('');
      setValorPix('');
      setObservacao('');
    } catch (error: any) {
      console.error('Erro ao finalizar venda:', error);
      alert(error.response?.data?.message || 'Erro ao finalizar venda');
    } finally {
      setSalvando(false);
    }
  };

  const distribuirValor = (forma: 'dinheiro' | 'cartao' | 'pix') => {
    const restante = valorTotalCarrinho;
    setValorDinheiro('0');
    setValorCartao('0');
    setValorPix('0');
    
    if (forma === 'dinheiro') setValorDinheiro(restante.toFixed(2));
    if (forma === 'cartao') setValorCartao(restante.toFixed(2));
    if (forma === 'pix') setValorPix(restante.toFixed(2));
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
          <h1 className="text-3xl font-bold text-gray-900 dark:text-white">PDV - Ponto de Venda</h1>
          <p className="text-gray-600 dark:text-gray-400 mt-2">
            Sistema de vendas com carrinho
          </p>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Lista de Produtos */}
        <div className="lg:col-span-2 space-y-4">
          <div className="bg-white dark:bg-gray-800 rounded-lg shadow-sm p-4">
            <input
              type="text"
              placeholder="Buscar produto..."
              value={busca}
              onChange={(e) => setBusca(e.target.value)}
              className="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-amber-500 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
            />
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            {produtosFiltrados.map(produto => (
              <div
                key={produto.id}
                onClick={() => abrirModalProduto(produto)}
                className="bg-white dark:bg-gray-800 rounded-lg shadow-sm p-4 cursor-pointer hover:shadow-md transition border border-gray-200 dark:border-gray-700"
              >
                <h3 className="font-semibold text-gray-900 dark:text-white mb-2">
                  {produto.descricao}
                </h3>
                <div className="space-y-1 text-sm">
                  <p className="text-gray-600 dark:text-gray-400">
                    Varejo: <span className="font-semibold text-green-600 dark:text-green-400">
                      R$ {produto.valorVenda.toFixed(2)}
                    </span>
                  </p>
                  <p className="text-gray-600 dark:text-gray-400">
                    Atacado: <span className="font-semibold text-blue-600 dark:text-blue-400">
                      R$ {produto.valorAtacadoCaixa.toFixed(2)}
                    </span>
                  </p>
                </div>
              </div>
            ))}
          </div>
        </div>

        {/* Carrinho */}
        <div className="lg:col-span-1">
          <div className="bg-white dark:bg-gray-800 rounded-lg shadow-lg p-6 sticky top-6">
            <div className="flex items-center gap-2 mb-4">
              <ShoppingCart className="text-amber-600" size={24} />
              <h2 className="text-xl font-bold text-gray-900 dark:text-white">
                Carrinho ({carrinho.length})
              </h2>
            </div>

            <div className="space-y-3 mb-4 max-h-96 overflow-y-auto">
              {carrinho.length === 0 ? (
                <p className="text-center text-gray-500 dark:text-gray-400 py-8">
                  Carrinho vazio
                </p>
              ) : (
                carrinho.map((item, index) => (
                  <div key={index} className="bg-gray-50 dark:bg-gray-700 rounded-lg p-3">
                    <div className="flex justify-between items-start mb-2">
                      <div className="flex-1">
                        <p className="font-medium text-sm text-gray-900 dark:text-white">
                          {item.produto.descricao}
                        </p>
                        <p className="text-xs text-gray-600 dark:text-gray-400">
                          {item.tipoVenda} - R$ {item.valorUnitario.toFixed(2)}
                        </p>
                      </div>
                      <button
                        onClick={() => removerDoCarrinho(index)}
                        className="text-red-600 hover:text-red-700 dark:text-red-400"
                      >
                        <Trash2 size={16} />
                      </button>
                    </div>
                    
                    <div className="flex items-center justify-between">
                      <div className="flex items-center gap-2">
                        <button
                          onClick={() => alterarQuantidade(index, item.quantidade - 1)}
                          className="w-6 h-6 flex items-center justify-center bg-gray-200 dark:bg-gray-600 rounded hover:bg-gray-300 dark:hover:bg-gray-500"
                        >
                          <Minus size={14} />
                        </button>
                        <span className="w-8 text-center font-semibold text-gray-900 dark:text-white">
                          {item.quantidade}
                        </span>
                        <button
                          onClick={() => alterarQuantidade(index, item.quantidade + 1)}
                          className="w-6 h-6 flex items-center justify-center bg-gray-200 dark:bg-gray-600 rounded hover:bg-gray-300 dark:hover:bg-gray-500"
                        >
                          <Plus size={14} />
                        </button>
                      </div>
                      <span className="font-bold text-gray-900 dark:text-white">
                        R$ {item.valorTotal.toFixed(2)}
                      </span>
                    </div>
                  </div>
                ))
              )}
            </div>

            <div className="border-t border-gray-200 dark:border-gray-600 pt-4 mb-4">
              <div className="flex justify-between items-center">
                <span className="text-lg font-bold text-gray-900 dark:text-white">Total:</span>
                <span className="text-2xl font-bold text-amber-600">
                  R$ {valorTotalCarrinho.toFixed(2)}
                </span>
              </div>
            </div>

            <button
              onClick={abrirModalPagamento}
              disabled={carrinho.length === 0}
              className="w-full bg-amber-600 hover:bg-amber-700 disabled:opacity-50 disabled:cursor-not-allowed text-white py-3 rounded-lg font-semibold transition flex items-center justify-center gap-2"
            >
              <DollarSign size={20} />
              Finalizar Venda
            </button>
          </div>
        </div>
      </div>

      {/* Modal Adicionar Produto */}
      <Modal
        isOpen={modalProdutoOpen}
        onClose={() => setModalProdutoOpen(false)}
        title="Adicionar ao Carrinho"
      >
        {produtoSelecionado && (
          <div className="space-y-4">
            <div>
              <h3 className="font-semibold text-lg text-gray-900 dark:text-white mb-2">
                {produtoSelecionado.descricao}
              </h3>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                Tipo de Venda
              </label>
              <div className="grid grid-cols-2 gap-3">
                <button
                  onClick={() => setTipoVenda('Varejo')}
                  className={`p-3 rounded-lg border-2 transition ${
                    tipoVenda === 'Varejo'
                      ? 'border-green-500 bg-green-50 dark:bg-green-900/20'
                      : 'border-gray-300 dark:border-gray-600'
                  }`}
                >
                  <p className="font-semibold text-gray-900 dark:text-white">Varejo</p>
                  <p className="text-sm text-green-600 dark:text-green-400">
                    R$ {produtoSelecionado.valorVenda.toFixed(2)}
                  </p>
                </button>
                <button
                  onClick={() => setTipoVenda('Atacado')}
                  className={`p-3 rounded-lg border-2 transition ${
                    tipoVenda === 'Atacado'
                      ? 'border-blue-500 bg-blue-50 dark:bg-blue-900/20'
                      : 'border-gray-300 dark:border-gray-600'
                  }`}
                >
                  <p className="font-semibold text-gray-900 dark:text-white">Atacado</p>
                  <p className="text-sm text-blue-600 dark:text-blue-400">
                    R$ {produtoSelecionado.valorAtacadoCaixa.toFixed(2)}
                  </p>
                </button>
              </div>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                Quantidade
              </label>
              <input
                type="number"
                min="1"
                value={quantidade}
                onChange={(e) => setQuantidade(parseInt(e.target.value) || 1)}
                className="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-amber-500 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
              />
            </div>

            <div className="bg-gray-50 dark:bg-gray-700 p-3 rounded-lg">
              <p className="text-sm text-gray-600 dark:text-gray-400">
                Subtotal: <span className="font-bold text-gray-900 dark:text-white text-lg">
                  R$ {(quantidade * (tipoVenda === 'Varejo' ? produtoSelecionado.valorVenda : produtoSelecionado.valorAtacadoCaixa)).toFixed(2)}
                </span>
              </p>
            </div>

            <div className="flex gap-3">
              <button
                onClick={() => setModalProdutoOpen(false)}
                className="flex-1 px-4 py-2 border border-gray-300 dark:border-gray-600 text-gray-700 dark:text-gray-300 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-700 transition"
              >
                Cancelar
              </button>
              <button
                onClick={adicionarAoCarrinho}
                className="flex-1 px-4 py-2 bg-amber-600 hover:bg-amber-700 text-white rounded-lg transition"
              >
                Adicionar
              </button>
            </div>
          </div>
        )}
      </Modal>

      {/* Modal Pagamento */}
      <Modal
        isOpen={modalPagamentoOpen}
        onClose={() => setModalPagamentoOpen(false)}
        title="Finalizar Pagamento"
      >
        <div className="space-y-4">
          <div className="bg-amber-50 dark:bg-amber-900/20 p-4 rounded-lg">
            <p className="text-sm text-gray-600 dark:text-gray-400">Total da Venda</p>
            <p className="text-3xl font-bold text-amber-600">
              R$ {valorTotalCarrinho.toFixed(2)}
            </p>
          </div>

          <div className="flex gap-2">
            <button
              onClick={() => distribuirValor('dinheiro')}
              className="flex-1 px-3 py-2 bg-green-100 dark:bg-green-900/30 text-green-700 dark:text-green-300 rounded-lg text-sm hover:bg-green-200 dark:hover:bg-green-900/50 transition"
            >
              <DollarSign size={16} className="inline mr-1" />
              Dinheiro
            </button>
            <button
              onClick={() => distribuirValor('cartao')}
              className="flex-1 px-3 py-2 bg-blue-100 dark:bg-blue-900/30 text-blue-700 dark:text-blue-300 rounded-lg text-sm hover:bg-blue-200 dark:hover:bg-blue-900/50 transition"
            >
              <CreditCard size={16} className="inline mr-1" />
              Cartão
            </button>
            <button
              onClick={() => distribuirValor('pix')}
              className="flex-1 px-3 py-2 bg-purple-100 dark:bg-purple-900/30 text-purple-700 dark:text-purple-300 rounded-lg text-sm hover:bg-purple-200 dark:hover:bg-purple-900/50 transition"
            >
              <Smartphone size={16} className="inline mr-1" />
              PIX
            </button>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
              Dinheiro
            </label>
            <input
              type="number"
              step="0.01"
              min="0"
              value={valorDinheiro}
              onChange={(e) => setValorDinheiro(e.target.value)}
              className="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-amber-500 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
              placeholder="0.00"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
              Cartão
            </label>
            <input
              type="number"
              step="0.01"
              min="0"
              value={valorCartao}
              onChange={(e) => setValorCartao(e.target.value)}
              className="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-amber-500 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
              placeholder="0.00"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
              PIX
            </label>
            <input
              type="number"
              step="0.01"
              min="0"
              value={valorPix}
              onChange={(e) => setValorPix(e.target.value)}
              className="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-amber-500 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
              placeholder="0.00"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
              Observação (opcional)
            </label>
            <textarea
              value={observacao}
              onChange={(e) => setObservacao(e.target.value)}
              rows={2}
              className="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-amber-500 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
              placeholder="Observações sobre a venda..."
            />
          </div>

          {calcularTroco() > 0 && (
            <div className="bg-green-50 dark:bg-green-900/20 p-3 rounded-lg">
              <p className="text-sm text-gray-600 dark:text-gray-400">Troco</p>
              <p className="text-xl font-bold text-green-600 dark:text-green-400">
                R$ {calcularTroco().toFixed(2)}
              </p>
            </div>
          )}

          {calcularTroco() < 0 && (
            <div className="bg-red-50 dark:bg-red-900/20 p-3 rounded-lg">
              <p className="text-sm text-red-600 dark:text-red-400">
                Faltam R$ {Math.abs(calcularTroco()).toFixed(2)}
              </p>
            </div>
          )}

          <div className="flex gap-3 pt-4">
            <button
              onClick={() => setModalPagamentoOpen(false)}
              className="flex-1 px-4 py-2 border border-gray-300 dark:border-gray-600 text-gray-700 dark:text-gray-300 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-700 transition"
            >
              Cancelar
            </button>
            <button
              onClick={finalizarVenda}
              disabled={salvando || Math.abs(calcularTroco()) > 0.01 && calcularTroco() < 0}
              className="flex-1 px-4 py-2 bg-green-600 hover:bg-green-700 disabled:opacity-50 disabled:cursor-not-allowed text-white rounded-lg transition font-semibold"
            >
              {salvando ? 'Finalizando...' : 'Confirmar Venda'}
            </button>
          </div>
        </div>
      </Modal>
    </div>
  );
}
