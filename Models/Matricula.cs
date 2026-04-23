using System.ComponentModel.DataAnnotations;

namespace PortalAcademico.Models
{
    public class Matricula
    {
        public int Id { get; set; }

        public int CursoId { get; set; }
        public Curso Curso { get; set; } = null!;

        public string UsuarioId { get; set; } = "";

        public DateTime FechaRegistro { get; set; }

        public string Estado { get; set; } = "";

    }
}