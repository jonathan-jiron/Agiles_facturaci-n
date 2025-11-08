using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ActualizarModeloCompleto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Clientes",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Clientes",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DropColumn(
                name: "Precio",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "Stock",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "FechaIngreso",
                table: "Lotes");

            migrationBuilder.DropColumn(
                name: "Precio",
                table: "Lotes");

            migrationBuilder.RenameColumn(
                name: "Contrasena",
                table: "Usuarios",
                newName: "Rol");

            migrationBuilder.RenameColumn(
                name: "CodigoLote",
                table: "Lotes",
                newName: "NumeroLote");

            migrationBuilder.AddColumn<string>(
                name: "Contraseña",
                table: "Usuarios",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Codigo",
                table: "Productos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Descripcion",
                table: "Productos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PrecioUnitario",
                table: "Lotes",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "Telefono",
                table: "Clientes",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Direccion",
                table: "Clientes",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.UpdateData(
                table: "Clientes",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CedulaRuc", "Correo", "Direccion", "Nombre", "Telefono" },
                values: new object[] { "1234567890", "juan@example.com", "Av. Principal 123", "Juan Pérez", "0999999999" });

            migrationBuilder.UpdateData(
                table: "Clientes",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CedulaRuc", "Correo", "Direccion", "Nombre", "Telefono" },
                values: new object[] { "0987654321", "maria@example.com", "Calle Secundaria 456", "María García", "0988888888" });

            migrationBuilder.UpdateData(
                table: "Clientes",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CedulaRuc", "Correo", "Direccion", "Nombre", "Telefono" },
                values: new object[] { "1122334455", "carlos@example.com", "Av. Los Pinos 789", "Carlos López", "0977777777" });

            migrationBuilder.UpdateData(
                table: "Lotes",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "NumeroLote", "PrecioUnitario" },
                values: new object[] { "LOTE-2025-01", 850.00m });

            migrationBuilder.UpdateData(
                table: "Lotes",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Cantidad", "NumeroLote", "PrecioUnitario" },
                values: new object[] { 5, "LOTE-2025-02", 900.00m });

            migrationBuilder.UpdateData(
                table: "Lotes",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Cantidad", "NumeroLote", "PrecioUnitario" },
                values: new object[] { 50, "LOTE-2025-03", 15.00m });

            migrationBuilder.UpdateData(
                table: "Lotes",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Cantidad", "NumeroLote", "PrecioUnitario", "ProductoId" },
                values: new object[] { 30, "LOTE-2025-04", 45.00m, 3 });

            migrationBuilder.UpdateData(
                table: "Lotes",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Cantidad", "NumeroLote", "PrecioUnitario", "ProductoId" },
                values: new object[] { 20, "LOTE-2025-05", 180.00m, 4 });

            migrationBuilder.UpdateData(
                table: "Lotes",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Cantidad", "NumeroLote", "PrecioUnitario", "ProductoId" },
                values: new object[] { 25, "LOTE-2025-06", 89.99m, 5 });

            migrationBuilder.UpdateData(
                table: "Lotes",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Cantidad", "NumeroLote", "PrecioUnitario", "ProductoId" },
                values: new object[] { 15, "LOTE-2025-07", 120.00m, 6 });

            migrationBuilder.UpdateData(
                table: "Lotes",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Cantidad", "NumeroLote", "PrecioUnitario" },
                values: new object[] { 8, "LOTE-2025-08", 350.00m });

            migrationBuilder.InsertData(
                table: "Lotes",
                columns: new[] { "Id", "Cantidad", "NumeroLote", "PrecioUnitario", "ProductoId" },
                values: new object[,]
                {
                    { 9, 40, "LOTE-2025-09", 65.00m, 8 },
                    { 10, 12, "LOTE-2025-10", 220.00m, 9 },
                    { 11, 18, "LOTE-2025-11", 180.00m, 10 }
                });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Codigo", "Descripcion", "Nombre" },
                values: new object[] { "PROD-001", "Laptop 15 pulgadas, 8GB RAM, 256GB SSD", "Laptop HP Pavilion" });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Codigo", "Descripcion" },
                values: new object[] { "PROD-002", "Mouse inalámbrico ergonómico" });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Codigo", "Descripcion", "Nombre" },
                values: new object[] { "PROD-003", "Teclado RGB retroiluminado", "Teclado Mecánico" });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Codigo", "Descripcion", "Nombre" },
                values: new object[] { "PROD-004", "Monitor Full HD IPS", "Monitor LG 24\"" });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Codigo", "Descripcion", "Nombre" },
                values: new object[] { "PROD-005", "Webcam Full HD 1080p", "Webcam Logitech C920" });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Codigo", "Descripcion", "Nombre" },
                values: new object[] { "PROD-006", "Auriculares con cancelación de ruido", "Auriculares Sony" });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Codigo", "Descripcion", "Nombre" },
                values: new object[] { "PROD-007", "Impresora láser monocromática", "Impresora HP LaserJet" });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Codigo", "Descripcion", "Nombre" },
                values: new object[] { "PROD-008", "Almacenamiento portátil USB 3.0", "Disco Duro Externo 1TB" });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "Codigo", "Descripcion", "Nombre" },
                values: new object[] { "PROD-009", "Router WiFi dual band", "Router TP-Link" });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "Codigo", "Descripcion", "Nombre" },
                values: new object[] { "PROD-010", "Silla ergonómica con soporte lumbar", "Silla Gamer" });

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Contraseña", "Rol" },
                values: new object[] { "admin123", "Administrador" });

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Contraseña", "NombreUsuario", "Rol" },
                values: new object[] { "vendedor123", "vendedor", "Vendedor" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Lotes",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Lotes",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Lotes",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DropColumn(
                name: "Contraseña",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "Codigo",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "Descripcion",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "PrecioUnitario",
                table: "Lotes");

            migrationBuilder.RenameColumn(
                name: "Rol",
                table: "Usuarios",
                newName: "Contrasena");

            migrationBuilder.RenameColumn(
                name: "NumeroLote",
                table: "Lotes",
                newName: "CodigoLote");

            migrationBuilder.AddColumn<decimal>(
                name: "Precio",
                table: "Productos",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "Stock",
                table: "Productos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaIngreso",
                table: "Lotes",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "Precio",
                table: "Lotes",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "Telefono",
                table: "Clientes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Direccion",
                table: "Clientes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Clientes",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CedulaRuc", "Correo", "Direccion", "Nombre", "Telefono" },
                values: new object[] { "0101010101", "juanp@example.com", "Quito Centro", "Juan Perez", "0991112233" });

            migrationBuilder.UpdateData(
                table: "Clientes",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CedulaRuc", "Correo", "Direccion", "Nombre", "Telefono" },
                values: new object[] { "0202020202", "ana.torres@example.com", "Guayaquil Norte", "Ana Torres", "0988899777" });

            migrationBuilder.UpdateData(
                table: "Clientes",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CedulaRuc", "Correo", "Direccion", "Nombre", "Telefono" },
                values: new object[] { "1718192021", "mena.mario@example.com", "Cuenca", "Mario Mena", "0995556666" });

            migrationBuilder.InsertData(
                table: "Clientes",
                columns: new[] { "Id", "CedulaRuc", "Correo", "Direccion", "Nombre", "Telefono" },
                values: new object[,]
                {
                    { 4, "2122232425", "luisa.vera@example.com", "Ambato", "Luisa Vera", "0975532332" },
                    { 5, "3031323334", "pedro.ruiz@example.com", "Manta", "Pedro Ruíz", "0993459821" }
                });

            migrationBuilder.UpdateData(
                table: "Lotes",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CodigoLote", "FechaIngreso", "Precio" },
                values: new object[] { "LAP202511A", new DateTime(2025, 10, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), 1200.00m });

            migrationBuilder.UpdateData(
                table: "Lotes",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Cantidad", "CodigoLote", "FechaIngreso", "Precio" },
                values: new object[] { 10, "LAP202511B", new DateTime(2025, 11, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), 1180.50m });

            migrationBuilder.UpdateData(
                table: "Lotes",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Cantidad", "CodigoLote", "FechaIngreso", "Precio" },
                values: new object[] { 100, "MOU202510A", new DateTime(2025, 10, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), 35.50m });

            migrationBuilder.UpdateData(
                table: "Lotes",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Cantidad", "CodigoLote", "FechaIngreso", "Precio", "ProductoId" },
                values: new object[] { 50, "MOU202511A", new DateTime(2025, 11, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), 34.75m, 2 });

            migrationBuilder.UpdateData(
                table: "Lotes",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Cantidad", "CodigoLote", "FechaIngreso", "Precio", "ProductoId" },
                values: new object[] { 25, "MON202509A", new DateTime(2025, 9, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), 300.00m, 3 });

            migrationBuilder.UpdateData(
                table: "Lotes",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Cantidad", "CodigoLote", "FechaIngreso", "Precio", "ProductoId" },
                values: new object[] { 15, "MON202510B", new DateTime(2025, 10, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), 295.00m, 3 });

            migrationBuilder.UpdateData(
                table: "Lotes",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Cantidad", "CodigoLote", "FechaIngreso", "Precio", "ProductoId" },
                values: new object[] { 25, "IMP202510A", new DateTime(2025, 10, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), 220.75m, 5 });

            migrationBuilder.UpdateData(
                table: "Lotes",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Cantidad", "CodigoLote", "FechaIngreso", "Precio" },
                values: new object[] { 50, "SSD202511A", new DateTime(2025, 11, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 102.99m });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Nombre", "Precio", "Stock" },
                values: new object[] { "Laptop Dell XPS", 1200.00m, 20 });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Precio", "Stock" },
                values: new object[] { 35.50m, 150 });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Nombre", "Precio", "Stock" },
                values: new object[] { "Monitor Samsung", 300.00m, 40 });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Nombre", "Precio", "Stock" },
                values: new object[] { "Teclado Mecánico", 80.00m, 60 });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Nombre", "Precio", "Stock" },
                values: new object[] { "Impresora HP", 220.75m, 25 });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Nombre", "Precio", "Stock" },
                values: new object[] { "Tablet Lenovo", 400.30m, 55 });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Nombre", "Precio", "Stock" },
                values: new object[] { "Disco SSD 1TB", 100.99m, 75 });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Nombre", "Precio", "Stock" },
                values: new object[] { "Cámara Web", 60.00m, 95 });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "Nombre", "Precio", "Stock" },
                values: new object[] { "Auriculares", 45.00m, 120 });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "Nombre", "Precio", "Stock" },
                values: new object[] { "Speakers Bluetooth", 70.00m, 35 });

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Contrasena",
                value: "admin123");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Contrasena", "NombreUsuario" },
                values: new object[] { "testpass", "testuser" });
        }
    }
}
