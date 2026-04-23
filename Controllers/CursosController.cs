using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalAcademico.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using PortalAcademico.Models;

namespace PortalAcademico.Controllers
{
    public class CursosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public CursosController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


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


        public async Task<IActionResult> Detalle(int id)
        {
            var curso = await _context.Cursos.FirstOrDefaultAsync(c => c.Id == id);

            if (curso == null)
                return NotFound();

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
                UsuarioId = userId,
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