using AdegaOak.Data.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdegaOak.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UsuariosController(AdegaOakDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult> GetAll()
    {
        var usuarios = await db.Usuarios
            .AsNoTracking()
            .Where(u => u.Ativo)
            .Select(u => new
            {
                u.Id,
                u.Nome,
                u.Username,
                u.Role
            })
            .ToListAsync();

        return Ok(usuarios);
    }
}
