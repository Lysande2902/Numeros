using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParImparAPI.Domain.Data;
using ParImparAPI.Domain.Entities;

namespace ParImparAPI.Controllers.V1
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize] // Requerir autenticación para todos los endpoints
    public class NumeroController : ControllerBase
    {
        private readonly AppDbContext _context;

        public NumeroController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/v1/Numero
        [HttpGet]
        public async Task<ActionResult<PaginatedResponse<Numero>>> Get(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10
        )
        {
            // Validar parámetros
            if (page < 1)
                page = 1;
            if (pageSize < 1 || pageSize > 100)
                pageSize = 10;

            // Obtener el total de registros
            var totalRecords = await _context.Numeros.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            // Calcular el offset
            var offset = (page - 1) * pageSize;

            // Obtener los registros de la página actual
            var numeros = await _context.Numeros.Skip(offset).Take(pageSize).ToListAsync();

            // Crear la respuesta paginada
            var response = new PaginatedResponse<Numero>
            {
                Data = numeros,
                Pagination = new PaginationInfo
                {
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalRecords = totalRecords,
                    TotalPages = totalPages,
                    HasNextPage = page < totalPages,
                    HasPreviousPage = page > 1,
                    NextPage = page < totalPages ? page + 1 : null,
                    PreviousPage = page > 1 ? page - 1 : null,
                },
            };

            return Ok(response);
        }

        // GET: api/v1/Numero/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Numero>> Get(int id)
        {
            var numero = await _context.Numeros.FindAsync(id);

            if (numero == null)
                return NotFound();

            return numero;
        }

        // POST: api/v1/Numero
        [HttpPost]
        public async Task<ActionResult<Numero>> Post(Numero numero)
        {
            // Validar que el número no sea negativo
            if (numero.Value < 0)
            {
                return BadRequest(new { message = "No se permiten números negativos." });
            }

            // Calcular la paridad automáticamente
            numero.Paridad = (numero.Value % 2 == 0) ? "Par" : "Impar";

            // Agregar el número a la base de datos
            _context.Numeros.Add(numero);
            await _context.SaveChangesAsync();

            // Devolver la respuesta con el id generado
            return CreatedAtAction(nameof(Get), new { id = numero.Id }, numero);
        }

        // PUT: api/v1/Numero/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, Numero numero)
        {
            if (id != numero.Id)
                return BadRequest();

            // Validar que el número no sea negativo
            if (numero.Value < 0)
            {
                return BadRequest(new { message = "No se permiten números negativos." });
            }

            // Calcular la paridad nuevamente
            numero.Paridad = (numero.Value % 2 == 0) ? "Par" : "Impar";

            _context.Entry(numero).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Numeros.Any(n => n.Id == id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // DELETE: api/v1/Numero/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var numero = await _context.Numeros.FindAsync(id);

            if (numero == null)
                return NotFound();

            _context.Numeros.Remove(numero);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

    // Clases para la paginación
    public class PaginatedResponse<T>
    {
        public List<T> Data { get; set; } = new List<T>();
        public PaginationInfo Pagination { get; set; } = new PaginationInfo();
    }

    public class PaginationInfo
    {
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
        public int? NextPage { get; set; }
        public int? PreviousPage { get; set; }
    }
}
