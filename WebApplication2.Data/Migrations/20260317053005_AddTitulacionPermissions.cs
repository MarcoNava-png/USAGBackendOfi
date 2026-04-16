using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication2.Data.Migrations
{
    public partial class AddTitulacionPermissions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                INSERT INTO Permissions (Code, Name, Description, Module, IsActive, CreatedAt) VALUES
                ('titulaciondirecta.view', 'Ver Titulación Directa', 'Permite visualizar el módulo de titulación directa', 'TitulacionDirecta', 1, GETDATE()),
                ('titulaciondirecta.manage', 'Gestionar Titulación Directa', 'Permite crear, editar y gestionar titulaciones directas', 'TitulacionDirecta', 1, GETDATE()),
                ('titulacionescolarizada.view', 'Ver Titulación Escolarizada', 'Permite visualizar el módulo de titulación escolarizada', 'TitulacionEscolarizada', 1, GETDATE()),
                ('titulacionescolarizada.manage', 'Gestionar Titulación Escolarizada', 'Permite crear, editar y gestionar titulaciones escolarizadas', 'TitulacionEscolarizada', 1, GETDATE());
            ");

            migrationBuilder.Sql(@"
                DECLARE @adminRoleId NVARCHAR(450) = (SELECT Id FROM AspNetRoles WHERE Name = 'admin');
                DECLARE @directorRoleId NVARCHAR(450) = (SELECT Id FROM AspNetRoles WHERE Name = 'director');
                DECLARE @ceRoleId NVARCHAR(450) = (SELECT Id FROM AspNetRoles WHERE Name = 'controlescolar');

                DECLARE @tdView INT = (SELECT IdPermission FROM Permissions WHERE Code = 'titulaciondirecta.view');
                DECLARE @tdManage INT = (SELECT IdPermission FROM Permissions WHERE Code = 'titulaciondirecta.manage');
                DECLARE @teView INT = (SELECT IdPermission FROM Permissions WHERE Code = 'titulacionescolarizada.view');
                DECLARE @teManage INT = (SELECT IdPermission FROM Permissions WHERE Code = 'titulacionescolarizada.manage');

                INSERT INTO RolePermissions (RoleId, PermissionId, CanView, CanCreate, CanEdit, CanDelete, AssignedAt, AssignedBy) VALUES
                (@adminRoleId, @tdView, 1, 1, 1, 1, GETDATE(), 'system'),
                (@adminRoleId, @tdManage, 1, 1, 1, 1, GETDATE(), 'system'),
                (@adminRoleId, @teView, 1, 1, 1, 1, GETDATE(), 'system'),
                (@adminRoleId, @teManage, 1, 1, 1, 1, GETDATE(), 'system'),
                (@directorRoleId, @tdView, 1, 1, 1, 0, GETDATE(), 'system'),
                (@directorRoleId, @tdManage, 1, 1, 1, 0, GETDATE(), 'system'),
                (@directorRoleId, @teView, 1, 1, 1, 0, GETDATE(), 'system'),
                (@directorRoleId, @teManage, 1, 1, 1, 0, GETDATE(), 'system'),
                (@ceRoleId, @tdView, 1, 1, 1, 0, GETDATE(), 'system'),
                (@ceRoleId, @tdManage, 1, 1, 1, 0, GETDATE(), 'system'),
                (@ceRoleId, @teView, 1, 1, 1, 0, GETDATE(), 'system'),
                (@ceRoleId, @teManage, 1, 1, 1, 0, GETDATE(), 'system');
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DELETE FROM RolePermissions WHERE PermissionId IN (SELECT IdPermission FROM Permissions WHERE Module IN ('TitulacionDirecta', 'TitulacionEscolarizada'));
                DELETE FROM Permissions WHERE Module IN ('TitulacionDirecta', 'TitulacionEscolarizada');
            ");
        }
    }
}
