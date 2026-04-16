using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication2.Data.Migrations
{
    public partial class DeleteEstudiante827AndResetAspirante36 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DELETE FROM EstudiantePlan WHERE IdEstudiante = 827;
                DELETE FROM Estudiante WHERE IdEstudiante = 827;

                DELETE FROM AspNetUserRoles WHERE UserId = '2df6bf9c-8394-4335-8fe0-e7dcec0344f6';
                DELETE FROM AspNetUserClaims WHERE UserId = '2df6bf9c-8394-4335-8fe0-e7dcec0344f6';
                DELETE FROM AspNetUserLogins WHERE UserId = '2df6bf9c-8394-4335-8fe0-e7dcec0344f6';
                DELETE FROM AspNetUserTokens WHERE UserId = '2df6bf9c-8394-4335-8fe0-e7dcec0344f6';
                DELETE FROM NotificacionesUsuario WHERE UsuarioDestinoId = '2df6bf9c-8394-4335-8fe0-e7dcec0344f6';
                DELETE FROM AspNetUsers WHERE Id = '2df6bf9c-8394-4335-8fe0-e7dcec0344f6';

                UPDATE Aspirante SET IdAspiranteEstatus = 2 WHERE IdAspirante = 36;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
