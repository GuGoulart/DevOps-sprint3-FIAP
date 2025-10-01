using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MottuProjeto.Data;
using MottuProjeto.Models;
using MottuProjeto.Infrastructure;

namespace MottuProjeto.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MotosController : ControllerBase
    {
        private readonly AppDbContext _context;
        public MotosController(AppDbContext context) => _context = context;

        /// <summary>Lista paginada de motos (com links de navegação).</summary>
        /// <param name="page">Página (&gt;=1).</param>
        /// <param name="pageSize">Itens por página (1–100).</param>
        /// <response code="200">Retorna a página solicitada.</response>
        [HttpGet]
        public async Task<ActionResult<object>> Listar([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var query = _context.Motos.AsNoTracking().OrderBy(m => m.Id);
            var total = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var self = Url.Action(nameof(Listar), values: new { page, pageSize });
            var next = (page * pageSize < total) ? Url.Action(nameof(Listar), values: new { page = page + 1, pageSize }) : null;
            var prev = (page > 1) ? Url.Action(nameof(Listar), values: new { page = page - 1, pageSize }) : null;

            return Ok(new { total, page, pageSize, _links = new { self, next, prev }, items });
        }

        /// <summary>Obtém uma moto (HATEOAS: self, update, delete).</summary>
        /// <param name="id">Identificador da moto.</param>
        /// <response code="200">Moto encontrada.</response>
        /// <response code="404">Moto não encontrada.</response>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<object>> Obter(int id)
        {
            var moto = await _context.Motos.FindAsync(id);
            if (moto is null) return NotFound();

            return Ok(this.WithLinks(moto, nameof(Obter), nameof(Atualizar), nameof(Delete), new { id }));
        }

        /// <summary>Cria uma moto (HATEOAS: self, update, delete).</summary>
        /// <param name="moto">Dados da moto.</param>
        /// <remarks>
        /// Exemplo de payload:
        /// {
        ///   "placa": "ABC1D23",
        ///   "modelo": "CG 160",
        ///   "idArea": 1
        /// }
        /// </remarks>
        /// <response code="201">Moto criada com sucesso.</response>
        /// <response code="400">Dados inválidos.</response>
        [HttpPost]
        public async Task<ActionResult<object>> Criar([FromBody] Moto moto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            _context.Motos.Add(moto);
            await _context.SaveChangesAsync();

            var envelope = this.WithLinks(moto, nameof(Obter), nameof(Atualizar), nameof(Delete), new { id = moto.Id });
            return CreatedAtAction(nameof(Obter), new { id = moto.Id }, envelope);
        }

        /// <summary>Atualiza uma moto (HATEOAS: self, delete).</summary>
        /// <param name="id">Identificador da moto.</param>
        /// <param name="moto">Dados atualizados da moto.</param>
        /// <remarks>
        /// Exemplo de payload:
        /// {
        ///   "id": 1,
        ///   "placa": "ABC1D23",
        ///   "modelo": "CG 160 Fan",
        ///   "idArea": 2
        /// }
        /// </remarks>
        /// <response code="200">Moto atualizada (envelope HATEOAS).</response>
        /// <response code="400">Ids divergentes ou dados inválidos.</response>
        /// <response code="404">Moto não encontrada.</response>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Atualizar(int id, [FromBody] Moto moto)
        {
            if (id != moto.Id) return BadRequest("Id do recurso diverge do corpo.");
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var existe = await _context.Motos.AnyAsync(m => m.Id == id);
            if (!existe) return NotFound();

            _context.Entry(moto).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            var envelope = this.WithLinks(moto, nameof(Obter), nameof(Atualizar), nameof(Delete), new { id });
            return Ok(envelope);
        }

        /// <summary>Exclui uma moto (HATEOAS: link para coleção e criação).</summary>
        /// <param name="id">Identificador da moto.</param>
        /// <response code="200">Confirma exclusão com links para coleção/criação.</response>
        /// <response code="404">Moto não encontrada.</response>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var moto = await _context.Motos.FindAsync(id);
            if (moto is null) return NotFound();

            _context.Motos.Remove(moto);
            await _context.SaveChangesAsync();

            var collection = Url.Action(nameof(Listar), new { page = 1, pageSize = 10 });
            var create = Url.Action(nameof(Criar));
            return Ok(new { message = "Excluída.", _links = new { collection, create } });
        }
    }
}
