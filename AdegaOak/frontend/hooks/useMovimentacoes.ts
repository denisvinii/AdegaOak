import { useState, useCallback } from 'react';
import api from '@/lib/api';
import type { MovimentacaoDto, MovimentacaoFiltroRequest, PagedResult } from '@/types/movimentacao';

const PAGE_SIZE = 50;

export function useMovimentacoes() {
  const [movimentacoes, setMovimentacoes] = useState<MovimentacaoDto[]>([]);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(false);

  const load = useCallback(async (filtros: MovimentacaoFiltroRequest, targetPage = 1) => {
    setLoading(true);
    try {
      // Usa o endpoint de filtro do backend — não filtra no frontend
      const { data } = await api.post<MovimentacaoDto[]>('/movimentacoes/filtrar', filtros);
      const all = Array.isArray(data) ? data : [];

      // Paginação client-side sobre o resultado filtrado
      // (o backend já filtra por tipo/data, só paginamos o resultado)
      const total = all.length;
      const pages = Math.max(1, Math.ceil(total / PAGE_SIZE));
      const start = (targetPage - 1) * PAGE_SIZE;
      const slice = all.slice(start, start + PAGE_SIZE);

      setMovimentacoes(slice);
      setTotalCount(total);
      setTotalPages(pages);
      setPage(targetPage);
    } catch (err) {
      console.error('Erro ao carregar movimentações:', err);
      setMovimentacoes([]);
    } finally {
      setLoading(false);
    }
  }, []);

  return { movimentacoes, page, totalPages, totalCount, pageSize: PAGE_SIZE, loading, load, setPage };
}
