using AdegaOak.Models.DTOs;

namespace AdegaOak.Services.Interfaces;

public interface IProdutoService
{
    Task<List<ProdutoDto>> GetAllAsync();
    Task<ProdutoDto?> GetByIdAsync(int id);
    Task<ProdutoDto> CreateAsync(CreateProdutoRequest request);
    Task<ProdutoDto> UpdatePrecosAsync(int id, UpdatePrecosRequest request);
    Task DeleteAsync(int id);
    Task<List<EstoqueProdutoDto>> GetEstoqueAsync();
}
