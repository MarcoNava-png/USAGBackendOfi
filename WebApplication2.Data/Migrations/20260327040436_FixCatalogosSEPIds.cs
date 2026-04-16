using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication2.Data.Migrations
{
    public partial class FixCatalogosSEPIds : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM CatalogoTipoPeriodoSEP");
            migrationBuilder.Sql(@"
                INSERT INTO CatalogoTipoPeriodoSEP (IdTipoPeriodo, Descripcion, Activo) VALUES
                ('91', 'Semestre', 1),
                ('92', 'Bimestre', 1),
                ('93', 'Cuatrimestre', 1),
                ('94', 'Tetramestre', 1),
                ('260', 'Trimestre', 1),
                ('261', 'Modular', 1),
                ('262', 'Anual', 1);
            ");

            migrationBuilder.Sql("DELETE FROM CatalogoTipoCertificacionSEP");
            migrationBuilder.Sql(@"
                INSERT INTO CatalogoTipoCertificacionSEP (IdTipoCertificacion, Descripcion, Activo) VALUES
                ('79', 'Total', 1),
                ('80', 'Parcial', 1);
            ");

            migrationBuilder.Sql("DELETE FROM CatalogoCargoSEP");
            migrationBuilder.Sql(@"
                INSERT INTO CatalogoCargoSEP (IdCargo, Descripcion, Activo) VALUES
                ('1', 'Director', 1),
                ('2', 'Subdirector', 1),
                ('3', 'Rector', 1),
                ('4', 'Vicerrector', 1),
                ('5', 'Responsable de Expedición', 1);
            ");

            migrationBuilder.Sql("DELETE FROM CatalogoObservacionSEP");
            migrationBuilder.Sql(@"
                INSERT INTO CatalogoObservacionSEP (IdObservacion, Descripcion, Activo) VALUES
                ('70', 'Equivalencia de Estudios', 1),
                ('71', 'Examen Extraordinario', 1),
                ('72', 'Examen a Título de Suficiencia', 1),
                ('73', 'Curso de Verano', 1),
                ('74', 'Recursamiento', 1),
                ('75', 'Reingreso', 1),
                ('76', 'Acuerdo Regularización', 1),
                ('77', 'Con Cambio en el Acuerdo de RVOE', 1),
                ('78', 'Revalidación de Estudios', 1),
                ('100', 'Normal / Ordinario', 1),
                ('101', 'Correspondencia de Asignatura por Plan', 1),
                ('102', 'Exento', 1),
                ('104', 'Curso de Regularización', 1),
                ('105', 'Intercambio Académico', 1),
                ('106', 'Examen de Última Materia', 1),
                ('107', 'Curso de Invierno', 1);
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'CatalogoNivelEstudiosSEP')
                CREATE TABLE CatalogoNivelEstudiosSEP (
                    Id INT IDENTITY(1,1) PRIMARY KEY,
                    IdNivelEstudios NVARCHAR(10) NOT NULL,
                    Descripcion NVARCHAR(200) NOT NULL,
                    Activo BIT NOT NULL DEFAULT 1
                );
                INSERT INTO CatalogoNivelEstudiosSEP (IdNivelEstudios, Descripcion, Activo) VALUES
                ('95', 'Doctorado', 1),
                ('85', 'Especialidad', 1),
                ('84', 'Técnico Superior Universitario', 1),
                ('83', 'Profesional Asociado', 1),
                ('82', 'Maestría', 1),
                ('81', 'Licenciatura', 1);
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'CatalogoGeneroSEP')
                CREATE TABLE CatalogoGeneroSEP (
                    Id INT IDENTITY(1,1) PRIMARY KEY,
                    IdGenero NVARCHAR(10) NOT NULL,
                    Descripcion NVARCHAR(50) NOT NULL,
                    Activo BIT NOT NULL DEFAULT 1
                );
                INSERT INTO CatalogoGeneroSEP (IdGenero, Descripcion, Activo) VALUES
                ('250', 'Mujer', 1),
                ('251', 'Hombre', 1);
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'CatalogoTipoAsignaturaSEP')
                CREATE TABLE CatalogoTipoAsignaturaSEP (
                    Id INT IDENTITY(1,1) PRIMARY KEY,
                    IdTipoAsignatura NVARCHAR(10) NOT NULL,
                    Descripcion NVARCHAR(100) NOT NULL,
                    Activo BIT NOT NULL DEFAULT 1
                );
                INSERT INTO CatalogoTipoAsignaturaSEP (IdTipoAsignatura, Descripcion, Activo) VALUES
                ('263', 'Obligatoria', 1),
                ('264', 'Optativa', 1),
                ('265', 'Adicional', 1),
                ('266', 'Complementaria', 1);
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'CatalogoEntidadFederativaSEP')
                CREATE TABLE CatalogoEntidadFederativaSEP (
                    Id INT IDENTITY(1,1) PRIMARY KEY,
                    IdEntidadFederativa NVARCHAR(5) NOT NULL,
                    Nombre NVARCHAR(100) NOT NULL,
                    Activo BIT NOT NULL DEFAULT 1
                );
                INSERT INTO CatalogoEntidadFederativaSEP (IdEntidadFederativa, Nombre, Activo) VALUES
                ('01', 'Aguascalientes', 1), ('02', 'Baja California', 1), ('03', 'Baja California Sur', 1),
                ('04', 'Campeche', 1), ('05', 'Coahuila de Zaragoza', 1), ('06', 'Colima', 1),
                ('07', 'Chiapas', 1), ('08', 'Chihuahua', 1), ('09', 'Ciudad de México', 1),
                ('10', 'Durango', 1), ('11', 'Guanajuato', 1), ('12', 'Guerrero', 1),
                ('13', 'Hidalgo', 1), ('14', 'Jalisco', 1), ('15', 'México', 1),
                ('16', 'Michoacán de Ocampo', 1), ('17', 'Morelos', 1), ('18', 'Nayarit', 1),
                ('19', 'Nuevo León', 1), ('20', 'Oaxaca', 1), ('21', 'Puebla', 1),
                ('22', 'Querétaro', 1), ('23', 'Quintana Roo', 1), ('24', 'San Luis Potosí', 1),
                ('25', 'Sinaloa', 1), ('26', 'Sonora', 1), ('27', 'Tabasco', 1),
                ('28', 'Tamaulipas', 1), ('29', 'Tlaxcala', 1), ('30', 'Veracruz de Ignacio de la Llave', 1),
                ('31', 'Yucatán', 1), ('32', 'Zacatecas', 1);
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE IF EXISTS CatalogoNivelEstudiosSEP");
            migrationBuilder.Sql("DROP TABLE IF EXISTS CatalogoGeneroSEP");
            migrationBuilder.Sql("DROP TABLE IF EXISTS CatalogoTipoAsignaturaSEP");
            migrationBuilder.Sql("DROP TABLE IF EXISTS CatalogoEntidadFederativaSEP");
        }
    }
}
