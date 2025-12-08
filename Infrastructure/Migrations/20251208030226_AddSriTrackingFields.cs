using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSriTrackingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClaveAcceso",
                table: "Facturas",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EstadoSri",
                table: "Facturas",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaAutorizacion",
                table: "Facturas",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MensajesSri",
                table: "Facturas",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NumeroAutorizacion",
                table: "Facturas",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SecuencialSri",
                table: "Facturas",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "XmlAutorizado",
                table: "Facturas",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "XmlFirmado",
                table: "Facturas",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "XmlGenerado",
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
                name: "EstadoSri",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "FechaAutorizacion",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "MensajesSri",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "NumeroAutorizacion",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "SecuencialSri",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "XmlAutorizado",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "XmlFirmado",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "XmlGenerado",
                table: "Facturas");
        }
    }
}
