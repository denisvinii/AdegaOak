export interface ProdutoDto {
  id: number;
  bebida: string;
  tamanho: string;
  material: string;
  descricao: string;
  valor: number;
  valorVenda: number;
  quantidadeCaixa: number;
  valorCaixa: number;
  valorAtacadoCaixa: number;
  estoqueMinimo: number;
  quantidadeMinimaAtacado: number;
  quantidade: number;
  valorTotal: number;
  estoqueBaixo: boolean;
  ativo: boolean;
}

export interface EstoqueProduto {
  produtoId: number;
  quantidade: number;
}

export interface CreateProdutoRequest {
  bebida: string;
  tamanho: string;
  material: string;
  valor: number;
  valorVenda: number;
  quantidadeCaixa: number;
  valorCaixa: number;
  valorAtacadoCaixa: number;
  estoqueMinimo: number;
  quantidadeMinimaAtacado?: number;
}

export interface UpdatePrecosRequest {
  valor: number;
  valorVenda: number;
  valorCaixa: number;
  valorAtacadoCaixa: number;
}
