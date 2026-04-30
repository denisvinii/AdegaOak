import type { ProdutoDto } from './produto';

export interface ItemVenda {
  produtoId: number;
  produtoDescricao: string;
  quantidade: number;
  valorUnitario: number;
  valorTotal: number;
  tipoVenda: string;
}

export interface VendaDto {
  id: number;
  data: string;
  usuarioId: number;
  responsavel: string;
  valorTotal: number;
  valorDinheiro: number;
  valorCartao: number;
  valorPix: number;
  observacao: string | null;
  itens: ItemVenda[];
}

export interface ItemCarrinho {
  produto: ProdutoDto;
  quantidade: number;
  tipoVenda: 'Varejo' | 'Atacado';
  valorUnitario: number;
  valorTotal: number;
}

export interface CreateVendaRequest {
  itens: {
    produtoId: number;
    quantidade: number;
    valorUnitario: number;
    tipoVenda: string;
  }[];
  valorDinheiro: number;
  valorCartao: number;
  valorPix: number;
  observacao: string | null;
}

export interface VendasPagedResult {
  items: VendaDto[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}
