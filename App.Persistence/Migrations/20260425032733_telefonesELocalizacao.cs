using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace App.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class telefonesELocalizacao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Localizacao",
                table: "Parametros",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TelefonePrincipal",
                table: "Parametros",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TelefoneSecundario",
                table: "Parametros",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Localizacao",
                table: "Parametros");

            migrationBuilder.DropColumn(
                name: "TelefonePrincipal",
                table: "Parametros");

            migrationBuilder.DropColumn(
                name: "TelefoneSecundario",
                table: "Parametros");
        }
    }
}
