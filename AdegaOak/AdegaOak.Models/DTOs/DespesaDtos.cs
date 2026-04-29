using AdegaOak.Models.Models;

namespace AdegaOak.Models.DTOs;

public record DespesaDto(
    int Id,
    string Descricao,
    decimal Valor,
    DateTime Data,
    TipoDespesa Tipo,
    string TipoNome,
    bool Pago,
    DateTime? DataPagamento,
    string? Notas,
    int? ProdutoId,
    string? ProdutoDescricao,
    int Quantidade,
    DateTime CriadoEm
);

public record CreateDespesaRequest(
    string Descricao,
    decimal Valor,
    DateTime Data,
    TipoDespesa Tipo,
    string? Notas,
    int? ProdutoId,
    int Quantidade
);

public record UpdateDespesaRequest(
    string? Descricao,
    decimal? Valor,
    DateTime? Data,
    TipoDespesa? Tipo,
    string? Notas
);

public record DespesaResumoDto(
    decimal TotalPago,
    decimal TotalPendente,
    decimal Total,
    int QuantidadePagas,
    int QuantidadePendentes
);
