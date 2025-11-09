using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TipoIdentificacion = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CedulaRuc = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Direccion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Correo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Productos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Productos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Rol = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Lotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NumeroLote = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    FechaIngreso = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ProductoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Lotes_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Clientes",
                columns: new[] { "Id", "CedulaRuc", "Correo", "Direccion", "Nombre", "Telefono", "TipoIdentificacion" },
                values: new object[,]
                {
                    { 1, "1234567890", "juan.perez@email.com", "Av. Principal 123 y Secundaria, Quito", "Juan Pérez García", "0999999999", "CEDULA" },
                    { 2, "1234567890001", "ventas@distrimartinez.com", "Calle Comercio 456, Edificio Blue, Guayaquil", "DISTRIBUIDORA MARTINEZ CIA. LTDA.", "0988888888", "RUC" },
                    { 3, "USA123456", "john.smith@email.com", "Hotel Hilton, Habitación 305, Quito", "John Smith", "0977777777", "PASAPORTE" },
                    { 4, "0987654321", "maria.lopez@email.com", "Urbanización Los Pinos, Mz 5 Villa 10, Cuenca", "María Fernanda López Torres", "0966666666", "CEDULA" }
                });

            migrationBuilder.InsertData(
                table: "Productos",
                columns: new[] { "Id", "Codigo", "Descripcion", "Nombre" },
                values: new object[,]
                {
                    { 1, "PROD-001", "Intel Core i5, 8GB RAM, 256GB SSD", "Laptop HP Pavilion 15" },
                    { 2, "PROD-002", "Inalámbrico, USB, Gris", "Mouse Logitech M185" },
                    { 3, "PROD-003", "USB, Negro, Español", "Teclado Genius KB-110" },
                    { 4, "PROD-004", "Full HD, HDMI, VGA", "Monitor Samsung 24 pulgadas" },
                    { 5, "PROD-005", "Multifunción, WiFi, Color", "Impresora HP DeskJet 2775" },
                    { 6, "PROD-006", "USB 3.0, Portátil, Negro", "Disco Duro Externo 1TB" },
                    { 7, "PROD-007", "USB 3.0, Alta velocidad", "Memoria USB 32GB Kingston" },
                    { 8, "PROD-008", "720p, USB, Micrófono integrado", "Webcam Logitech C270" },
                    { 9, "PROD-009", "1080p, Compatible 4K", "Cable HDMI 2m" },
                    { 10, "PROD-010", "USB 3.0, Alimentación externa", "Hub USB 4 puertos" }
                });

            migrationBuilder.InsertData(
                table: "Lotes",
                columns: new[] { "Id", "Cantidad", "FechaIngreso", "NumeroLote", "PrecioUnitario", "ProductoId" },
                values: new object[,]
                {
                    { 1, 5, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "FAC-001-001-0001234", 850.00m, 1 },
                    { 2, 3, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "FAC-001-001-0001890", 870.00m, 1 },
                    { 3, 50, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "FAC-002-002-0002345", 15.00m, 2 },
                    { 4, 30, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "FAC-002-002-0002890", 18.00m, 2 },
                    { 5, 25, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "FAC-003-003-0003456", 20.00m, 3 },
                    { 6, 10, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "FAC-004-004-0004567", 180.00m, 4 },
                    { 7, 5, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "FAC-004-004-0004890", 175.00m, 4 },
                    { 8, 8, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "FAC-005-005-0005678", 120.00m, 5 },
                    { 9, 20, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "FAC-006-006-0006789", 55.00m, 6 },
                    { 10, 100, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "FAC-007-007-0007890", 8.00m, 7 },
                    { 11, 50, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "FAC-007-007-0007999", 7.50m, 7 },
                    { 12, 15, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "FAC-008-008-0008901", 35.00m, 8 },
                    { 13, 60, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "FAC-009-009-0009012", 5.00m, 9 },
                    { 14, 12, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "FAC-010-010-0010123", 25.00m, 10 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Lotes_ProductoId",
                table: "Lotes",
                column: "ProductoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Clientes");

            migrationBuilder.DropTable(
                name: "Lotes");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Productos");
        }
    }
}
