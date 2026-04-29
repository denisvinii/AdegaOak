'use client';

import { useEffect, useState } from 'react';
import api from '@/lib/api';
import { History, Search, Calendar } from 'lucide-react';

export default function HistoricoPage() {
  const [historico, setHistorico] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [filtro, setFiltro] = useState('');

  useEffect(() => {
    loadHistorico();
  }, []);

  const loadHistorico = async () => {
    try {
      // Carrega movimentações e despesas para criar um histórico unificado
      const [movimentacoes, despesas] = await Promise.all([
        api.get('/movimentacoes').catch(() => ({ data: [] })),
        api.get('/despesas').catch(() => ({ data: [] })),
      ]);

      const historicoUnificado = [
        ...(Array.isArray(movimentacoes.data) ? movimentacoes.data : []).map((m: any) => ({
          ...m,
          tipo: 'movimentacao',
          tipoDescricao: m.tipo,
          descricao: `${m.tipo} - ${m.produtoDescricao}`,
        })),
        ...(Array.isArray(despesas.data) ? despesas.data : []).map((d: any) => ({
          ...d,
          tipo: 'despesa',
          tipoDescricao: 'Despesa',
          descricao: d.descricao,
        })),
      ].sort((a, b) => new Date(b.data).getTime() - new Date(a.data).getTime());

      setHistorico(historicoUnificado);
    } catch (error) {
      console.error('Erro ao carregar histórico:', error);
      setHistorico([]);
    } finally {
      setLoading(false);
    }
  };

  const historicoFiltrado = (historico || []).filter((item) =>
    (item?.descricao || '').toLowerCase().includes(filtro.toLowerCase())
  );

  if (loading) {
    return (
      <div className="flex items-center justify-center py-12">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-amber-600"></div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold text-gray-900 dark:text-white">Histórico</h1>
        <p className="text-gray-600 dark:text-gray-400 mt-2">
          Histórico completo de movimentações e despesas
        </p>
      </div>

      {/* Search */}
      <div className="bg-white dark:bg-gray-800 rounded-xl shadow-sm p-4 border border-gray-100 dark:border-gray-700">
        <div className="flex items-center gap-3">
          <Search size={20} className="text-gray-400" />
          <input
            type="text"
            placeholder="Buscar no histórico..."
            value={filtro}
            onChange={(e) => setFiltro(e.target.value)}
            className="flex-1 outline-none bg-transparent text-gray-900 dark:text-white placeholder-gray-400"
          />
        </div>
      </div>

      {/* Timeline */}
      <div className="bg-white dark:bg-gray-800 rounded-xl shadow-sm border border-gray-100 dark:border-gray-700 p-6">
        <div className="space-y-4">
          {historicoFiltrado.map((item, index) => (
            <div key={`${item?.tipo || 'item'}-${item?.id || index}`} className="flex gap-4">
              {/* Timeline dot */}
              <div className="flex flex-col items-center">
                <div
                  className={`w-10 h-10 rounded-full flex items-center justify-center ${
                    item?.tipo === 'movimentacao'
                      ? item?.tipoDescricao === 'Entrada'
                        ? 'bg-green-100 dark:bg-green-900'
                        : 'bg-red-100 dark:bg-red-900'
                      : 'bg-blue-100 dark:bg-blue-900'
                  }`}
                >
                  {item?.tipo === 'movimentacao' ? (
                    <History
                      size={20}
                      className={
                        item?.tipoDescricao === 'Entrada'
                          ? 'text-green-600 dark:text-green-400'
                          : 'text-red-600 dark:text-red-400'
                      }
                    />
                  ) : (
                    <Calendar size={20} className="text-blue-600 dark:text-blue-400" />
                  )}
                </div>
                {index < historicoFiltrado.length - 1 && (
                  <div className="w-0.5 h-full bg-gray-200 dark:bg-gray-700 mt-2"></div>
                )}
              </div>

              {/* Content */}
              <div className="flex-1 pb-8">
                <div className="flex items-start justify-between">
                  <div>
                    <p className="font-semibold text-gray-900 dark:text-white">
                      {item?.descricao || 'Sem descrição'}
                    </p>
                    <p className="text-sm text-gray-600 dark:text-gray-400 mt-1">
                      {item?.data ? new Date(item.data).toLocaleString('pt-BR') : '-'}
                    </p>
                    {item?.responsavel && (
                      <p className="text-xs text-gray-500 dark:text-gray-500 mt-1">
                        Por: {item.responsavel}
                      </p>
                    )}
                  </div>
                  <div className="text-right">
                    <p className="font-bold text-gray-900 dark:text-white">
                      R${' '}
                      {(item?.valorTotal || item?.valor || 0).toFixed(2)}
                    </p>
                    {item?.quantidade && (
                      <p className="text-sm text-gray-600 dark:text-gray-400">
                        {item.quantidade} un.
                      </p>
                    )}
                  </div>
                </div>
              </div>
            </div>
          ))}
        </div>

        {historicoFiltrado.length === 0 && (
          <div className="text-center py-12 text-gray-500 dark:text-gray-400">
            Nenhum registro encontrado
          </div>
        )}
      </div>
    </div>
  );
}
