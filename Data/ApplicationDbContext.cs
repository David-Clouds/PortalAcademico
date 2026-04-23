using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PortalAcademico.Models;

namespace PortalAcademico.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }


    public DbSet<Curso> Cursos { get; set; }
    public DbSet<Matricula> Matriculas { get; set; }


    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Curso>()
            .HasIndex(c => c.Codigo)
            .IsUnique();

        builder.Entity<Curso>()
            .ToTable(t => t.HasCheckConstraint("CK_Curso_Creditos", "Creditos > 0"));

        builder.Entity<Curso>()
            .ToTable(t => t.HasCheckConstraint("CK_Curso_Horario", "HorarioInicio < HorarioFin"));

        builder.Entity<Matricula>()
            .HasIndex(m => new { m.CursoId, m.UsuarioId })
            .IsUnique();
    }
}