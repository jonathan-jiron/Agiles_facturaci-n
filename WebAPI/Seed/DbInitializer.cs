using Infrastructure.Data;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace WebAPI.Seed;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();

        // Admin por defecto (solo si no existe)
        if (!await db.Usuarios.AnyAsync(u => u.Username == "admin"))
        {
            db.Usuarios.Add(new Usuario
            {
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123", workFactor: 11),
                Rol = "ADMIN"
            });
            await db.SaveChangesAsync();
        }
    }
}