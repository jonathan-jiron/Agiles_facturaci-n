using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Lote> Lotes { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurar precisión decimal para PrecioUnitario en Lote
            modelBuilder.Entity<Lote>()
                .Property(l => l.PrecioUnitario)
                .HasPrecision(18, 2);

            // Configurar relación Producto -> Lotes
            modelBuilder.Entity<Producto>()
                .HasMany(p => p.Lotes)
                .WithOne(l => l.Producto)
                .HasForeignKey(l => l.ProductoId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed Data - Productos
            modelBuilder.Entity<Producto>().HasData(
                new Producto { Id = 1, Codigo = "PROD-001", Nombre = "Laptop HP Pavilion", Descripcion = "Laptop 15 pulgadas, 8GB RAM, 256GB SSD" },
                new Producto { Id = 2, Codigo = "PROD-002", Nombre = "Mouse Logitech", Descripcion = "Mouse inalámbrico ergonómico" },
                new Producto { Id = 3, Codigo = "PROD-003", Nombre = "Teclado Mecánico", Descripcion = "Teclado RGB retroiluminado" },
                new Producto { Id = 4, Codigo = "PROD-004", Nombre = "Monitor LG 24\"", Descripcion = "Monitor Full HD IPS" },
                new Producto { Id = 5, Codigo = "PROD-005", Nombre = "Webcam Logitech C920", Descripcion = "Webcam Full HD 1080p" },
                new Producto { Id = 6, Codigo = "PROD-006", Nombre = "Auriculares Sony", Descripcion = "Auriculares con cancelación de ruido" },
                new Producto { Id = 7, Codigo = "PROD-007", Nombre = "Impresora HP LaserJet", Descripcion = "Impresora láser monocromática" },
                new Producto { Id = 8, Codigo = "PROD-008", Nombre = "Disco Duro Externo 1TB", Descripcion = "Almacenamiento portátil USB 3.0" },
                new Producto { Id = 9, Codigo = "PROD-009", Nombre = "Router TP-Link", Descripcion = "Router WiFi dual band" },
                new Producto { Id = 10, Codigo = "PROD-010", Nombre = "Silla Gamer", Descripcion = "Silla ergonómica con soporte lumbar" }
            );

            // Seed Data - Lotes
            modelBuilder.Entity<Lote>().HasData(
                new Lote { Id = 1, NumeroLote = "LOTE-2025-01", Cantidad = 10, PrecioUnitario = 850.00m, ProductoId = 1 },
                new Lote { Id = 2, NumeroLote = "LOTE-2025-02", Cantidad = 5, PrecioUnitario = 900.00m, ProductoId = 1 },
                new Lote { Id = 3, NumeroLote = "LOTE-2025-03", Cantidad = 50, PrecioUnitario = 15.00m, ProductoId = 2 },
                new Lote { Id = 4, NumeroLote = "LOTE-2025-04", Cantidad = 30, PrecioUnitario = 45.00m, ProductoId = 3 },
                new Lote { Id = 5, NumeroLote = "LOTE-2025-05", Cantidad = 20, PrecioUnitario = 180.00m, ProductoId = 4 },
                new Lote { Id = 6, NumeroLote = "LOTE-2025-06", Cantidad = 25, PrecioUnitario = 89.99m, ProductoId = 5 },
                new Lote { Id = 7, NumeroLote = "LOTE-2025-07", Cantidad = 15, PrecioUnitario = 120.00m, ProductoId = 6 },
                new Lote { Id = 8, NumeroLote = "LOTE-2025-08", Cantidad = 8, PrecioUnitario = 350.00m, ProductoId = 7 },
                new Lote { Id = 9, NumeroLote = "LOTE-2025-09", Cantidad = 40, PrecioUnitario = 65.00m, ProductoId = 8 },
                new Lote { Id = 10, NumeroLote = "LOTE-2025-10", Cantidad = 12, PrecioUnitario = 220.00m, ProductoId = 9 },
                new Lote { Id = 11, NumeroLote = "LOTE-2025-11", Cantidad = 18, PrecioUnitario = 180.00m, ProductoId = 10 }
            );

            // Seed Data - Clientes
            modelBuilder.Entity<Cliente>().HasData(
                new Cliente { Id = 1, Nombre = "Juan Pérez", CedulaRuc = "1234567890", Telefono = "0999999999", Correo = "juan@example.com", Direccion = "Av. Principal 123" },
                new Cliente { Id = 2, Nombre = "María García", CedulaRuc = "0987654321", Telefono = "0988888888", Correo = "maria@example.com", Direccion = "Calle Secundaria 456" },
                new Cliente { Id = 3, Nombre = "Carlos López", CedulaRuc = "1122334455", Telefono = "0977777777", Correo = "carlos@example.com", Direccion = "Av. Los Pinos 789" }
            );

            // Seed Data - Usuarios
            modelBuilder.Entity<Usuario>().HasData(
                new Usuario { Id = 1, NombreUsuario = "admin", Contraseña = "admin123", Rol = "Administrador" },
                new Usuario { Id = 2, NombreUsuario = "vendedor", Contraseña = "vendedor123", Rol = "Vendedor" }
            );
        }
    }
}
