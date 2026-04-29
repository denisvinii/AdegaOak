using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AdegaOak.Data.Repositories;
using AdegaOak.Models.DTOs;
using AdegaOak.Models.Models;
using AdegaOak.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AdegaOak.Services.Services;

public class AuthService(
    IUsuarioRepository usuarioRepository,
    IConfiguration configuration) : IAuthService
{
    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var usuario = await usuarioRepository.GetByUsernameAsync(request.Username)
            ?? throw new UnauthorizedAccessException("Credenciais inválidas.");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, usuario.PasswordHash))
            throw new UnauthorizedAccessException("Credenciais inválidas.");

        var token = GenerateJwtToken(usuario);
        return new LoginResponse(token, usuario.Username, usuario.Nome, usuario.Role);
    }

    public async Task<UsuarioDto> CreateUsuarioAsync(CreateUsuarioRequest request)
    {
        if (await usuarioRepository.UsernameExistsAsync(request.Username))
            throw new InvalidOperationException($"Username '{request.Username}' já existe.");

        var usuario = new Usuario
        {
            Nome = request.Nome,
            Username = request.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = request.Role,
            Ativo = true
        };

        await usuarioRepository.CreateAsync(usuario);
        return MapToDto(usuario);
    }

    public async Task<UsuarioDto> UpdateUsuarioAsync(int id, UpdateUsuarioRequest request)
    {
        var usuario = await usuarioRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Usuário {id} não encontrado.");

        if (request.Nome != null) usuario.Nome = request.Nome;
        if (request.Password != null) usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        if (request.Role != null) usuario.Role = request.Role;
        if (request.Ativo.HasValue) usuario.Ativo = request.Ativo.Value;

        await usuarioRepository.UpdateAsync(usuario);
        return MapToDto(usuario);
    }

    public async Task<List<UsuarioDto>> GetAllUsuariosAsync()
    {
        var usuarios = await usuarioRepository.GetAllAsync();
        return usuarios.Select(MapToDto).ToList();
    }

    public async Task<UsuarioDto?> GetUsuarioByIdAsync(int id)
    {
        var usuario = await usuarioRepository.GetByIdAsync(id);
        return usuario == null ? null : MapToDto(usuario);
    }

    public async Task DeleteUsuarioAsync(int id) =>
        await usuarioRepository.DeleteAsync(id);

    private string GenerateJwtToken(Usuario usuario)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key não configurada.")));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, usuario.Username),
            new Claim(ClaimTypes.Role, usuario.Role),
            new Claim("nome", usuario.Nome)
        };

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static UsuarioDto MapToDto(Usuario u) =>
        new(u.Id, u.Nome, u.Username, u.Role, u.Ativo, u.CriadoEm);
}
