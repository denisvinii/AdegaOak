namespace AdegaOak.Models.DTOs;

public record ComboComposicaoDto(
    int Id,
    int ProdutoId,
    string ProdutoDescricao,
    decimal Quantidade,
    string Unidade,
    bool DebitaEstoque
);

public record ComboDto(
    int Id,
    string Nome,
    string? Descricao,
    decimal PrecoVenda,
    bool Ativo,
    bool EhCopao,
    DateTime CriadoEm,
    List<ComboComposicaoDto> Composicao
);

public record CreateComboRequest(
    string Nome,
    string? Descricao,
    decimal PrecoVenda,
    bool EhCopao,
    List<ComboComposicaoItemRequest> Composicao
);

public record ComboComposicaoItemRequest(
    int ProdutoId,
    decimal Quantidade,
    string Unidade,
    bool DebitaEstoque
);

public record UpdateComboRequest(
    string? Nome,
    string? Descricao,
    decimal? PrecoVenda,
    bool? Ativo,
    List<ComboComposicaoItemRequest>? Composicao
);

public record ComboVendaDto(
    int Id,
    int ComboId,
    string NomeCombo,
    int UsuarioId,
    string Responsavel,
    int Quantidade,
    decimal PrecoUnitario,
    decimal PrecoTotal,
    DateTime DataVenda,
    string? Observacoes,
    string TipoMovimento
);

public record CreateComboVendaRequest(
    int ComboId,
    int Quantidade,
    decimal PrecoUnitario,
    string? Observacoes,
    string TipoMovimento = "Normal"
);
