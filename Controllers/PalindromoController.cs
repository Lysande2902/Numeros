using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ParImparAPI.Controllers.V1
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class PalindromoController : ControllerBase
    {
        // Simulación de almacenamiento en memoria
        private static List<PalindromoItem> _palindromos = new List<PalindromoItem>();
        private static int _nextId = 1;

        // GET: api/v1/Palindromo?page=1&pageSize=10
        [HttpGet]
        public ActionResult<object> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var totalRecords = _palindromos.Count;
            var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
            var offset = (page - 1) * pageSize;
            var data = _palindromos.Skip(offset).Take(pageSize).ToList();

            return Ok(new
            {
                Data = data,
                Pagination = new
                {
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalRecords = totalRecords,
                    TotalPages = totalPages,
                    HasNextPage = page < totalPages,
                    HasPreviousPage = page > 1,
                    NextPage = page < totalPages ? page + 1 : (int?)null,
                    PreviousPage = page > 1 ? page - 1 : (int?)null
                }
            });
        }

        // GET: api/v1/Palindromo/5
        [HttpGet("{id}")]
        public ActionResult<PalindromoItem> Get(int id)
        {
            var item = _palindromos.FirstOrDefault(x => x.Id == id);
            if (item == null)
                return NotFound();
            return Ok(item);
        }

        // POST: api/v1/Palindromo
        [HttpPost]
        public ActionResult<PalindromoItem> Post(PalindromoRequest request)
        {
            var validacion = ValidarTexto(request);
            if (validacion != null)
                return BadRequest(validacion);

            var texto = request.Texto.Trim();
            var normalizado = texto.ToLowerInvariant();
            var esPalindromo = normalizado.SequenceEqual(normalizado.Reverse());

            var item = new PalindromoItem
            {
                Id = _nextId++,
                Texto = texto,
                EsPalindromo = esPalindromo
            };
            _palindromos.Add(item);
            return CreatedAtAction(nameof(Get), new { id = item.Id }, item);
        }

        // PUT: api/v1/Palindromo/5
        [HttpPut("{id}")]
        public ActionResult Put(int id, PalindromoRequest request)
        {
            var item = _palindromos.FirstOrDefault(x => x.Id == id);
            if (item == null)
                return NotFound();

            var validacion = ValidarTexto(request);
            if (validacion != null)
                return BadRequest(validacion);

            var texto = request.Texto.Trim();
            var normalizado = texto.ToLowerInvariant();
            var esPalindromo = normalizado.SequenceEqual(normalizado.Reverse());

            item.Texto = texto;
            item.EsPalindromo = esPalindromo;
            return NoContent();
        }

        // DELETE: api/v1/Palindromo/5
        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var item = _palindromos.FirstOrDefault(x => x.Id == id);
            if (item == null)
                return NotFound();
            _palindromos.Remove(item);
            return NoContent();
        }

        // Validación reutilizable
        private object? ValidarTexto(PalindromoRequest? request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Texto))
                return new { message = "El texto es requerido." };
            var texto = request.Texto.Trim();
            if (texto.Contains(' '))
                return new { message = "Solo se permite una palabra." };
            if (!Regex.IsMatch(texto, "^[a-zA-ZáéíóúÁÉÍÓÚñÑ]+$"))
                return new { message = "Solo se permiten letras (sin números ni símbolos)." };
            return null;
        }
    }

    public class PalindromoRequest
    {
        public string Texto { get; set; } = string.Empty;
    }

    public class PalindromoItem
    {
        public int Id { get; set; }
        public string Texto { get; set; } = string.Empty;
        public bool EsPalindromo { get; set; }
    }
}
