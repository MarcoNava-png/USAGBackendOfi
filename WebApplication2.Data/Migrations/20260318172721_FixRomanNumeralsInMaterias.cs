using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication2.Data.Migrations
{
    public partial class FixRomanNumeralsInMaterias : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE Materia SET Nombre = REPLACE(Nombre, ' Viii', ' VIII') WHERE Nombre LIKE '% Viii' OR Nombre LIKE '% Viii %'");
            migrationBuilder.Sql("UPDATE Materia SET Nombre = REPLACE(Nombre, ' Vii', ' VII') WHERE Nombre LIKE '% Vii' OR Nombre LIKE '% Vii %'");
            migrationBuilder.Sql("UPDATE Materia SET Nombre = REPLACE(Nombre, ' Iii', ' III') WHERE Nombre LIKE '% Iii' OR Nombre LIKE '% Iii %'");
            migrationBuilder.Sql("UPDATE Materia SET Nombre = REPLACE(Nombre, ' Iv', ' IV') WHERE Nombre LIKE '% Iv' OR Nombre LIKE '% Iv %'");
            migrationBuilder.Sql("UPDATE Materia SET Nombre = REPLACE(Nombre, ' Vi', ' VI') WHERE (Nombre LIKE '% Vi' OR Nombre LIKE '% Vi %') AND Nombre NOT LIKE '% VII%' AND Nombre NOT LIKE '% VIII%'");
            migrationBuilder.Sql("UPDATE Materia SET Nombre = REPLACE(Nombre, ' Ii', ' II') WHERE (Nombre LIKE '% Ii' OR Nombre LIKE '% Ii %') AND Nombre NOT LIKE '% III%' AND Nombre NOT LIKE '% IV%'");

            migrationBuilder.Sql("UPDATE Materia SET Clave = UPPER(Clave) WHERE Clave IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
