using AdegaOak.Models.DTOs;

namespace AdegaOak.Services.Interfaces;

public interface IVendaService
{
    Task<VendaDto> CreateAsync(CreateVendaRequest request, int usuarioId, string responsavel);
    Task<List<VendaDto>> GetAllAsync();
    Task<VendaDto?> GetByIdAsync(int id);
    Task<VendaResumoDto> GetResumoAsync(DateTime? dataInicio, DateTime? dataFim);
}
