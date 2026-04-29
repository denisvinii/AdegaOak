namespace AdegaOak.Models.DTOs;

public record MovimentacaoDto(
    int Id,
    DateTime Data,
    string Tipo,
    string TipoVenda,
    int ProdutoId,
    string ProdutoDescricao,
    int Quantidade,
    int UsuarioId,
    string Responsavel,
    string? TipoSaida,
    decimal ValorUnitario,
    decimal ValorTotal
);

public record CreateMovimentacaoRequest(
    string Tipo,
    string TipoVenda,
    int ProdutoId,
    int Quantidade,
    string? TipoSaida,
    decimal ValorUnitario
);

public record MovimentacaoFiltroRequest(
    string? Tipo,
    string? TipoVenda,
    int? UsuarioId,
    DateTime? DataInicio,
    DateTime? DataFim,
    int? ProdutoId
);

public record MovimentacaoResumoDto(
    decimal TotalEntradas,
    decimal TotalSaidas,
    decimal Saldo,
    int QuantidadeMovimentacoes
);
