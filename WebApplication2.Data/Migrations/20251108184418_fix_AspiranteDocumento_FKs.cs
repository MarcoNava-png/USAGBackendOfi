using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication2.Data.Migrations
{
    /// <inheritdoc />
    public partial class fix_AspiranteDocumento_FKs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspiranteDocumento_Aspirante_AspiranteIdAspirante",
                table: "AspiranteDocumento");

            migrationBuilder.DropForeignKey(
                name: "FK_AspiranteDocumento_DocumentoRequisito_RequisitoIdDocumentoRequisito",
                table: "AspiranteDocumento");

            migrationBuilder.DropIndex(
                name: "IX_AspiranteDocumento_AspiranteIdAspirante",
                table: "AspiranteDocumento");

            migrationBuilder.DropIndex(
                name: "IX_AspiranteDocumento_RequisitoIdDocumentoRequisito",
                table: "AspiranteDocumento");

            migrationBuilder.DropColumn(
                name: "AspiranteIdAspirante",
                table: "AspiranteDocumento");

            migrationBuilder.DropColumn(
                name: "RequisitoIdDocumentoRequisito",
                table: "AspiranteDocumento");

            migrationBuilder.AlterColumn<string>(
                name: "Descripcion",
                table: "DocumentoRequisito",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Clave",
                table: "DocumentoRequisito",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "DocumentoRequisito",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "DocumentoRequisito",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "DocumentoRequisito",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "DocumentoRequisito",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "DocumentoRequisito",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UrlArchivo",
                table: "AspiranteDocumento",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Notas",
                table: "AspiranteDocumento",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "AspiranteDocumento",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "AspiranteDocumento",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "AspiranteDocumento",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "AspiranteDocumento",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "AspiranteDocumento",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentoRequisito_Clave",
                table: "DocumentoRequisito",
                column: "Clave",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspiranteDocumento_IdAspirante_IdDocumentoRequisito",
                table: "AspiranteDocumento",
                columns: new[] { "IdAspirante", "IdDocumentoRequisito" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspiranteDocumento_IdDocumentoRequisito",
                table: "AspiranteDocumento",
                column: "IdDocumentoRequisito");

            migrationBuilder.AddForeignKey(
                name: "FK_AspiranteDocumento_Aspirante_IdAspirante",
                table: "AspiranteDocumento",
                column: "IdAspirante",
                principalTable: "Aspirante",
                principalColumn: "IdAspirante",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspiranteDocumento_DocumentoRequisito_IdDocumentoRequisito",
                table: "AspiranteDocumento",
                column: "IdDocumentoRequisito",
                principalTable: "DocumentoRequisito",
                principalColumn: "IdDocumentoRequisito",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspiranteDocumento_Aspirante_IdAspirante",
                table: "AspiranteDocumento");

            migrationBuilder.DropForeignKey(
                name: "FK_AspiranteDocumento_DocumentoRequisito_IdDocumentoRequisito",
                table: "AspiranteDocumento");

            migrationBuilder.DropIndex(
                name: "IX_DocumentoRequisito_Clave",
                table: "DocumentoRequisito");

            migrationBuilder.DropIndex(
                name: "IX_AspiranteDocumento_IdAspirante_IdDocumentoRequisito",
                table: "AspiranteDocumento");

            migrationBuilder.DropIndex(
                name: "IX_AspiranteDocumento_IdDocumentoRequisito",
                table: "AspiranteDocumento");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "DocumentoRequisito");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "DocumentoRequisito");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "DocumentoRequisito");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "DocumentoRequisito");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "DocumentoRequisito");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "AspiranteDocumento");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "AspiranteDocumento");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "AspiranteDocumento");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "AspiranteDocumento");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "AspiranteDocumento");

            migrationBuilder.AlterColumn<string>(
                name: "Descripcion",
                table: "DocumentoRequisito",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Clave",
                table: "DocumentoRequisito",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "UrlArchivo",
                table: "AspiranteDocumento",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Notas",
                table: "AspiranteDocumento",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AspiranteIdAspirante",
                table: "AspiranteDocumento",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RequisitoIdDocumentoRequisito",
                table: "AspiranteDocumento",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspiranteDocumento_AspiranteIdAspirante",
                table: "AspiranteDocumento",
                column: "AspiranteIdAspirante");

            migrationBuilder.CreateIndex(
                name: "IX_AspiranteDocumento_RequisitoIdDocumentoRequisito",
                table: "AspiranteDocumento",
                column: "RequisitoIdDocumentoRequisito");

            migrationBuilder.AddForeignKey(
                name: "FK_AspiranteDocumento_Aspirante_AspiranteIdAspirante",
                table: "AspiranteDocumento",
                column: "AspiranteIdAspirante",
                principalTable: "Aspirante",
                principalColumn: "IdAspirante");

            migrationBuilder.AddForeignKey(
                name: "FK_AspiranteDocumento_DocumentoRequisito_RequisitoIdDocumentoRequisito",
                table: "AspiranteDocumento",
                column: "RequisitoIdDocumentoRequisito",
                principalTable: "DocumentoRequisito",
                principalColumn: "IdDocumentoRequisito");
        }
    }
}
