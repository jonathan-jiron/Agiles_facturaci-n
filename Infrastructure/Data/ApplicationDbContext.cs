using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Lote> Lotes { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<EventoActividad> EventosActividad { get; set; }
        public DbSet<Factura> Facturas { get; set; }
        public DbSet<DetalleFactura> DetallesFactura { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Índices
            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<Cliente>()
                .HasIndex(c => c.Identificacion)
                .IsUnique();

            // Concurrency (opcional)
            modelBuilder.Entity<Cliente>()
                .Property(c => c.RowVersion)
                .IsRowVersion();

            modelBuilder.Entity<Producto>()
                .Property(p => p.RowVersion)
                .IsRowVersion();

            // Shadow properties and global query filters for soft delete
            modelBuilder.Entity<Producto>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Codigo)
                      .IsRequired()
                      .HasMaxLength(50);
                entity.Property(p => p.Nombre)
                      .IsRequired()
                      .HasMaxLength(200);
                entity.Property(p => p.Descripcion)
                      .HasMaxLength(500);

                entity.Property<decimal>("PrecioVenta").HasPrecision(18, 2).HasDefaultValue(0m);
                entity.Property<bool>("AplicaIva").HasDefaultValue(false);
                entity.Property<bool>("IsDeleted").HasDefaultValue(false);

                entity.HasQueryFilter(p => !EF.Property<bool>(p, "IsDeleted"));

                entity.HasMany(p => p.Lotes)
                      .WithOne(l => l.Producto)
                      .HasForeignKey(l => l.ProductoId)
                      .OnDelete(DeleteBehavior.Cascade)
                      .IsRequired(false); // Relación opcional
            });

            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.HasKey(c => c.Id);

                entity.Property(c => c.NombreRazonSocial)
                      .IsRequired()
                      .HasMaxLength(200);

                entity.Property(c => c.Identificacion)
                      .IsRequired()
                      .HasMaxLength(20);

                entity.Property(c => c.Telefono)
                      .HasMaxLength(30);

                entity.Property(c => c.Direccion)
                      .HasMaxLength(300);

                entity.Property(c => c.Correo)
                      .HasMaxLength(150);

                entity.Property<bool>("IsDeleted").HasDefaultValue(false);
                entity.HasQueryFilter(c => !EF.Property<bool>(c, "IsDeleted"));
            });

            modelBuilder.Entity<Lote>(entity =>
            {
                entity.HasKey(l => l.Id);
                entity.Property(l => l.NumeroLote)
                      .IsRequired()
                      .HasMaxLength(100);
                entity.HasIndex(l => l.NumeroLote).IsUnique();
                entity.Property(l => l.Cantidad)
                      .IsRequired();
                entity.Property(l => l.PrecioUnitario)
                      .HasPrecision(18, 2)
                      .IsRequired();
                entity.Property(l => l.FechaIngreso)
                      .IsRequired();
                entity.Property(l => l.FechaVencimiento);

                entity.HasOne(l => l.Producto)
                      .WithMany(p => p.Lotes)
                      .HasForeignKey(l => l.ProductoId)
                      .OnDelete(DeleteBehavior.Cascade)
                      .IsRequired(false); // Relación opcional
            });

            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Username)
                      .IsRequired()
                      .HasMaxLength(50);
                entity.Property(u => u.PasswordHash)
                      .IsRequired();
                entity.Property(u => u.Rol)
                      .IsRequired()
                      .HasMaxLength(20);
            });

            // Configuración básica para Factura y DetalleFactura
            modelBuilder.Entity<Factura>(entity =>
            {
                entity.HasKey(f => f.Id);
                entity.Property(f => f.Numero).IsRequired().HasMaxLength(50);
                entity.Property(f => f.Subtotal).HasPrecision(18, 2);
                entity.Property(f => f.Iva).HasPrecision(18, 2);
                entity.Property(f => f.Total).HasPrecision(18, 2);
                entity.HasMany(f => f.Detalles)
                        .WithOne(d => d.Factura)
                        .HasForeignKey(d => d.FacturaId)
                        .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(f => f.Cliente)
                      .WithMany()
                      .HasForeignKey(f => f.ClienteId)
                      .OnDelete(DeleteBehavior.Cascade)
                      .IsRequired(false); // Relación opcional
            });

            modelBuilder.Entity<DetalleFactura>(entity =>
            {
                entity.HasKey(d => d.Id);
                entity.Property(d => d.Cantidad).IsRequired();
                entity.Property(d => d.PrecioUnitario).HasPrecision(18, 2);
                entity.Property(d => d.Iva).HasPrecision(18, 2);
                entity.Property(d => d.Total).HasPrecision(18, 2);
                entity.Property(d => d.PrecioTotal)
                      .HasColumnType("decimal(18,2)");

                entity.HasOne(d => d.Producto)
                      .WithMany()
                      .HasForeignKey(d => d.ProductoId)
                      .OnDelete(DeleteBehavior.Cascade)
                      .IsRequired(false); // Relación opcional
            });

            // Seed Data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            var fechaActual = new DateTime(2025, 1, 1);

            modelBuilder.Entity<Cliente>().HasData(
                new { Id = 1, TipoIdentificacion = "CEDULA", Identificacion = "1234567890", NombreRazonSocial = "Juan Pérez García", Telefono = "0999999999", Direccion = "Av. Principal 123 y Secundaria, Quito", Correo = "juan.perez@email.com", IsDeleted = false },
                new { Id = 2, TipoIdentificacion = "RUC", Identificacion = "1234567890001", NombreRazonSocial = "DISTRIBUIDORA MARTINEZ CIA. LTDA.", Telefono = "0988888888", Direccion = "Calle Comercio 456, Edificio Blue, Guayaquil", Correo = "ventas@distrimartinez.com", IsDeleted = false },
                new { Id = 3, TipoIdentificacion = "PASAPORTE", Identificacion = "USA123456", NombreRazonSocial = "John Smith", Telefono = "0977777777", Direccion = "Hotel Hilton, Habitación 305, Quito", Correo = "john.smith@email.com", IsDeleted = false },
                new { Id = 4, TipoIdentificacion = "CEDULA", Identificacion = "0987654321", NombreRazonSocial = "María Fernanda López Torres", Telefono = "0966666666", Direccion = "Urbanización Los Pinos, Mz 5 Villa 10, Cuenca", Correo = "maria.lopez@email.com", IsDeleted = false }
            );

            modelBuilder.Entity<Producto>().HasData(
                new { Id = 1, Codigo = "PROD-001", Nombre = "Laptop HP Pavilion 15", Descripcion = "Intel Core i5, 8GB RAM, 256GB SSD", PrecioVenta = 1100.00m, AplicaIva = true, IsDeleted = false },
                new { Id = 2, Codigo = "PROD-002", Nombre = "Mouse Logitech M185", Descripcion = "Inalámbrico, USB, Gris", PrecioVenta = 25.00m, AplicaIva = true, IsDeleted = false },
                new { Id = 3, Codigo = "PROD-003", Nombre = "Teclado Genius KB-110", Descripcion = "USB, Negro, Español", PrecioVenta = 30.00m, AplicaIva = true, IsDeleted = false },
                new { Id = 4, Codigo = "PROD-004", Nombre = "Monitor Samsung 24 pulgadas", Descripcion = "Full HD, HDMI, VGA", PrecioVenta = 220.00m, AplicaIva = true, IsDeleted = false },
                new { Id = 5, Codigo = "PROD-005", Nombre = "Impresora HP DeskJet 2775", Descripcion = "Multifunción, WiFi, Color", PrecioVenta = 140.00m, AplicaIva = true, IsDeleted = false },
                new { Id = 6, Codigo = "PROD-006", Nombre = "Disco Duro Externo 1TB", Descripcion = "USB 3.0, Portátil, Negro", PrecioVenta = 80.00m, AplicaIva = true, IsDeleted = false },
                new { Id = 7, Codigo = "PROD-007", Nombre = "Memoria USB 32GB Kingston", Descripcion = "USB 3.0, Alta velocidad", PrecioVenta = 12.00m, AplicaIva = true, IsDeleted = false },
                new { Id = 8, Codigo = "PROD-008", Nombre = "Webcam Logitech C270", Descripcion = "720p, USB, Micrófono integrado", PrecioVenta = 45.00m, AplicaIva = true, IsDeleted = false },
                new { Id = 9, Codigo = "PROD-009", Nombre = "Cable HDMI 2m", Descripcion = "1080p, Compatible 4K", PrecioVenta = 10.00m, AplicaIva = false, IsDeleted = false },
                new { Id = 10, Codigo = "PROD-010", Nombre = "Hub USB 4 puertos", Descripcion = "USB 3.0, Alimentación externa", PrecioVenta = 28.00m, AplicaIva = false, IsDeleted = false }
            );

            modelBuilder.Entity<Lote>().HasData(
                new Lote { Id = 1,  NumeroLote = "FAC-001-001-0001234", ProductoId = 1, Cantidad = 5,  PrecioUnitario = 850.00m, FechaIngreso = fechaActual, FechaVencimiento = fechaActual.AddYears(1) },
                new Lote { Id = 2,  NumeroLote = "FAC-001-001-0001890", ProductoId = 1, Cantidad = 3,  PrecioUnitario = 870.00m, FechaIngreso = fechaActual },
                new Lote { Id = 3,  NumeroLote = "FAC-002-002-0002345", ProductoId = 2, Cantidad = 50, PrecioUnitario = 15.00m,  FechaIngreso = fechaActual },
                new Lote { Id = 4,  NumeroLote = "FAC-002-002-0002890", ProductoId = 2, Cantidad = 30, PrecioUnitario = 18.00m, FechaIngreso = fechaActual },
                new Lote { Id = 5,  NumeroLote = "FAC-003-003-0003456", ProductoId = 3, Cantidad = 25, PrecioUnitario = 20.00m, FechaIngreso = fechaActual },
                new Lote { Id = 6,  NumeroLote = "FAC-004-004-0004567", ProductoId = 4, Cantidad = 10, PrecioUnitario = 180.00m, FechaIngreso = fechaActual },
                new Lote { Id = 7,  NumeroLote = "FAC-004-004-0004890", ProductoId = 4, Cantidad = 5,  PrecioUnitario = 175.00m, FechaIngreso = fechaActual },
                new Lote { Id = 8,  NumeroLote = "FAC-005-005-0005678", ProductoId = 5, Cantidad = 8,  PrecioUnitario = 120.00m, FechaIngreso = fechaActual },
                new Lote { Id = 9,  NumeroLote = "FAC-006-006-0006789", ProductoId = 6, Cantidad = 20, PrecioUnitario = 55.00m,  FechaIngreso = fechaActual },
                new Lote { Id = 10, NumeroLote = "FAC-007-007-0007890", ProductoId = 7, Cantidad = 100, PrecioUnitario = 8.00m,  FechaIngreso = fechaActual },
                new Lote { Id = 11, NumeroLote = "FAC-007-007-0007999", ProductoId = 7, Cantidad = 50, PrecioUnitario = 7.50m,  FechaIngreso = fechaActual },
                new Lote { Id = 12, NumeroLote = "FAC-008-008-0008901", ProductoId = 8, Cantidad = 15, PrecioUnitario = 35.00m,  FechaIngreso = fechaActual },
                new Lote { Id = 13, NumeroLote = "FAC-009-009-0009012", ProductoId = 9, Cantidad = 60, PrecioUnitario = 5.00m,  FechaIngreso = fechaActual },
                new Lote { Id = 14, NumeroLote = "FAC-010-010-0010123", ProductoId = 10, Cantidad = 12, PrecioUnitario = 25.00m, FechaIngreso = fechaActual }
            );
        }
    }
}
