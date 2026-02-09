using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication2.Data.Migrations
{
    /// <inheritdoc />
    public partial class aspirante_add_periodoacademico_field : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UQ__Turno__E8181E11A9929118",
                table: "Turno");

            migrationBuilder.DropIndex(
                name: "UQ__Profesor__82F7575B30F93488",
                table: "Profesor");

            migrationBuilder.DropIndex(
                name: "UQ_PlanEstudios",
                table: "PlanEstudios");

            migrationBuilder.DropIndex(
                name: "UQ__PeriodoA__E8181E117466779A",
                table: "PeriodoAcademico");

            migrationBuilder.DropIndex(
                name: "UQ_Periodicidad",
                table: "Periodicidad");

            migrationBuilder.DropIndex(
                name: "UQ_NivelEducativo",
                table: "NivelEducativo");

            migrationBuilder.DropIndex(
                name: "UQ__Materia__E8181E1169244C5A",
                table: "Materia");

            migrationBuilder.DropIndex(
                name: "UQ_Genero",
                table: "Genero");

            migrationBuilder.DropIndex(
                name: "UQ__Estudian__0FB9FB4F890AE66D",
                table: "Estudiante");

            migrationBuilder.DropIndex(
                name: "UQ_EstadoCivil",
                table: "EstadoCivil");

            migrationBuilder.DropIndex(
                name: "UQ__DiaSeman__75E3EFCF34622C9D",
                table: "DiaSemana");

            migrationBuilder.DropIndex(
                name: "UQ__Convenio__A6197EE9ADAF6A0B",
                table: "Convenio");

            migrationBuilder.DropIndex(
                name: "UQ_Campus_Clave",
                table: "Campus");

            migrationBuilder.DropIndex(
                name: "UQ_Campus_Nombre",
                table: "Campus");

            migrationBuilder.DropIndex(
                name: "UQ_AspiranteEstatus",
                table: "AspiranteEstatus");

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Turno",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Clave",
                table: "Turno",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "NoEmpleado",
                table: "Profesor",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "ClavePlanEstudios",
                table: "PlanEstudios",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "PeriodoAcademico",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Clave",
                table: "PeriodoAcademico",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "DescPeriodicidad",
                table: "Periodicidad",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "DescNivelEducativo",
                table: "NivelEducativo",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Municipios",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "DescMedio",
                table: "MedioContacto",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Materia",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<string>(
                name: "Clave",
                table: "Materia",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "Estado",
                table: "Inscripcion",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                defaultValue: "Inscrito",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "Inscrito");

            migrationBuilder.AlterColumn<string>(
                name: "DescGenero",
                table: "Genero",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "Matricula",
                table: "Estudiante",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Estados",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Abreviatura",
                table: "Estados",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "DescEstadoCivil",
                table: "EstadoCivil",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "DiaSemana",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "TipoBeneficio",
                table: "Convenio",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Convenio",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(120)",
                oldMaxLength: 120);

            migrationBuilder.AlterColumn<string>(
                name: "ClaveConvenio",
                table: "Convenio",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "Codigo",
                table: "CodigosPostales",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Asentamiento",
                table: "CodigosPostales",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Campus",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(120)",
                oldMaxLength: 120);

            migrationBuilder.AlterColumn<string>(
                name: "ClaveCampus",
                table: "Campus",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "DescEstatus",
                table: "AspiranteEstatus",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Estatus",
                table: "AspiranteConvenio",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                defaultValue: "Pendiente",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "Pendiente");

            migrationBuilder.CreateIndex(
                name: "UQ__Turno__E8181E11A9929118",
                table: "Turno",
                column: "Clave",
                unique: true,
                filter: "[Clave] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UQ__Profesor__82F7575B30F93488",
                table: "Profesor",
                column: "NoEmpleado",
                unique: true,
                filter: "[NoEmpleado] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UQ_PlanEstudios",
                table: "PlanEstudios",
                column: "ClavePlanEstudios",
                unique: true,
                filter: "[ClavePlanEstudios] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UQ__PeriodoA__E8181E117466779A",
                table: "PeriodoAcademico",
                column: "Clave",
                unique: true,
                filter: "[Clave] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UQ_Periodicidad",
                table: "Periodicidad",
                column: "DescPeriodicidad",
                unique: true,
                filter: "[DescPeriodicidad] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UQ_NivelEducativo",
                table: "NivelEducativo",
                column: "DescNivelEducativo",
                unique: true,
                filter: "[DescNivelEducativo] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UQ__Materia__E8181E1169244C5A",
                table: "Materia",
                column: "Clave",
                unique: true,
                filter: "[Clave] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UQ_Genero",
                table: "Genero",
                column: "DescGenero",
                unique: true,
                filter: "[DescGenero] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UQ__Estudian__0FB9FB4F890AE66D",
                table: "Estudiante",
                column: "Matricula",
                unique: true,
                filter: "[Matricula] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UQ_EstadoCivil",
                table: "EstadoCivil",
                column: "DescEstadoCivil",
                unique: true,
                filter: "[DescEstadoCivil] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UQ__DiaSeman__75E3EFCF34622C9D",
                table: "DiaSemana",
                column: "Nombre",
                unique: true,
                filter: "[Nombre] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UQ__Convenio__A6197EE9ADAF6A0B",
                table: "Convenio",
                column: "ClaveConvenio",
                unique: true,
                filter: "[ClaveConvenio] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UQ_Campus_Clave",
                table: "Campus",
                column: "ClaveCampus",
                unique: true,
                filter: "[ClaveCampus] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UQ_Campus_Nombre",
                table: "Campus",
                column: "Nombre",
                unique: true,
                filter: "[Nombre] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UQ_AspiranteEstatus",
                table: "AspiranteEstatus",
                column: "DescEstatus",
                unique: true,
                filter: "[DescEstatus] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UQ__Turno__E8181E11A9929118",
                table: "Turno");

            migrationBuilder.DropIndex(
                name: "UQ__Profesor__82F7575B30F93488",
                table: "Profesor");

            migrationBuilder.DropIndex(
                name: "UQ_PlanEstudios",
                table: "PlanEstudios");

            migrationBuilder.DropIndex(
                name: "UQ__PeriodoA__E8181E117466779A",
                table: "PeriodoAcademico");

            migrationBuilder.DropIndex(
                name: "UQ_Periodicidad",
                table: "Periodicidad");

            migrationBuilder.DropIndex(
                name: "UQ_NivelEducativo",
                table: "NivelEducativo");

            migrationBuilder.DropIndex(
                name: "UQ__Materia__E8181E1169244C5A",
                table: "Materia");

            migrationBuilder.DropIndex(
                name: "UQ_Genero",
                table: "Genero");

            migrationBuilder.DropIndex(
                name: "UQ__Estudian__0FB9FB4F890AE66D",
                table: "Estudiante");

            migrationBuilder.DropIndex(
                name: "UQ_EstadoCivil",
                table: "EstadoCivil");

            migrationBuilder.DropIndex(
                name: "UQ__DiaSeman__75E3EFCF34622C9D",
                table: "DiaSemana");

            migrationBuilder.DropIndex(
                name: "UQ__Convenio__A6197EE9ADAF6A0B",
                table: "Convenio");

            migrationBuilder.DropIndex(
                name: "UQ_Campus_Clave",
                table: "Campus");

            migrationBuilder.DropIndex(
                name: "UQ_Campus_Nombre",
                table: "Campus");

            migrationBuilder.DropIndex(
                name: "UQ_AspiranteEstatus",
                table: "AspiranteEstatus");

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Turno",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Clave",
                table: "Turno",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NoEmpleado",
                table: "Profesor",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ClavePlanEstudios",
                table: "PlanEstudios",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "PeriodoAcademico",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Clave",
                table: "PeriodoAcademico",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DescPeriodicidad",
                table: "Periodicidad",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DescNivelEducativo",
                table: "NivelEducativo",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Municipios",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DescMedio",
                table: "MedioContacto",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Materia",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Clave",
                table: "Materia",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Estado",
                table: "Inscripcion",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Inscrito",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true,
                oldDefaultValue: "Inscrito");

            migrationBuilder.AlterColumn<string>(
                name: "DescGenero",
                table: "Genero",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Matricula",
                table: "Estudiante",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Estados",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Abreviatura",
                table: "Estados",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DescEstadoCivil",
                table: "EstadoCivil",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "DiaSemana",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TipoBeneficio",
                table: "Convenio",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Convenio",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(120)",
                oldMaxLength: 120,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ClaveConvenio",
                table: "Convenio",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Codigo",
                table: "CodigosPostales",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Asentamiento",
                table: "CodigosPostales",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Campus",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(120)",
                oldMaxLength: 120,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ClaveCampus",
                table: "Campus",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DescEstatus",
                table: "AspiranteEstatus",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Estatus",
                table: "AspiranteConvenio",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Pendiente",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true,
                oldDefaultValue: "Pendiente");

            migrationBuilder.CreateIndex(
                name: "UQ__Turno__E8181E11A9929118",
                table: "Turno",
                column: "Clave",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__Profesor__82F7575B30F93488",
                table: "Profesor",
                column: "NoEmpleado",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_PlanEstudios",
                table: "PlanEstudios",
                column: "ClavePlanEstudios",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__PeriodoA__E8181E117466779A",
                table: "PeriodoAcademico",
                column: "Clave",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Periodicidad",
                table: "Periodicidad",
                column: "DescPeriodicidad",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_NivelEducativo",
                table: "NivelEducativo",
                column: "DescNivelEducativo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__Materia__E8181E1169244C5A",
                table: "Materia",
                column: "Clave",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Genero",
                table: "Genero",
                column: "DescGenero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__Estudian__0FB9FB4F890AE66D",
                table: "Estudiante",
                column: "Matricula",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_EstadoCivil",
                table: "EstadoCivil",
                column: "DescEstadoCivil",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__DiaSeman__75E3EFCF34622C9D",
                table: "DiaSemana",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__Convenio__A6197EE9ADAF6A0B",
                table: "Convenio",
                column: "ClaveConvenio",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Campus_Clave",
                table: "Campus",
                column: "ClaveCampus",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Campus_Nombre",
                table: "Campus",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_AspiranteEstatus",
                table: "AspiranteEstatus",
                column: "DescEstatus",
                unique: true);
        }
    }
}
