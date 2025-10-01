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
    public class AreaController : ControllerBase
    {
        private readonly AppDbContext _context;
        public AreaController(AppDbContext context) => _context = context;

        /// <summary>Lista paginada de áreas (com links de navegação).</summary>
        /// <param name="page">Página (&gt;=1).</param>
        /// <param name="pageSize">Itens por página (1–100).</param>
        /// <response code="200">Retorna a página solicitada.</response>
        [HttpGet]
        public async Task<ActionResult<object>> Listar([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var query = _context.Areas.AsNoTracking().OrderBy(a => a.Id);
            var total = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var self = Url.Action(nameof(Listar), values: new { page, pageSize });
            var next = (page * pageSize < total) ? Url.Action(nameof(Listar), values: new { page = page + 1, pageSize }) : null;
            var prev = (page > 1) ? Url.Action(nameof(Listar), values: new { page = page - 1, pageSize }) : null;

            return Ok(new { total, page, pageSize, _links = new { self, next, prev }, items });
        }

        /// <summary>Obtém uma área (HATEOAS: self, update, delete).</summary>
        /// <param name="id">Identificador da área.</param>
        /// <response code="200">Área encontrada.</response>
        /// <response code="404">Área não encontrada.</response>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<object>> Obter(int id)
        {
            var area = await _context.Areas.FindAsync(id);
            if (area is null) return NotFound();

            return Ok(this.WithLinks(area, nameof(Obter), nameof(Atualizar), nameof(Delete), new { id }));
        }

        /// <summary>Cria uma área (HATEOAS: self, update, delete).</summary>
        /// <param name="area">Dados da área.</param>
        /// <remarks>
        /// Exemplo de payload:
        /// {
        ///   "nome": "Zona Leste"
        /// }
        /// </remarks>
        /// <response code="201">Área criada com sucesso.</response>
        /// <response code="400">Dados inválidos.</response>
        [HttpPost]
        public async Task<ActionResult<object>> Criar([FromBody] Area area)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            _context.Areas.Add(area);
            await _context.SaveChangesAsync();

            var envelope = this.WithLinks(area, nameof(Obter), nameof(Atualizar), nameof(Delete), new { id = area.Id });
            return CreatedAtAction(nameof(Obter), new { id = area.Id }, envelope);
        }

        /// <summary>Atualiza uma área (HATEOAS: self, delete).</summary>
        /// <param name="id">Identificador da área.</param>
        /// <param name="area">Dados atualizados da área.</param>
        /// <remarks>
        /// Exemplo de payload:
        /// {
        ///   "id": 1,
        ///   "nome": "Centro"
        /// }
        /// </remarks>
        /// <response code="200">Área atualizada (envelope HATEOAS).</response>
        /// <response code="400">Ids divergentes ou dados inválidos.</response>
        /// <response code="404">Área não encontrada.</response>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Atualizar(int id, [FromBody] Area area)
        {
            if (id != area.Id) return BadRequest("Id do recurso diverge do corpo.");
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var existe = await _context.Areas.AnyAsync(a => a.Id == id);
            if (!existe) return NotFound();

            _context.Entry(area).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            var envelope = this.WithLinks(area, nameof(Obter), nameof(Atualizar), nameof(Delete), new { id });
            return Ok(envelope);
        }

        /// <summary>Exclui uma área (HATEOAS: link para coleção e criação).</summary>
        /// <param name="id">Identificador da área.</param>
        /// <response code="200">Confirma exclusão com links para coleção/criação.</response>
        /// <response code="404">Área não encontrada.</response>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var area = await _context.Areas.FindAsync(id);
            if (area is null) return NotFound();

            _context.Areas.Remove(area);
            await _context.SaveChangesAsync();

            var collection = Url.Action(nameof(Listar), new { page = 1, pageSize = 10 });
            var create = Url.Action(nameof(Criar));
            return Ok(new { message = "Excluída.", _links = new { collection, create } });
        }
    }
}
