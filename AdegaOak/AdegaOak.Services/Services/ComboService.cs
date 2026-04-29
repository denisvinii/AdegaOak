using AdegaOak.Data.Repositories;
using AdegaOak.Models.DTOs;
using AdegaOak.Models.Models;
using AdegaOak.Services.Interfaces;

namespace AdegaOak.Services.Services;

public class ComboService(
    IComboRepository comboRepository,
    IProdutoRepository produtoRepository,
    IMovimentacaoRepository movimentacaoRepository) : IComboService
{
    public async Task<List<ComboDto>> GetAllAsync(bool? ehCopao = null)
    {
        var combos = await comboRepository.GetAllAsync(ehCopao);
        return combos.Select(MapToDto).ToList();
    }

    public async Task<ComboDto?> GetByIdAsync(int id)
    {
        var combo = await comboRepository.GetByIdAsync(id);
        return combo == null ? null : MapToDto(combo);
    }

    public async Task<ComboDto> CreateAsync(CreateComboRequest request)
    {
        if (await comboRepository.NomeExisteAsync(request.Nome))
            throw new InvalidOperationException($"Combo '{request.Nome}' já existe.");

        if (request.Composicao.Count == 0)
            throw new InvalidOperationException("Combo deve ter pelo menos 1 produto.");

        if (request.PrecoVenda <= 0)
            throw new InvalidOperationException("Preço de venda deve ser maior que zero.");

        var combo = new Combo
        {
            Nome = request.Nome,
            Descricao = request.Descricao,
            PrecoVenda = request.PrecoVenda,
            EhCopao = request.EhCopao,
            Composicao = request.Composicao.Select(c => new ComboComposicao
            {
                ProdutoId = c.ProdutoId,
                Quantidade = c.Quantidade,
                Unidade = c.Unidade,
                DebitaEstoque = c.DebitaEstoque
            }).ToList()
        };

        await comboRepository.CreateAsync(combo);
        return MapToDto(combo);
    }

    public async Task<ComboDto> UpdateAsync(int id, UpdateComboRequest request)
    {
        var combo = await comboRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Combo {id} não encontrado.");

        if (request.Nome != null && request.Nome != combo.Nome)
        {
            if (await comboRepository.NomeExisteAsync(request.Nome, id))
                throw new InvalidOperationException($"Combo '{request.Nome}' já existe.");
            combo.Nome = request.Nome;
        }

        if (request.Descricao != null) combo.Descricao = request.Descricao;
        if (request.PrecoVenda.HasValue) combo.PrecoVenda = request.PrecoVenda.Value;
        if (request.Ativo.HasValue) combo.Ativo = request.Ativo.Value;

        if (request.Composicao != null)
        {
            combo.Composicao.Clear();
            combo.Composicao = request.Composicao.Select(c => new ComboComposicao
            {
                ComboId = id,
                ProdutoId = c.ProdutoId,
                Quantidade = c.Quantidade,
                Unidade = c.Unidade,
                DebitaEstoque = c.DebitaEstoque
            }).ToList();
        }

        await comboRepository.UpdateAsync(combo);
        return MapToDto(combo);
    }

    public async Task DeleteAsync(int id)
    {
        if (await comboRepository.FoiVendidoAsync(id))
            throw new InvalidOperationException("Não é possível excluir combo que já foi vendido.");

        await comboRepository.DeleteAsync(id);
    }

    public async Task<List<ComboVendaDto>> GetVendasAsync(int? comboId = null, int? mes = null, int? ano = null)
    {
        var vendas = await comboRepository.GetVendasAsync(comboId, mes, ano);
        return vendas.Select(MapVendaToDto).ToList();
    }

    public async Task<ComboVendaDto> CreateVendaAsync(CreateComboVendaRequest request, int usuarioId, string responsavel)
    {
        var combo = await comboRepository.GetByIdAsync(request.ComboId)
            ?? throw new KeyNotFoundException($"Combo {request.ComboId} não encontrado.");

        // Validate stock for items that debit inventory
        foreach (var item in combo.Composicao.Where(c => c.DebitaEstoque))
        {
            var quantidade = await produtoRepository.GetQuantidadeAsync(item.ProdutoId);
            var necessario = (int)Math.Ceiling(item.Quantidade * request.Quantidade);
            if (quantidade < necessario)
            {
                var produto = await produtoRepository.GetByIdAsync(item.ProdutoId);
                throw new InvalidOperationException(
                    $"Estoque insuficiente para {produto?.Descricao}. Disponível: {quantidade}, Necessário: {necessario}");
            }
        }

        var venda = new ComboVenda
        {
            ComboId = request.ComboId,
            UsuarioId = usuarioId,
            Quantidade = request.Quantidade,
            PrecoUnitario = request.PrecoUnitario,
            PrecoTotal = request.PrecoUnitario * request.Quantidade,
            Responsavel = responsavel,
            Observacoes = request.Observacoes,
            TipoMovimento = request.TipoMovimento
        };

        await comboRepository.CreateVendaAsync(venda);

        // Create Saída movements for items that debit inventory
        foreach (var item in combo.Composicao.Where(c => c.DebitaEstoque))
        {
            var produto = await produtoRepository.GetByIdAsync(item.ProdutoId);
            var movimentacao = new Movimentacao
            {
                Data = DateTime.UtcNow,
                Tipo = "Saída",
                TipoVenda = "Varejo",
                ProdutoId = item.ProdutoId,
                ProdutoDescricao = produto?.Descricao ?? "",
                Quantidade = (int)Math.Ceiling(item.Quantidade * request.Quantidade),
                UsuarioId = usuarioId,
                Responsavel = responsavel,
                TipoSaida = combo.EhCopao ? "Copao" : "Combo",
                ValorUnitario = 0
            };

            await movimentacaoRepository.CreateAsync(movimentacao);
        }

        venda.Combo = combo;
        return MapVendaToDto(venda);
    }

    private static ComboDto MapToDto(Combo c) =>
        new(
            c.Id,
            c.Nome,
            c.Descricao,
            c.PrecoVenda,
            c.Ativo,
            c.EhCopao,
            c.CriadoEm,
            c.Composicao.Select(cc => new ComboComposicaoDto(
                cc.Id,
                cc.ProdutoId,
                cc.Produto?.Descricao ?? "",
                cc.Quantidade,
                cc.Unidade,
                cc.DebitaEstoque
            )).ToList()
        );

    private static ComboVendaDto MapVendaToDto(ComboVenda cv) =>
        new(
            cv.Id,
            cv.ComboId,
            cv.Combo?.Nome ?? "",
            cv.UsuarioId,
            cv.Responsavel ?? "",
            cv.Quantidade,
            cv.PrecoUnitario,
            cv.PrecoTotal,
            cv.DataVenda,
            cv.Observacoes,
            cv.TipoMovimento
        );
}
