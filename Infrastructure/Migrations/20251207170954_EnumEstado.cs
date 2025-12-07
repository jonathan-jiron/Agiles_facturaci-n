using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EnumEstado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Actualiza los valores de texto a los valores numéricos del enum
            migrationBuilder.Sql(@"
                UPDATE [Facturas] SET [Estado] = '0' WHERE [Estado] = 'Emitida';
                UPDATE [Facturas] SET [Estado] = '1' WHERE [Estado] = 'Generada';
                UPDATE [Facturas] SET [Estado] = '2' WHERE [Estado] = 'Firmada';
                UPDATE [Facturas] SET [Estado] = '3' WHERE [Estado] = 'Autorizada';
                UPDATE [Facturas] SET [Estado] = '4' WHERE [Estado] = 'Borrador';
            ");

            // 2. Cambia el tipo de columna de nvarchar a int
            migrationBuilder.AlterColumn<int>(
                name: "Estado",
                table: "Facturas",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Estado",
                table: "Facturas",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
