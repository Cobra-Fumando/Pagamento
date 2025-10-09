using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pic.Migrations
{
    /// <inheritdoc />
    public partial class UsuarioName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Data",
                table: "Transacaos");

            migrationBuilder.AddColumn<string>(
                name: "UsuarioName",
                table: "Transacaos",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UsuarioName",
                table: "Transacaos");

            migrationBuilder.AddColumn<DateTime>(
                name: "Data",
                table: "Transacaos",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
