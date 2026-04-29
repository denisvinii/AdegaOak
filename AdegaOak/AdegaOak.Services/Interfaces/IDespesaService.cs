using AdegaOak.Models.DTOs;

namespace AdegaOak.Services.Interfaces;

public interface IDespesaService
{
    Task<List<DespesaDto>> GetAllAsync();
    Task<DespesaDto?> GetByIdAsync(int id);
    Task<DespesaDto> CreateAsync(CreateDespesaRequest request, int usuarioId, string responsavel);
    Task<DespesaDto> UpdateAsync(int id, UpdateDespesaRequest request);
    Task DeleteAsync(int id);
    Task<List<DespesaDto>> GetByPeriodoAsync(int mes, int ano);
    Task MarcarPagaAsync(int id, bool pago);
    Task<DespesaResumoDto> GetResumoAsync(int? mes = null, int? ano = null);
}
