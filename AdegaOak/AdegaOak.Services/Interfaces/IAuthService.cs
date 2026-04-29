using AdegaOak.Models.DTOs;

namespace AdegaOak.Services.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<UsuarioDto> CreateUsuarioAsync(CreateUsuarioRequest request);
    Task<UsuarioDto> UpdateUsuarioAsync(int id, UpdateUsuarioRequest request);
    Task<List<UsuarioDto>> GetAllUsuariosAsync();
    Task<UsuarioDto?> GetUsuarioByIdAsync(int id);
    Task DeleteUsuarioAsync(int id);
}
