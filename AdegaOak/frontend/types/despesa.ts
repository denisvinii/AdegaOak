export type TipoDespesa = 'Fixa' | 'Variavel' | 'Eventual';

export interface DespesaDto {
  id: number;
  descricao: string;
  valor: number;
  data: string;
  tipo: TipoDespesa;
  tipoNome: string;
  pago: boolean;
  dataPagamento: string | null;
  notas: string | null;
  produtoId: number | null;
  produtoDescricao: string | null;
  quantidade: number;
  criadoEm: string;
}

export interface CreateDespesaRequest {
  descricao: string;
  valor: number;
  tipo: TipoDespesa;
  pago: boolean;
}

export interface UpdateDespesaRequest {
  descricao?: string;
  valor?: number;
  tipo?: TipoDespesa;
}

export interface DespesaResumoDto {
  totalPago: number;
  totalPendente: number;
  total: number;
  quantidadePagas: number;
  quantidadePendentes: number;
}
