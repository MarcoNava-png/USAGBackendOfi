using Microsoft.EntityFrameworkCore.Migrations;
using WebApplication2.Core.Enums;

#nullable disable

namespace WebApplication2.Data.Migrations
{
    /// <inheritdoc />
    public partial class add_baseentity_to_aspirantebitacoraseguimiento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "AspiranteBitacoraSeguimiento",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "AspiranteBitacoraSeguimiento",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "AspiranteBitacoraSeguimiento",
                type: "int",
                nullable: false,
                defaultValue: StatusEnum.Active);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "AspiranteBitacoraSeguimiento",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "AspiranteBitacoraSeguimiento",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "AspiranteBitacoraSeguimiento");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "AspiranteBitacoraSeguimiento");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "AspiranteBitacoraSeguimiento");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "AspiranteBitacoraSeguimiento");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "AspiranteBitacoraSeguimiento");
        }
    }
}
