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
    [Authorize] // <-- exige usuário logado para todos os endpoints deste controller
    public class UsuariosController : ControllerBase
    {
        private readonly AppDbContext _context;
        public UsuariosController(AppDbContext context) => _context = context;

        /// <summary>Lista paginada de usuários (com links de navegação).</summary>
        /// <param name="page">Página (&gt;=1).</param>
        /// <param name="pageSize">Itens por página (1–100).</param>
        /// <response code="200">Retorna a página solicitada.</response>
        [HttpGet]
        public async Task<ActionResult<object>> Listar([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var query = _context.Usuarios.AsNoTracking().OrderBy(u => u.Id);
            var total = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var self = Url.Action(nameof(Listar), values: new { page, pageSize });
            var next = (page * pageSize < total) ? Url.Action(nameof(Listar), values: new { page = page + 1, pageSize }) : null;
            var prev = (page > 1) ? Url.Action(nameof(Listar), values: new { page = page - 1, pageSize }) : null;

            return Ok(new { total, page, pageSize, _links = new { self, next, prev }, items });
        }

        /// <summary>Obtém um usuário (HATEOAS: self, update, delete).</summary>
        /// <param name="id">Identificador do usuário.</param>
        /// <response code="200">Usuário encontrado.</response>
        /// <response code="404">Usuário não encontrado.</response>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<object>> Obter(int id)
        {
            var u = await _context.Usuarios.FindAsync(id);
            if (u is null) return NotFound();

            return Ok(this.WithLinks(u, nameof(Obter), nameof(Atualizar), nameof(Delete), new { id }));
        }

        /// <summary>Cria um usuário (HATEOAS: self, update, delete).</summary>
        /// <param name="usuario">Dados do usuário.</param>
        /// <remarks>
        /// Exemplo de payload:
        /// {
        ///   "nome": "João Silva",
        ///   "email": "joao@exemplo.com",
        ///   "username": "joaosilva",
        ///   "passwordHash": "senha123",
        ///   "role": "User"
        /// }
        /// </remarks>
        /// <response code="201">Usuário criado.</response>
        /// <response code="400">Dados inválidos.</response>
        [HttpPost]
        public async Task<ActionResult<object>> Criar([FromBody] Usuario usuario)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            var envelope = this.WithLinks(usuario, nameof(Obter), nameof(Atualizar), nameof(Delete), new { id = usuario.Id });
            return CreatedAtAction(nameof(Obter), new { id = usuario.Id }, envelope);
        }

        /// <summary>Atualiza um usuário (HATEOAS: self, delete).</summary>
        /// <param name="id">Identificador do usuário (rota).</param>
        /// <param name="usuario">Dados atualizados do usuário.</param>
        /// <remarks>
        /// Exemplo de payload:
        /// {
        ///   "id": 1,
        ///   "nome": "João A. Silva",
        ///   "email": "joao@exemplo.com",
        ///   "username": "joaosilva",
        ///   "passwordHash": "novaSenha",
        ///   "role": "Admin"
        /// }
        /// </remarks>
        /// <response code="200">Usuário atualizado (envelope HATEOAS).</response>
        /// <response code="400">Ids divergentes ou dados inválidos.</response>
        /// <response code="404">Usuário não encontrado.</response>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Atualizar(int id, [FromBody] Usuario usuario)
        {
            if (id != usuario.Id) return BadRequest("Id do recurso diverge do corpo.");
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var existe = await _context.Usuarios.AnyAsync(x => x.Id == id);
            if (!existe) return NotFound();

            _context.Entry(usuario).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            var envelope = this.WithLinks(usuario, nameof(Obter), nameof(Atualizar), nameof(Delete), new { id });
            return Ok(envelope);
        }

        /// <summary>Exclui um usuário (HATEOAS: link para coleção e criação).</summary>
        /// <param name="id">Identificador do usuário.</param>
        /// <response code="200">Confirma exclusão com links para coleção/criação.</response>
        /// <response code="404">Usuário não encontrado.</response>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var u = await _context.Usuarios.FindAsync(id);
            if (u is null) return NotFound();

            _context.Usuarios.Remove(u);
            await _context.SaveChangesAsync();

            var collection = Url.Action(nameof(Listar), new { page = 1, pageSize = 10 });
            var create = Url.Action(nameof(Criar));
            return Ok(new { message = "Excluído.", _links = new { collection, create } });
        }
    }
}
