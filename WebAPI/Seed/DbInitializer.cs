using Infrastructure.Data;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration; // <-- agregar

namespace WebAPI.Seed;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>(); // <-- obtener config
        var workFactor = config.GetValue<int?>("Security:BcryptWorkFactor") ?? 10; // default

        await db.Database.MigrateAsync();

        // Admin por defecto (solo si no existe)
        if (!await db.Usuarios.AnyAsync(u => u.Username == "admin"))
        {
            db.Usuarios.Add(new Usuario
            {
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123", workFactor),
                Rol = "ADMIN"
            });
            await db.SaveChangesAsync();
        }
    }
}