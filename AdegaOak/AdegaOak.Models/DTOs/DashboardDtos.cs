namespace AdegaOak.Models.DTOs;

public record SaldoDto(
    decimal CapitalEmpresa,
    decimal InvestimentoPorFora,
    decimal Saldo,
    decimal TotalEntradas,
    decimal TotalSaidas,
    decimal TotalDespesasPagas,
    decimal TotalComboVendas
);

public record DashboardDto(
    SaldoDto Saldo,
    List<VendasPorDiaDto> VendasPorDia,
    List<TopProdutoDto> TopProdutos,
    List<EstoqueBaixoDto> EstoqueBaixo,
    List<DespesasPorTipoDto> DespesasPorTipo,
    List<VendasPorUsuarioDto> VendasPorUsuario,
    decimal ReceitaMes,
    decimal DespesasMes,
    int TotalMovimentacoesMes
);

public record VendasPorDiaDto(DateTime Data, decimal Total, int Quantidade);

public record TopProdutoDto(int ProdutoId, string Descricao, int QuantidadeVendida, decimal ValorTotal);

public record EstoqueBaixoDto(int ProdutoId, string Descricao, int Quantidade, int EstoqueMinimo);

public record DespesasPorTipoDto(string Tipo, decimal Total, int Quantidade);

public record VendasPorUsuarioDto(int UsuarioId, string Nome, decimal Total, int Quantidade);

public record FiltrosDashboardRequest(
    DateTime? DataInicio,
    DateTime? DataFim,
    int? UsuarioId
);

public record VendaMensalDto(
    int Mes,
    int Ano,
    string MesNome,
    decimal TotalVendas,
    int QuantidadeVendas,
    decimal TicketMedio
);

public record RelatorioVendasDto(
    List<VendaMensalDto> VendasMensais,
    List<VendasPorUsuarioDto> VendasPorUsuario,
    decimal TotalGeral,
    int QuantidadeGeral,
    decimal TicketMedioGeral
);

public record FiltrosRelatorioRequest(
    int Ano,
    int? Mes,
    int? UsuarioId
);

public record ProdutoMargemDto(
    int ProdutoId,
    string Descricao,
    decimal Valor,
    decimal ValorVenda,
    decimal MargemLucro,
    decimal MargemPercentual
);
