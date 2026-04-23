using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PortalAcademico.Data;
using PortalAcademico.Models;

var builder = WebApplication.CreateBuilder(args);

// DB
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string not found");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Identity
builder.Services.AddDefaultIdentity<IdentityUser>(options => 
    options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();

var app = builder.Build(); // ⚠️ AQUÍ se crea app

// 🔥 SEED (DESPUÉS de builder.Build)
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    if (!context.Cursos.Any())
    {
        context.Cursos.AddRange(
            new Curso
            {
                Codigo = "CS101",
                Nombre = "Programación",
                Creditos = 4,
                CupoMaximo = 30,
                HorarioInicio = DateTime.Now,
                HorarioFin = DateTime.Now.AddHours(2),
                Activo = true
            },
            new Curso
            {
                Codigo = "MAT202",
                Nombre = "Matemática",
                Creditos = 3,
                CupoMaximo = 25,
                HorarioInicio = DateTime.Now.AddHours(3),
                HorarioFin = DateTime.Now.AddHours(5),
                Activo = true
            },
            new Curso
            {
                Codigo = "HIS303",
                Nombre = "Historia",
                Creditos = 2,
                CupoMaximo = 20,
                HorarioInicio = DateTime.Now.AddHours(6),
                HorarioFin = DateTime.Now.AddHours(8),
                Activo = true
            }
        );

        context.SaveChanges();
    }
}

// pipeline
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Cursos}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();