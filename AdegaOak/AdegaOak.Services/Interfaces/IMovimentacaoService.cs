using AdegaOak.Models.DTOs;

namespace AdegaOak.Services.Interfaces;

public interface IMovimentacaoService
{
    Task<List<MovimentacaoDto>> GetAllAsync();
    Task<PagedResult<MovimentacaoDto>> GetPagedAsync(int page, int pageSize);
    Task<MovimentacaoDto?> GetByIdAsync(int id);
    Task<MovimentacaoDto> CreateAsync(CreateMovimentacaoRequest request, int usuarioId, string responsavel);
    Task DeleteAsync(int id);
    Task<List<MovimentacaoDto>> GetByFiltrosAsync(MovimentacaoFiltroRequest filtros);
    Task<MovimentacaoResumoDto> GetResumoAsync(MovimentacaoFiltroRequest filtros);
}
