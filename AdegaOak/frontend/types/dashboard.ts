export interface SaldoDto {
  capitalEmpresa: number;
  investimentoPorFora: number;
  saldo: number;
  totalEntradas: number;
  totalSaidas: number;
  totalDespesasPagas: number;
  totalComboVendas: number;
}

export interface VendasPorDiaDto {
  data: string;
  total: number;
  quantidade: number;
}

export interface TopProdutoDto {
  produtoId: number;
  descricao: string;
  quantidadeVendida: number;
  valorTotal: number;
}

export interface EstoqueBaixoDto {
  produtoId: number;
  descricao: string;
  quantidade: number;
  estoqueMinimo: number;
}

export interface DespesasPorTipoDto {
  tipo: string;
  total: number;
  quantidade: number;
}

export interface VendasPorUsuarioDto {
  usuarioId: number;
  nome: string;
  total: number;
  quantidade: number;
}

export interface DashboardDto {
  saldo: SaldoDto;
  vendasPorDia: VendasPorDiaDto[];
  topProdutos: TopProdutoDto[];
  estoqueBaixo: EstoqueBaixoDto[];
  despesasPorTipo: DespesasPorTipoDto[];
  vendasPorUsuario: VendasPorUsuarioDto[];
  receitaMes: number;
  despesasMes: number;
  totalMovimentacoesMes: number;
}

export interface FiltrosDashboardRequest {
  dataInicio?: string;
  dataFim?: string;
  usuarioId?: number;
}

export interface ProdutoMargemDto {
  produtoId: number;
  descricao: string;
  valor: number;
  valorVenda: number;
  margemLucro: number;
  margemPercentual: number;
}
