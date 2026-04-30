import { useState, useCallback } from 'react';
import api from '@/lib/api';
import type { ProdutoDto, EstoqueProduto } from '@/types/produto';

export function useProdutos() {
  const [produtos, setProdutos] = useState<ProdutoDto[]>([]);
  const [estoque, setEstoque] = useState<EstoqueProduto[]>([]);
  const [loading, setLoading] = useState(false);

  const loadProdutos = useCallback(async () => {
    setLoading(true);
    try {
      const { data } = await api.get<ProdutoDto[]>('/produtos');
      setProdutos(Array.isArray(data) ? data : []);
    } catch (err) {
      console.error('Erro ao carregar produtos:', err);
      setProdutos([]);
    } finally {
      setLoading(false);
    }
  }, []);

  const loadEstoque = useCallback(async () => {
    try {
      const { data } = await api.get<EstoqueProduto[]>('/produtos/estoque');
      setEstoque(Array.isArray(data) ? data : []);
    } catch (err) {
      console.error('Erro ao carregar estoque:', err);
      setEstoque([]);
    }
  }, []);

  const getEstoqueProduto = useCallback(
    (produtoId: number): number => {
      return estoque.find((e) => e.produtoId === produtoId)?.quantidade ?? 0;
    },
    [estoque]
  );

  return { produtos, estoque, loading, loadProdutos, loadEstoque, getEstoqueProduto };
}
