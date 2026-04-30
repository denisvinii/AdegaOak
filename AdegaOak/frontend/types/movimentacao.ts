export interface MovimentacaoDto {
  id: number;
  data: string;
  tipo: 'Entrada' | 'Saída';
  tipoVenda: string;
  produtoId: number;
  produtoDescricao: string;
  quantidade: number;
  usuarioId: number;
  responsavel: string;
  tipoSaida: string | null;
  valorUnitario: number;
  valorTotal: number;
}

export interface CreateMovimentacaoRequest {
  tipo: string;
  tipoVenda: string;
  produtoId: number;
  quantidade: number;
  tipoSaida?: string | null;
  valorUnitario: number;
}

export interface MovimentacaoFiltroRequest {
  tipo?: string;
  tipoVenda?: string;
  usuarioId?: number;
  dataInicio?: string;
  dataFim?: string;
  produtoId?: number;
}

export interface MovimentacaoResumoDto {
  totalEntradas: number;
  totalSaidas: number;
  saldo: number;
  quantidadeMovimentacoes: number;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}
