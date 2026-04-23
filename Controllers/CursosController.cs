using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalAcademico.Data;

namespace PortalAcademico.Controllers
{
    public class CursosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CursosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // LISTADO + FILTROS
        public async Task<IActionResult> Index(string nombre, int? minCreditos, int? maxCreditos)
        {
            var cursos = _context.Cursos.Where(c => c.Activo);

            if (!string.IsNullOrEmpty(nombre))
                cursos = cursos.Where(c => c.Nombre.Contains(nombre));

            if (minCreditos.HasValue)
                cursos = cursos.Where(c => c.Creditos >= minCreditos);

            if (maxCreditos.HasValue)
                cursos = cursos.Where(c => c.Creditos <= maxCreditos);

            return View(await cursos.ToListAsync());
        }

        // DETALLE
        public async Task<IActionResult> Detalle(int id)
        {
            var curso = await _context.Cursos.FirstOrDefaultAsync(c => c.Id == id);

            if (curso == null)
                return NotFound();

            return View(curso);
        }
    }
}