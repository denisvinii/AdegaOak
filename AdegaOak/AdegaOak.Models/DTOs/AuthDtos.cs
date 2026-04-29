namespace AdegaOak.Models.DTOs;

public record LoginRequest(string Username, string Password);

public record LoginResponse(string Token, string Username, string Nome, string Role);

public record CreateUsuarioRequest(
    string Nome,
    string Username,
    string Password,
    string Role = "funcionario"
);

public record UpdateUsuarioRequest(
    string? Nome,
    string? Password,
    string? Role,
    bool? Ativo
);

public record UsuarioDto(int Id, string Nome, string Username, string Role, bool Ativo, DateTime CriadoEm);
