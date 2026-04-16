using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication2.Data.Migrations
{
    public partial class FixSolicitudBajaNombres : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE sb SET sb.SolicitadoPor = ISNULL(u.Nombres + ' ' + ISNULL(u.Apellidos,''), sb.SolicitadoPor)
                FROM SolicitudesBaja sb
                JOIN AspNetUsers u ON sb.SolicitadoPor = u.Id;

                UPDATE sb SET sb.AutorizadoPor = ISNULL(u.Nombres + ' ' + ISNULL(u.Apellidos,''), sb.AutorizadoPor)
                FROM SolicitudesBaja sb
                JOIN AspNetUsers u ON sb.AutorizadoPor = u.Id;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
