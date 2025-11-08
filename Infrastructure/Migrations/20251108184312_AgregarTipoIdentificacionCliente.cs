using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarTipoIdentificacionCliente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DropColumn(
                name: "Contraseña",
                table: "Usuarios");

            migrationBuilder.RenameColumn(
                name: "NombreUsuario",
                table: "Usuarios",
                newName: "PasswordHash");

            migrationBuilder.AlterColumn<string>(
                name: "Rol",
                table: "Usuarios",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "Usuarios",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Productos",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Descripcion",
                table: "Productos",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Codigo",
                table: "Productos",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "NumeroLote",
                table: "Lotes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaIngreso",
                table: "Lotes",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<string>(
                name: "Telefono",
                table: "Clientes",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Clientes",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Direccion",
                table: "Clientes",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Correo",
                table: "Clientes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "CedulaRuc",
                table: "Clientes",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "TipoIdentificacion",
                table: "Clientes",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Clientes",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Correo", "Direccion", "Nombre", "TipoIdentificacion" },
                values: new object[] { "juan.perez@email.com", "Av. Principal 123 y Secundaria, Quito", "Juan Pérez García", "CEDULA" });

            migrationBuilder.UpdateData(
                table: "Clientes",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CedulaRuc", "Correo", "Direccion", "Nombre", "TipoIdentificacion" },
                values: new object[] { "1234567890001", "ventas@distrimartinez.com", "Calle Comercio 456, Edificio Blue, Guayaquil", "DISTRIBUIDORA MARTINEZ CIA. LTDA.", "RUC" });

            migrationBuilder.UpdateData(
                table: "Clientes",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CedulaRuc", "Correo", "Direccion", "Nombre", "TipoIdentificacion" },
                values: new object[] { "USA123456", "john.smith@email.com", "Hotel Hilton, Habitación 305, Quito", "John Smith", "PASAPORTE" });

            migrationBuilder.InsertData(
                table: "Clientes",
                columns: new[] { "Id", "CedulaRuc", "Correo", "Direccion", "Nombre", "Telefono", "TipoIdentificacion" },
                values: new object[] { 4, "0987654321", "maria.lopez@email.com", "Urbanización Los Pinos, Mz 5 Villa 10, Cuenca", "María Fernanda López Torres", "0966666666", "CEDULA" });

            migrationBuilder.UpdateData(
                table: "Lotes",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Cantidad", "FechaIngreso", "NumeroLote" },
                values: new object[] { 5, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "FAC-001-001-0001234" });

            migrationBuilder.UpdateData(
                table: "Lotes",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Cantidad", "FechaIngreso", "NumeroLote", "PrecioUnitario" },
                values: new object[] { 3, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "FAC-001-001-0001890", 870.00m });

            migrationBuilder.UpdateData(
                table: "Lotes",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "FechaIngreso", "NumeroLote" },
                values: new object[] { new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "FAC-002-002-0002345" });

            migrationBuilder.UpdateData(
                table: "Lotes",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "FechaIngreso", "NumeroLote", "PrecioUnitario", "ProductoId" },
                values: new object[] { new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "FAC-002-002-0002890", 18.00m, 2 });

            migrationBuilder.UpdateData(
                table: "Lotes",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Cantidad", "FechaIngreso", "NumeroLote", "PrecioUnitario", "ProductoId" },
                values: new object[] { 25, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "FAC-003-003-0003456", 20.00m, 3 });

            migrationBuilder.UpdateData(
                table: "Lotes",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Cantidad", "FechaIngreso", "NumeroLote", "PrecioUnitario", "ProductoId" },
                values: new object[] { 10, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "FAC-004-004-0004567", 180.00m, 4 });

            migrationBuilder.UpdateData(
                table: "Lotes",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Cantidad", "FechaIngreso", "NumeroLote", "PrecioUnitario", "ProductoId" },
                values: new object[] { 5, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "FAC-004-004-0004890", 175.00m, 4 });

            migrationBuilder.UpdateData(
                table: "Lotes",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "FechaIngreso", "NumeroLote", "PrecioUnitario", "ProductoId" },
                values: new object[] { new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "FAC-005-005-0005678", 120.00m, 5 });

            migrationBuilder.UpdateData(
                table: "Lotes",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "Cantidad", "FechaIngreso", "NumeroLote", "PrecioUnitario", "ProductoId" },
                values: new object[] { 20, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "FAC-006-006-0006789", 55.00m, 6 });

            migrationBuilder.UpdateData(
                table: "Lotes",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "Cantidad", "FechaIngreso", "NumeroLote", "PrecioUnitario", "ProductoId" },
                values: new object[] { 100, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "FAC-007-007-0007890", 8.00m, 7 });

            migrationBuilder.UpdateData(
                table: "Lotes",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "Cantidad", "FechaIngreso", "NumeroLote", "PrecioUnitario", "ProductoId" },
                values: new object[] { 50, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "FAC-007-007-0007999", 7.50m, 7 });

            migrationBuilder.InsertData(
                table: "Lotes",
                columns: new[] { "Id", "Cantidad", "FechaIngreso", "NumeroLote", "PrecioUnitario", "ProductoId" },
                values: new object[,]
                {
                    { 12, 15, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "FAC-008-008-0008901", 35.00m, 8 },
                    { 13, 60, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "FAC-009-009-0009012", 5.00m, 9 },
                    { 14, 12, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "FAC-010-010-0010123", 25.00m, 10 }
                });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Descripcion", "Nombre" },
                values: new object[] { "Intel Core i5, 8GB RAM, 256GB SSD", "Laptop HP Pavilion 15" });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Descripcion", "Nombre" },
                values: new object[] { "Inalámbrico, USB, Gris", "Mouse Logitech M185" });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Descripcion", "Nombre" },
                values: new object[] { "USB, Negro, Español", "Teclado Genius KB-110" });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Descripcion", "Nombre" },
                values: new object[] { "Full HD, HDMI, VGA", "Monitor Samsung 24 pulgadas" });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Descripcion", "Nombre" },
                values: new object[] { "Multifunción, WiFi, Color", "Impresora HP DeskJet 2775" });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Descripcion", "Nombre" },
                values: new object[] { "USB 3.0, Portátil, Negro", "Disco Duro Externo 1TB" });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Descripcion", "Nombre" },
                values: new object[] { "USB 3.0, Alta velocidad", "Memoria USB 32GB Kingston" });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Descripcion", "Nombre" },
                values: new object[] { "720p, USB, Micrófono integrado", "Webcam Logitech C270" });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "Descripcion", "Nombre" },
                values: new object[] { "1080p, Compatible 4K", "Cable HDMI 2m" });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "Descripcion", "Nombre" },
                values: new object[] { "USB 3.0, Alimentación externa", "Hub USB 4 puertos" });

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "PasswordHash", "Rol", "Username" },
                values: new object[] { "$2a$11$8z7kZqK5y3K0Wl8KqX5Y.eN7HgZ8xF2Q3R5T6Y7U8V9W0X1Y2Z3A4", "ADMIN", "admin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Clientes",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Lotes",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Lotes",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Lotes",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DropColumn(
                name: "Username",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "FechaIngreso",
                table: "Lotes");

            migrationBuilder.DropColumn(
                name: "TipoIdentificacion",
                table: "Clientes");

            migrationBuilder.RenameColumn(
                name: "PasswordHash",
                table: "Usuarios",
                newName: "NombreUsuario");

            migrationBuilder.AlterColumn<string>(
                name: "Rol",
                table: "Usuarios",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<string>(
                name: "Contraseña",
                table: "Usuarios",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Productos",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Descripcion",
                table: "Productos",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Codigo",
                table: "Productos",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "NumeroLote",
                table: "Lotes",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Telefono",
                table: "Clientes",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(15)",
                oldMaxLength: 15);

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Clientes",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Direccion",
                table: "Clientes",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(300)",
                oldMaxLength: 300);

            migrationBuilder.AlterColumn<string>(
                name: "Correo",
                table: "Clientes",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "CedulaRuc",
                table: "Clientes",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.UpdateData(
                table: "Clientes",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Correo", "Direccion", "Nombre" },
                values: new object[] { "juan@example.com", "Av. Principal 123", "Juan Pérez" });

            migrationBuilder.UpdateData(
                table: "Clientes",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CedulaRuc", "Correo", "Direccion", "Nombre" },
                values: new object[] { "0987654321", "maria@example.com", "Calle Secundaria 456", "María García" });

            migrationBuilder.UpdateData(
                table: "Clientes",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CedulaRuc", "Correo", "Direccion", "Nombre" },
                values: new object[] { "1122334455", "carlos@example.com", "Av. Los Pinos 789", "Carlos López" });

            migrationBuilder.UpdateData(
                table: "Lotes",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Cantidad", "NumeroLote" },
                values: new object[] { 10, "LOTE-2025-01" });

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
                column: "NumeroLote",
                value: "LOTE-2025-03");

            migrationBuilder.UpdateData(
                table: "Lotes",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "NumeroLote", "PrecioUnitario", "ProductoId" },
                values: new object[] { "LOTE-2025-04", 45.00m, 3 });

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
                columns: new[] { "NumeroLote", "PrecioUnitario", "ProductoId" },
                values: new object[] { "LOTE-2025-08", 350.00m, 7 });

            migrationBuilder.UpdateData(
                table: "Lotes",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "Cantidad", "NumeroLote", "PrecioUnitario", "ProductoId" },
                values: new object[] { 40, "LOTE-2025-09", 65.00m, 8 });

            migrationBuilder.UpdateData(
                table: "Lotes",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "Cantidad", "NumeroLote", "PrecioUnitario", "ProductoId" },
                values: new object[] { 12, "LOTE-2025-10", 220.00m, 9 });

            migrationBuilder.UpdateData(
                table: "Lotes",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "Cantidad", "NumeroLote", "PrecioUnitario", "ProductoId" },
                values: new object[] { 18, "LOTE-2025-11", 180.00m, 10 });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Descripcion", "Nombre" },
                values: new object[] { "Laptop 15 pulgadas, 8GB RAM, 256GB SSD", "Laptop HP Pavilion" });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Descripcion", "Nombre" },
                values: new object[] { "Mouse inalámbrico ergonómico", "Mouse Logitech" });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Descripcion", "Nombre" },
                values: new object[] { "Teclado RGB retroiluminado", "Teclado Mecánico" });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Descripcion", "Nombre" },
                values: new object[] { "Monitor Full HD IPS", "Monitor LG 24\"" });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Descripcion", "Nombre" },
                values: new object[] { "Webcam Full HD 1080p", "Webcam Logitech C920" });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Descripcion", "Nombre" },
                values: new object[] { "Auriculares con cancelación de ruido", "Auriculares Sony" });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Descripcion", "Nombre" },
                values: new object[] { "Impresora láser monocromática", "Impresora HP LaserJet" });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Descripcion", "Nombre" },
                values: new object[] { "Almacenamiento portátil USB 3.0", "Disco Duro Externo 1TB" });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "Descripcion", "Nombre" },
                values: new object[] { "Router WiFi dual band", "Router TP-Link" });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "Descripcion", "Nombre" },
                values: new object[] { "Silla ergonómica con soporte lumbar", "Silla Gamer" });

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Contraseña", "NombreUsuario", "Rol" },
                values: new object[] { "admin123", "admin", "Administrador" });

            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "Id", "Contraseña", "NombreUsuario", "Rol" },
                values: new object[] { 2, "vendedor123", "vendedor", "Vendedor" });
        }
    }
}
