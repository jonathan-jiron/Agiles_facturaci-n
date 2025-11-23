using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSriFieldsFinalFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClaveAcceso",
                table: "Facturas",
                type: "nvarchar(49)",
                maxLength: 49,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EstadoSRI",
                table: "Facturas",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MotivoRechazo",
                table: "Facturas",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "XmlComprobante",
                table: "Facturas",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "XmlRecepcion",
                table: "Facturas",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClaveAcceso",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "EstadoSRI",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "MotivoRechazo",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "XmlComprobante",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "XmlRecepcion",
                table: "Facturas");
        }
    }
}
