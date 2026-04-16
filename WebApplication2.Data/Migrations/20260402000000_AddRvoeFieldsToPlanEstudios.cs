using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication2.Data.Migrations
{
    public partial class AddRvoeFieldsToPlanEstudios : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaExpedicionRvoe",
                table: "PlanEstudios",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdCarreraSEP",
                table: "PlanEstudios",
                type: "int",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "FechaExpedicionRvoe", table: "PlanEstudios");
            migrationBuilder.DropColumn(name: "IdCarreraSEP", table: "PlanEstudios");
        }
    }
}
