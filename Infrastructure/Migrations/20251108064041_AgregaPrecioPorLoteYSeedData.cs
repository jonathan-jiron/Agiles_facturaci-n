using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregaPrecioPorLoteYSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Precio",
                table: "Lotes",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.InsertData(
                table: "Clientes",
                columns: new[] { "Id", "CedulaRuc", "Correo", "Direccion", "Nombre", "Telefono" },
                values: new object[,]
                {
                    { 1, "0101010101", "juanp@example.com", "Quito Centro", "Juan Perez", "0991112233" },
                    { 2, "0202020202", "ana.torres@example.com", "Guayaquil Norte", "Ana Torres", "0988899777" },
                    { 3, "1718192021", "mena.mario@example.com", "Cuenca", "Mario Mena", "0995556666" },
                    { 4, "2122232425", "luisa.vera@example.com", "Ambato", "Luisa Vera", "0975532332" },
                    { 5, "3031323334", "pedro.ruiz@example.com", "Manta", "Pedro Ruíz", "0993459821" }
                });

            migrationBuilder.InsertData(
                table: "Productos",
                columns: new[] { "Id", "Nombre", "Precio", "Stock" },
                values: new object[,]
                {
                    { 1, "Laptop Dell XPS", 1200.00m, 20 },
                    { 2, "Mouse Logitech", 35.50m, 150 },
                    { 3, "Monitor Samsung", 300.00m, 40 },
                    { 4, "Teclado Mecánico", 80.00m, 60 },
                    { 5, "Impresora HP", 220.75m, 25 },
                    { 6, "Tablet Lenovo", 400.30m, 55 },
                    { 7, "Disco SSD 1TB", 100.99m, 75 },
                    { 8, "Cámara Web", 60.00m, 95 },
                    { 9, "Auriculares", 45.00m, 120 },
                    { 10, "Speakers Bluetooth", 70.00m, 35 }
                });

            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "Id", "Contrasena", "NombreUsuario" },
                values: new object[,]
                {
                    { 1, "admin123", "admin" },
                    { 2, "testpass", "testuser" }
                });

            migrationBuilder.InsertData(
                table: "Lotes",
                columns: new[] { "Id", "Cantidad", "CodigoLote", "FechaIngreso", "Precio", "ProductoId" },
                values: new object[,]
                {
                    { 1, 10, "LAP202511A", new DateTime(2025, 10, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), 1200.00m, 1 },
                    { 2, 10, "LAP202511B", new DateTime(2025, 11, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), 1180.50m, 1 },
                    { 3, 100, "MOU202510A", new DateTime(2025, 10, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), 35.50m, 2 },
                    { 4, 50, "MOU202511A", new DateTime(2025, 11, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), 34.75m, 2 },
                    { 5, 25, "MON202509A", new DateTime(2025, 9, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), 300.00m, 3 },
                    { 6, 15, "MON202510B", new DateTime(2025, 10, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), 295.00m, 3 },
                    { 7, 25, "IMP202510A", new DateTime(2025, 10, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), 220.75m, 5 },
                    { 8, 50, "SSD202511A", new DateTime(2025, 11, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 102.99m, 7 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Clientes",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Clientes",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Clientes",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Clientes",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Clientes",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Lotes",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Lotes",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Lotes",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Lotes",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Lotes",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Lotes",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Lotes",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Lotes",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DropColumn(
                name: "Precio",
                table: "Lotes");
        }
    }
}
