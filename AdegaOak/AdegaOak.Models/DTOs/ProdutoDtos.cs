namespace AdegaOak.Models.DTOs;

public record ProdutoDto(
    int Id,
    string Bebida,
    string Tamanho,
    string Material,
    string Descricao,
    decimal Valor,
    decimal ValorVenda,
    int QuantidadeCaixa,
    decimal ValorCaixa,
    decimal ValorAtacadoCaixa,
    int EstoqueMinimo,
    int QuantidadeMinimaAtacado,
    int Quantidade,
    decimal ValorTotal,
    bool EstoqueBaixo
);

public record CreateProdutoRequest(
    string Bebida,
    string Tamanho,
    string Material,
    decimal Valor,
    decimal ValorVenda,
    int QuantidadeCaixa,
    decimal ValorCaixa,
    decimal ValorAtacadoCaixa,
    int EstoqueMinimo,
    int QuantidadeMinimaAtacado
);

public record UpdatePrecosRequest(
    decimal ValorVenda,
    decimal ValorCaixa,
    decimal ValorAtacadoCaixa
);
