namespace AdegaOak.Models.DTOs;

public record VendaDto(
    int Id,
    DateTime Data,
    int UsuarioId,
    string Responsavel,
    decimal ValorTotal,
    decimal ValorDinheiro,
    decimal ValorCartao,
    decimal ValorPix,
    string? Observacao,
    List<ItemVendaDto> Itens
);

public record ItemVendaDto(
    int ProdutoId,
    string ProdutoDescricao,
    int Quantidade,
    decimal ValorUnitario,
    decimal ValorTotal,
    string TipoVenda
);

public record CreateVendaRequest(
    List<ItemVendaRequest> Itens,
    decimal ValorDinheiro,
    decimal ValorCartao,
    decimal ValorPix,
    string? Observacao
);

public record ItemVendaRequest(
    int ProdutoId,
    int Quantidade,
    decimal ValorUnitario,
    string TipoVenda  // "Varejo" ou "Atacado"
);

public record VendaResumoDto(
    int TotalVendas,
    decimal ValorTotalVendas,
    decimal TotalDinheiro,
    decimal TotalCartao,
    decimal TotalPix,
    List<FormaPagamentoResumoDto> FormasPagamento
);

public record FormaPagamentoResumoDto(
    string Forma,
    decimal Valor,
    int Quantidade,
    decimal Percentual
);
