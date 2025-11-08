using Microsoft.EntityFrameworkCore;
using Domain.Entities;

namespace Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Producto>()
                .Property(p => p.Precio)
                .HasPrecision(18, 4); // Puedes ajustar los números según lo que necesites
            modelBuilder.Entity<Lote>()
                .Property(l => l.Precio)
                .HasPrecision(18, 4);

            modelBuilder.Entity<Cliente>().HasData(
            new Cliente { Id = 1, CedulaRuc = "0101010101", Nombre = "Juan Perez", Telefono = "0991112233", Direccion = "Quito Centro", Correo = "juanp@example.com" },
            new Cliente { Id = 2, CedulaRuc = "0202020202", Nombre = "Ana Torres", Telefono = "0988899777", Direccion = "Guayaquil Norte", Correo = "ana.torres@example.com" },
            new Cliente { Id = 3, CedulaRuc = "1718192021", Nombre = "Mario Mena", Telefono = "0995556666", Direccion = "Cuenca", Correo = "mena.mario@example.com" },
            new Cliente { Id = 4, CedulaRuc = "2122232425", Nombre = "Luisa Vera", Telefono = "0975532332", Direccion = "Ambato", Correo = "luisa.vera@example.com" },
            new Cliente { Id = 5, CedulaRuc = "3031323334", Nombre = "Pedro Ruíz", Telefono = "0993459821", Direccion = "Manta", Correo = "pedro.ruiz@example.com" }
    );
            modelBuilder.Entity<Producto>().HasData(
                new Producto { Id = 1, Nombre = "Laptop Dell XPS", Precio = 1200.00m, Stock = 20 },
                new Producto { Id = 2, Nombre = "Mouse Logitech", Precio = 35.50m, Stock = 150 },
                new Producto { Id = 3, Nombre = "Monitor Samsung", Precio = 300.00m, Stock = 40 },
                new Producto { Id = 4, Nombre = "Teclado Mecánico", Precio = 80.00m, Stock = 60 },
                new Producto { Id = 5, Nombre = "Impresora HP", Precio = 220.75m, Stock = 25 },
                new Producto { Id = 6, Nombre = "Tablet Lenovo", Precio = 400.30m, Stock = 55 },
                new Producto { Id = 7, Nombre = "Disco SSD 1TB", Precio = 100.99m, Stock = 75 },
                new Producto { Id = 8, Nombre = "Cámara Web", Precio = 60.00m, Stock = 95 },
                new Producto { Id = 9, Nombre = "Auriculares", Precio = 45.00m, Stock = 120 },
                new Producto { Id = 10, Nombre = "Speakers Bluetooth", Precio = 70.00m, Stock = 35 }
            );
            modelBuilder.Entity<Lote>().HasData(
                // Laptop Dell XPS (ProductoId = 1): 2 lotes distintos con diferentes precios
                new Lote { Id = 1, CodigoLote = "LAP202511A", Cantidad = 10, FechaIngreso = new DateTime(2025, 10, 20), ProductoId = 1, Precio = 1200.00m },
                new Lote { Id = 2, CodigoLote = "LAP202511B", Cantidad = 10, FechaIngreso = new DateTime(2025, 11, 3), ProductoId = 1, Precio = 1180.50m },
                // Mouse Logitech (ProductoId = 2): varios lotes
                new Lote { Id = 3, CodigoLote = "MOU202510A", Cantidad = 100, FechaIngreso = new DateTime(2025, 10, 10), ProductoId = 2, Precio = 35.50m },
                new Lote { Id = 4, CodigoLote = "MOU202511A", Cantidad = 50, FechaIngreso = new DateTime(2025, 11, 2), ProductoId = 2, Precio = 34.75m },
                // Monitor Samsung
                new Lote { Id = 5, CodigoLote = "MON202509A", Cantidad = 25, FechaIngreso = new DateTime(2025, 9, 12), ProductoId = 3, Precio = 300.00m },
                new Lote { Id = 6, CodigoLote = "MON202510B", Cantidad = 15, FechaIngreso = new DateTime(2025, 10, 25), ProductoId = 3, Precio = 295.00m },
                // ... (agrega más lotes para productos restantes, puedes variar fechas y precios)
                new Lote { Id = 7, CodigoLote = "IMP202510A", Cantidad = 25, FechaIngreso = new DateTime(2025, 10, 12), ProductoId = 5, Precio = 220.75m },
                new Lote { Id = 8, CodigoLote = "SSD202511A", Cantidad = 50, FechaIngreso = new DateTime(2025, 11, 1), ProductoId = 7, Precio = 102.99m }
            // (agrega más según el stock de otros productos)
            );
            modelBuilder.Entity<Usuario>().HasData(
                new Usuario { Id = 1, NombreUsuario = "admin", Contrasena = "admin123" },
                new Usuario { Id = 2, NombreUsuario = "testuser", Contrasena = "testpass" }
            );
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Lote> Lotes { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
    }
}
