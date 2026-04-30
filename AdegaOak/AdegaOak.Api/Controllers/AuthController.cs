using AdegaOak.Models.DTOs;
using AdegaOak.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdegaOak.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            var response = await authService.LoginAsync(request);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            // Log the full error for debugging
            Console.WriteLine($"[LOGIN ERROR] {ex.GetType().Name}: {ex.Message}");
            Console.WriteLine($"[LOGIN ERROR] Stack: {ex.StackTrace}");
            return StatusCode(500, new { message = ex.Message, type = ex.GetType().Name });
        }
    }

    [Authorize(Roles = "admin")]
    [HttpPost("usuarios")]
    public async Task<ActionResult<UsuarioDto>> CreateUsuario([FromBody] CreateUsuarioRequest request)
    {
        try
        {
            var usuario = await authService.CreateUsuarioAsync(request);
            return CreatedAtAction(nameof(GetUsuario), new { id = usuario.Id }, usuario);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "admin")]
    [HttpGet("usuarios")]
    public async Task<ActionResult<List<UsuarioDto>>> GetAllUsuarios()
    {
        var usuarios = await authService.GetAllUsuariosAsync();
        return Ok(usuarios);
    }

    [Authorize]
    [HttpGet("usuarios/{id}")]
    public async Task<ActionResult<UsuarioDto>> GetUsuario(int id)
    {
        var usuario = await authService.GetUsuarioByIdAsync(id);
        return usuario == null ? NotFound() : Ok(usuario);
    }

    [Authorize(Roles = "admin")]
    [HttpPut("usuarios/{id}")]
    public async Task<ActionResult<UsuarioDto>> UpdateUsuario(int id, [FromBody] UpdateUsuarioRequest request)
    {
        try
        {
            var usuario = await authService.UpdateUsuarioAsync(id, request);
            return Ok(usuario);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "admin")]
    [HttpDelete("usuarios/{id}")]
    public async Task<IActionResult> DeleteUsuario(int id)
    {
        try
        {
            await authService.DeleteUsuarioAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
