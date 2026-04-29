using AdegaOak.Models.DTOs;

namespace AdegaOak.Services.Interfaces;

public interface IComboService
{
    Task<List<ComboDto>> GetAllAsync(bool? ehCopao = null);
    Task<ComboDto?> GetByIdAsync(int id);
    Task<ComboDto> CreateAsync(CreateComboRequest request);
    Task<ComboDto> UpdateAsync(int id, UpdateComboRequest request);
    Task DeleteAsync(int id);
    Task<List<ComboVendaDto>> GetVendasAsync(int? comboId = null, int? mes = null, int? ano = null);
    Task<ComboVendaDto> CreateVendaAsync(CreateComboVendaRequest request, int usuarioId, string responsavel);
}
