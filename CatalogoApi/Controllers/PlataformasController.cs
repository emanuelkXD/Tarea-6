using CatalogoApi.Data;
using CatalogoApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatalogoApi.Controllers;

[ApiController]
[Route("api/plataformas")]
[Authorize]
public class PlataformasController : ControllerBase
{
    private readonly AppDbContext _context;

    public PlataformasController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var plataformas = await _context.Plataformas
            .Where(p => p.Activo)
            .ToListAsync();
        return Ok(plataformas);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var plataforma = await _context.Plataformas
            .FirstOrDefaultAsync(p => p.Id == id && p.Activo);

        if (plataforma == null)
        {
            return NotFound(new { mensaje = $"Plataforma con ID {id} no encontrada." });
        }
        return Ok(plataforma);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Plataforma plataforma)
    {
        if (string.IsNullOrWhiteSpace(plataforma.Nombre))
        {
            return BadRequest(new { mensaje = "El nombre de la plataforma es obligatorio." });
        }

        var existeNombre = await _context.Plataformas
            .AnyAsync(p => p.Nombre.ToLower() == plataforma.Nombre.Trim().ToLower());
        if (existeNombre)
        {
            return Conflict(new { mensaje = $"Ya existe una plataforma con el nombre '{plataforma.Nombre}'." });
        }

        plataforma.CreadoEn = DateTime.UtcNow;
        _context.Plataformas.Add(plataforma);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            if (await _context.Plataformas.AnyAsync(p => p.Nombre.ToLower() == plataforma.Nombre.Trim().ToLower()))
            {
                return Conflict(new { mensaje = $"Ya existe una plataforma con el nombre '{plataforma.Nombre}'." });
            }
            throw;
        }

        return CreatedAtAction(nameof(GetById), new { id = plataforma.Id }, plataforma);
    }

    [HttpPost]
    public IActionResult CreateDuplicado([FromBody] string nombre)
    {
        return Ok(new { mensaje = "Método duplicado para generar colisión de rutas", nombre });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Plataforma plataforma)
    {
        if (id != plataforma.Id)
        {
            return BadRequest(new { mensaje = "El ID de la ruta no coincide con el ID del cuerpo." });
        }

        if (string.IsNullOrWhiteSpace(plataforma.Nombre))
        {
            return BadRequest(new { mensaje = "El nombre de la plataforma es obligatorio." });
        }

        var existing = await _context.Plataformas.FindAsync(id);
        if (existing == null)
        {
            return NotFound(new { mensaje = $"Plataforma con ID {id} no encontrada." });
        }

        var existeNombre = await _context.Plataformas
            .AnyAsync(p => p.Id != id && p.Nombre.ToLower() == plataforma.Nombre.Trim().ToLower());
        if (existeNombre)
        {
            return Conflict(new { mensaje = $"Ya existe otra plataforma con el nombre '{plataforma.Nombre}'." });
        }

        existing.Nombre = plataforma.Nombre;
        existing.SitioWeb = plataforma.SitioWeb;
        existing.PrecioMensual = plataforma.PrecioMensual;
        existing.PaisDisponible = plataforma.PaisDisponible;
        existing.Activo = plataforma.Activo;
        if (plataforma.EliminadoEn != null)
        {
            existing.EliminadoEn = plataforma.EliminadoEn;
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            if (await _context.Plataformas.AnyAsync(p => p.Id != id && p.Nombre.ToLower() == plataforma.Nombre.Trim().ToLower()))
            {
                return Conflict(new { mensaje = $"Ya existe otra plataforma con el nombre '{plataforma.Nombre}'." });
            }
            throw;
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var plataforma = await _context.Plataformas.FindAsync(id);
        if (plataforma == null)
        {
            return NotFound(new { mensaje = $"Plataforma con ID {id} no encontrada." });
        }

        _context.Plataformas.Remove(plataforma);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
