using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteAndProductPrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AplicaIva",
                table: "Productos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Productos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "PrecioVenta",
                table: "Productos",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Clientes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Clientes",
                keyColumn: "Id",
                keyValue: 1,
                column: "TipoIdentificacion",
                value: "CEDULA");

            migrationBuilder.UpdateData(
                table: "Clientes",
                keyColumn: "Id",
                keyValue: 2,
                column: "TipoIdentificacion",
                value: "RUC");

            migrationBuilder.UpdateData(
                table: "Clientes",
                keyColumn: "Id",
                keyValue: 3,
                column: "TipoIdentificacion",
                value: "PASAPORTE");

            migrationBuilder.UpdateData(
                table: "Clientes",
                keyColumn: "Id",
                keyValue: 4,
                column: "TipoIdentificacion",
                value: "CEDULA");

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "AplicaIva", "PrecioVenta" },
                values: new object[] { true, 1100.00m });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "AplicaIva", "PrecioVenta" },
                values: new object[] { true, 25.00m });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "AplicaIva", "PrecioVenta" },
                values: new object[] { true, 30.00m });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 4,
                column: "AplicaIva",
                value: true);

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "AplicaIva", "PrecioVenta" },
                values: new object[] { true, 140.00m });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "AplicaIva", "PrecioVenta" },
                values: new object[] { true, 80.00m });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "AplicaIva", "PrecioVenta" },
                values: new object[] { true, 12.00m });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "AplicaIva", "PrecioVenta" },
                values: new object[] { true, 45.00m });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 9,
                column: "PrecioVenta",
                value: 10.00m);

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 10,
                column: "PrecioVenta",
                value: 28.00m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AplicaIva",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "PrecioVenta",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Clientes");

            migrationBuilder.UpdateData(
                table: "Clientes",
                keyColumn: "Id",
                keyValue: 1,
                column: "TipoIdentificacion",
                value: "");

            migrationBuilder.UpdateData(
                table: "Clientes",
                keyColumn: "Id",
                keyValue: 2,
                column: "TipoIdentificacion",
                value: "");

            migrationBuilder.UpdateData(
                table: "Clientes",
                keyColumn: "Id",
                keyValue: 3,
                column: "TipoIdentificacion",
                value: "");

            migrationBuilder.UpdateData(
                table: "Clientes",
                keyColumn: "Id",
                keyValue: 4,
                column: "TipoIdentificacion",
                value: "");
        }
    }
}
