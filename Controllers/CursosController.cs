using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalAcademico.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using PortalAcademico.Models;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace PortalAcademico.Controllers
{
    public class CursosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IDistributedCache _cache;

        public CursosController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            IDistributedCache cache)
        {
            _context = context;
            _userManager = userManager;
            _cache = cache;
        }

        public async Task<IActionResult> Index(string nombre, int? minCreditos, int? maxCreditos)
        {
            var cacheKey = "cursos_activos";

            string? cacheData = null;

            try
            {
                cacheData = await _cache.GetStringAsync(cacheKey);
            }
            catch
            {
                cacheData = null;
            }

            List<Curso> cursos;

            if (cacheData != null)
            {
                cursos = JsonSerializer.Deserialize<List<Curso>>(cacheData)!;
            }
            else
            {
                var query = _context.Cursos.Where(c => c.Activo);

                if (!string.IsNullOrEmpty(nombre))
                    query = query.Where(c => c.Nombre.Contains(nombre));

                if (minCreditos.HasValue)
                    query = query.Where(c => c.Creditos >= minCreditos);

                if (maxCreditos.HasValue)
                    query = query.Where(c => c.Creditos <= maxCreditos);

                cursos = await query.ToListAsync();

                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60)
                };

                try
                {
                    await _cache.SetStringAsync(
                        cacheKey,
                        JsonSerializer.Serialize(cursos),
                        options);
                }
                catch
                {
                    // ignora error si Redis falla
                }
            }

            return View(cursos);
        }

        public async Task<IActionResult> Detalle(int id)
        {
            var curso = await _context.Cursos.FirstOrDefaultAsync(c => c.Id == id);

            if (curso == null)
                return NotFound();

            HttpContext.Session.SetString("UltimoCurso", curso.Nombre);
            HttpContext.Session.SetInt32("UltimoCursoId", curso.Id);

            return View(curso);
        }

        [Authorize]
        public async Task<IActionResult> Inscribirse(int id)
        {
            var curso = await _context.Cursos.FindAsync(id);

            if (curso == null)
                return NotFound();

            var userId = _userManager.GetUserId(User);

            var existe = _context.Matriculas
                .Any(m => m.CursoId == id && m.UsuarioId == userId);

            if (existe)
            {
                TempData["Error"] = "Ya estás inscrito en este curso";
                return RedirectToAction("Detalle", new { id });
            }

            var total = _context.Matriculas.Count(m => m.CursoId == id);

            if (total >= curso.CupoMaximo)
            {
                TempData["Error"] = "Cupo lleno";
                return RedirectToAction("Detalle", new { id });
            }

            var conflictos = _context.Matriculas
                .Include(m => m.Curso)
                .Where(m => m.UsuarioId == userId)
                .Any(m =>
                    m.Curso.HorarioInicio < curso.HorarioFin &&
                    m.Curso.HorarioFin > curso.HorarioInicio
                );

            if (conflictos)
            {
                TempData["Error"] = "Cruce de horario";
                return RedirectToAction("Detalle", new { id });
            }

            var matricula = new Matricula
            {
                CursoId = id,
                UsuarioId = userId!,
                FechaRegistro = DateTime.Now,
                Estado = "Pendiente"
            };

            _context.Matriculas.Add(matricula);
            await _context.SaveChangesAsync();

            TempData["Ok"] = "Inscripción exitosa";
            return RedirectToAction("Detalle", new { id });
        }
    }
}