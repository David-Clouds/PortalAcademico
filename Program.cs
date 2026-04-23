using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PortalAcademico.Data;
using PortalAcademico.Models;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string not found");


builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));


builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddRoles<IdentityRole>() 
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();

var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<ApplicationDbContext>();
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    // Rol
    if (!await roleManager.RoleExistsAsync("Coordinador"))
    {
        await roleManager.CreateAsync(new IdentityRole("Coordinador"));
    }


    var email = "coordinador@demo.com";
    var user = await userManager.FindByEmailAsync(email);

    if (user == null)
    {
        user = new IdentityUser
        {
            UserName = email,
            Email = email
        };

        await userManager.CreateAsync(user, "Admin123!");
        await userManager.AddToRoleAsync(user, "Coordinador");
    }

    if (!context.Cursos.Any())
    {
        context.Cursos.AddRange(
            new Curso { Codigo = "CS101", Nombre = "Programación", Creditos = 4, CupoMaximo = 30, HorarioInicio = DateTime.Now, HorarioFin = DateTime.Now.AddHours(2), Activo = true },
            new Curso { Codigo = "MAT202", Nombre = "Matemática", Creditos = 3, CupoMaximo = 25, HorarioInicio = DateTime.Now.AddHours(3), HorarioFin = DateTime.Now.AddHours(5), Activo = true },
            new Curso { Codigo = "HIS303", Nombre = "Historia", Creditos = 2, CupoMaximo = 20, HorarioInicio = DateTime.Now.AddHours(6), HorarioFin = DateTime.Now.AddHours(8), Activo = true }
        );

        await context.SaveChangesAsync();
    }
}


if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();