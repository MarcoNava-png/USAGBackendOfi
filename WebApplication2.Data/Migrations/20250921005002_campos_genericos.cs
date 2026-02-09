using System;
using Microsoft.EntityFrameworkCore.Migrations;
using WebApplication2.Core.Common;

#nullable disable

namespace WebApplication2.Data.Migrations
{
    /// <inheritdoc />
    public partial class campos_genericos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Profesor",
                type: "datetime2",
                nullable: false,
                defaultValue: DateTime.UtcNow);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Profesor",
                type: "nvarchar(max)",
                defaultValue: "Sistema",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Profesor",
                type: "int",
                nullable: false,
                defaultValue: StatusEnum.Activo);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Profesor",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Profesor",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "PlanEstudios",
                type: "datetime2",
                nullable: false,
                defaultValue: DateTime.UtcNow);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "PlanEstudios",
                type: "nvarchar(max)",
                defaultValue: "Sistema",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "PlanEstudios",
                type: "int",
                nullable: false,
                defaultValue: StatusEnum.Activo);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "PlanEstudios",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "PlanEstudios",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Persona",
                type: "datetime2",
                nullable: false,
                defaultValue: DateTime.UtcNow);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Persona",
                type: "nvarchar(max)",
                defaultValue: "Sistema",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Persona",
                type: "int",
                nullable: false,
                defaultValue: StatusEnum.Activo);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Persona",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Persona",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "PeriodoAcademico",
                type: "datetime2",
                nullable: false,
                defaultValue: DateTime.UtcNow);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "PeriodoAcademico",
                type: "nvarchar(max)",
                defaultValue: "Sistema",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "PeriodoAcademico",
                type: "int",
                nullable: false,
                defaultValue: StatusEnum.Activo);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "PeriodoAcademico",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "PeriodoAcademico",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "MedioContacto",
                type: "datetime2",
                nullable: false,
                defaultValue: DateTime.UtcNow);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "MedioContacto",
                type: "nvarchar(max)",
                defaultValue: "Sistema",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "MedioContacto",
                type: "int",
                nullable: false,
                defaultValue: StatusEnum.Activo);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "MedioContacto",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "MedioContacto",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "MateriaPlan",
                type: "datetime2",
                nullable: false,
                defaultValue: DateTime.UtcNow);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "MateriaPlan",
                type: "nvarchar(max)",
                defaultValue: "Sistema",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "MateriaPlan",
                type: "int",
                nullable: false,
                defaultValue: StatusEnum.Activo);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "MateriaPlan",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "MateriaPlan",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Materia",
                type: "datetime2",
                nullable: false,
                defaultValue: DateTime.UtcNow);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Materia",
                type: "nvarchar(max)",
                defaultValue: "Sistema",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Materia",
                type: "int",
                nullable: false,
                defaultValue: StatusEnum.Activo);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Materia",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Materia",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Inscripcion",
                type: "datetime2",
                nullable: false,
                defaultValue: DateTime.UtcNow);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Inscripcion",
                type: "nvarchar(max)",
                defaultValue: "Sistema",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Inscripcion",
                type: "int",
                nullable: false,
                defaultValue: StatusEnum.Activo);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Inscripcion",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Inscripcion",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Horario",
                type: "datetime2",
                nullable: false,
                defaultValue: DateTime.UtcNow);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Horario",
                type: "nvarchar(max)",
                defaultValue: "Sistema",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Horario",
                type: "int",
                nullable: false,
                defaultValue: StatusEnum.Activo);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Horario",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Horario",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "GrupoMateria",
                type: "datetime2",
                nullable: false,
                defaultValue: DateTime.UtcNow);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "GrupoMateria",
                type: "nvarchar(max)",
                defaultValue: "Sistema",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "GrupoMateria",
                type: "int",
                nullable: false,
                defaultValue: StatusEnum.Activo);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "GrupoMateria",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "GrupoMateria",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Grupo",
                type: "datetime2",
                nullable: false,
                defaultValue: DateTime.UtcNow);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Grupo",
                type: "nvarchar(max)",
                defaultValue: "Sistema",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Grupo",
                type: "int",
                nullable: false,
                defaultValue: StatusEnum.Activo);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Grupo",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Grupo",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "EstudiantePlan",
                type: "datetime2",
                nullable: false,
                defaultValue: DateTime.UtcNow);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "EstudiantePlan",
                type: "nvarchar(max)",
                defaultValue: "Sistema",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "EstudiantePlan",
                type: "int",
                nullable: false,
                defaultValue: StatusEnum.Activo);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "EstudiantePlan",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "EstudiantePlan",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Estudiante",
                type: "datetime2",
                nullable: false,
                defaultValue: DateTime.UtcNow);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Estudiante",
                type: "nvarchar(max)",
                defaultValue: "Sistema",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Estudiante",
                type: "int",
                nullable: false,
                defaultValue: StatusEnum.Activo);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Estudiante",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Estudiante",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Direccion",
                type: "datetime2",
                nullable: false,
                defaultValue: DateTime.UtcNow);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Direccion",
                type: "nvarchar(max)",
                defaultValue: "Sistema",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Direccion",
                type: "int",
                nullable: false,
                defaultValue: StatusEnum.Activo);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Direccion",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Direccion",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "ConvenioAlcance",
                type: "datetime2",
                nullable: false,
                defaultValue: DateTime.UtcNow);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "ConvenioAlcance",
                type: "nvarchar(max)",
                defaultValue: "Sistema",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "ConvenioAlcance",
                type: "int",
                nullable: false,
                defaultValue: StatusEnum.Activo);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "ConvenioAlcance",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "ConvenioAlcance",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Convenio",
                type: "datetime2",
                nullable: false,
                defaultValue: DateTime.UtcNow);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Convenio",
                type: "nvarchar(max)",
                defaultValue: "Sistema",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Convenio",
                type: "int",
                nullable: false,
                defaultValue: StatusEnum.Activo);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Convenio",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Convenio",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Campus",
                type: "datetime2",
                nullable: false,
                defaultValue: DateTime.UtcNow);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Campus",
                type: "nvarchar(max)",
                defaultValue: "Sistema",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Campus",
                type: "int",
                nullable: false,
                defaultValue: StatusEnum.Activo);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Campus",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Campus",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "AspiranteEstatus",
                type: "datetime2",
                nullable: false,
                defaultValue: DateTime.UtcNow);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "AspiranteEstatus",
                type: "nvarchar(max)",
                defaultValue: "Sistema",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "AspiranteEstatus",
                type: "int",
                nullable: false,
                defaultValue: StatusEnum.Activo);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "AspiranteEstatus",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "AspiranteEstatus",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "AspiranteConvenio",
                type: "datetime2",
                nullable: false,
                defaultValue: DateTime.UtcNow);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "AspiranteConvenio",
                type: "nvarchar(max)",
                defaultValue: "Sistema",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "AspiranteConvenio",
                type: "int",
                nullable: false,
                defaultValue: StatusEnum.Activo);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "AspiranteConvenio",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "AspiranteConvenio",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Aspirante",
                type: "datetime2",
                nullable: false,
                defaultValue: DateTime.UtcNow);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Aspirante",
                type: "nvarchar(max)",
                defaultValue: "Sistema",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Aspirante",
                type: "int",
                nullable: false,
                defaultValue: StatusEnum.Activo);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Aspirante",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Aspirante",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Profesor");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Profesor");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Profesor");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Profesor");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Profesor");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "PlanEstudios");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "PlanEstudios");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "PlanEstudios");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "PlanEstudios");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "PlanEstudios");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Persona");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Persona");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Persona");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Persona");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Persona");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "PeriodoAcademico");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "PeriodoAcademico");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "PeriodoAcademico");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "PeriodoAcademico");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "PeriodoAcademico");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "MedioContacto");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "MedioContacto");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "MedioContacto");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "MedioContacto");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "MedioContacto");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "MateriaPlan");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "MateriaPlan");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "MateriaPlan");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "MateriaPlan");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "MateriaPlan");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Materia");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Materia");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Materia");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Materia");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Materia");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Inscripcion");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Inscripcion");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Inscripcion");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Inscripcion");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Inscripcion");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Horario");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Horario");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Horario");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Horario");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Horario");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "GrupoMateria");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "GrupoMateria");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "GrupoMateria");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "GrupoMateria");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "GrupoMateria");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Grupo");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Grupo");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Grupo");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Grupo");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Grupo");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "EstudiantePlan");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "EstudiantePlan");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "EstudiantePlan");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "EstudiantePlan");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "EstudiantePlan");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Estudiante");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Estudiante");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Estudiante");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Estudiante");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Estudiante");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Direccion");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Direccion");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Direccion");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Direccion");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Direccion");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ConvenioAlcance");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "ConvenioAlcance");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "ConvenioAlcance");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ConvenioAlcance");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "ConvenioAlcance");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Convenio");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Convenio");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Convenio");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Convenio");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Convenio");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Campus");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Campus");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Campus");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Campus");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Campus");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "AspiranteEstatus");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "AspiranteEstatus");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "AspiranteEstatus");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "AspiranteEstatus");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "AspiranteEstatus");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "AspiranteConvenio");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "AspiranteConvenio");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "AspiranteConvenio");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "AspiranteConvenio");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "AspiranteConvenio");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Aspirante");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Aspirante");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Aspirante");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Aspirante");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Aspirante");
        }
    }
}
