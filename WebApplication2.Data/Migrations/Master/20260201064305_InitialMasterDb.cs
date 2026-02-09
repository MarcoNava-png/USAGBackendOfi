using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WebApplication2.Data.Migrations.Master
{
    /// <inheritdoc />
    public partial class InitialMasterDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlanLicencia",
                columns: table => new
                {
                    IdPlanLicencia = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PrecioMensual = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PrecioAnual = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MaxEstudiantes = table.Column<int>(type: "int", nullable: false),
                    MaxUsuarios = table.Column<int>(type: "int", nullable: false),
                    MaxCampus = table.Column<int>(type: "int", nullable: false),
                    IncluyeSoporte = table.Column<bool>(type: "bit", nullable: false),
                    IncluyeReportes = table.Column<bool>(type: "bit", nullable: false),
                    IncluyeAPI = table.Column<bool>(type: "bit", nullable: false),
                    IncluyeFacturacion = table.Column<bool>(type: "bit", nullable: false),
                    Caracteristicas = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanLicencia", x => x.IdPlanLicencia);
                });

            migrationBuilder.CreateTable(
                name: "SuperAdmin",
                columns: table => new
                {
                    IdSuperAdmin = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NombreCompleto = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AccesoTotal = table.Column<bool>(type: "bit", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastLoginIP = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SuperAdmin", x => x.IdSuperAdmin);
                });

            migrationBuilder.CreateTable(
                name: "Tenant",
                columns: table => new
                {
                    IdTenant = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NombreCorto = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Subdominio = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DominioPersonalizado = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DatabaseName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ConnectionString = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LogoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ColorPrimario = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: true),
                    ColorSecundario = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: true),
                    Timezone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EmailContacto = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TelefonoContacto = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DireccionFiscal = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RFC = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: true),
                    IdPlanLicencia = table.Column<int>(type: "int", nullable: false),
                    FechaContratacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaVencimiento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MaximoEstudiantes = table.Column<int>(type: "int", nullable: false),
                    MaximoUsuarios = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastAccessAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenant", x => x.IdTenant);
                    table.ForeignKey(
                        name: "FK_Tenant_PlanLicencia_IdPlanLicencia",
                        column: x => x.IdPlanLicencia,
                        principalTable: "PlanLicencia",
                        principalColumn: "IdPlanLicencia",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SuperAdminTenant",
                columns: table => new
                {
                    IdSuperAdminTenant = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdSuperAdmin = table.Column<int>(type: "int", nullable: false),
                    IdTenant = table.Column<int>(type: "int", nullable: false),
                    Rol = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AsignadoEn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SuperAdminTenant", x => x.IdSuperAdminTenant);
                    table.ForeignKey(
                        name: "FK_SuperAdminTenant_SuperAdmin_IdSuperAdmin",
                        column: x => x.IdSuperAdmin,
                        principalTable: "SuperAdmin",
                        principalColumn: "IdSuperAdmin",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SuperAdminTenant_Tenant_IdTenant",
                        column: x => x.IdTenant,
                        principalTable: "Tenant",
                        principalColumn: "IdTenant",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TenantAuditLog",
                columns: table => new
                {
                    IdLog = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdTenant = table.Column<int>(type: "int", nullable: true),
                    TenantCodigo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IdSuperAdmin = table.Column<int>(type: "int", nullable: true),
                    Accion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Detalles = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantAuditLog", x => x.IdLog);
                    table.ForeignKey(
                        name: "FK_TenantAuditLog_SuperAdmin_IdSuperAdmin",
                        column: x => x.IdSuperAdmin,
                        principalTable: "SuperAdmin",
                        principalColumn: "IdSuperAdmin");
                    table.ForeignKey(
                        name: "FK_TenantAuditLog_Tenant_IdTenant",
                        column: x => x.IdTenant,
                        principalTable: "Tenant",
                        principalColumn: "IdTenant");
                });

            migrationBuilder.InsertData(
                table: "PlanLicencia",
                columns: new[] { "IdPlanLicencia", "Activo", "Caracteristicas", "Codigo", "Descripcion", "IncluyeAPI", "IncluyeFacturacion", "IncluyeReportes", "IncluyeSoporte", "MaxCampus", "MaxEstudiantes", "MaxUsuarios", "Nombre", "Orden", "PrecioAnual", "PrecioMensual" },
                values: new object[,]
                {
                    { 1, true, null, "BASIC", "Plan básico para escuelas pequeñas", false, false, false, false, 1, 200, 5, "Básico", 1, 25000m, 2500m },
                    { 2, true, null, "PRO", "Plan profesional para escuelas medianas", false, false, true, true, 3, 1000, 20, "Profesional", 2, 50000m, 5000m },
                    { 3, true, null, "ENTERPRISE", "Plan enterprise para universidades y grandes instituciones", true, true, true, true, 10, 50000, 100, "Enterprise", 3, 150000m, 15000m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlanLicencia_Codigo",
                table: "PlanLicencia",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SuperAdmin_Email",
                table: "SuperAdmin",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SuperAdminTenant_IdSuperAdmin_IdTenant",
                table: "SuperAdminTenant",
                columns: new[] { "IdSuperAdmin", "IdTenant" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SuperAdminTenant_IdTenant",
                table: "SuperAdminTenant",
                column: "IdTenant");

            migrationBuilder.CreateIndex(
                name: "IX_Tenant_Codigo",
                table: "Tenant",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tenant_DatabaseName",
                table: "Tenant",
                column: "DatabaseName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tenant_IdPlanLicencia",
                table: "Tenant",
                column: "IdPlanLicencia");

            migrationBuilder.CreateIndex(
                name: "IX_Tenant_Status",
                table: "Tenant",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Tenant_Subdominio",
                table: "Tenant",
                column: "Subdominio",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TenantAuditLog_Accion",
                table: "TenantAuditLog",
                column: "Accion");

            migrationBuilder.CreateIndex(
                name: "IX_TenantAuditLog_IdSuperAdmin",
                table: "TenantAuditLog",
                column: "IdSuperAdmin");

            migrationBuilder.CreateIndex(
                name: "IX_TenantAuditLog_IdTenant",
                table: "TenantAuditLog",
                column: "IdTenant");

            migrationBuilder.CreateIndex(
                name: "IX_TenantAuditLog_Timestamp",
                table: "TenantAuditLog",
                column: "Timestamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SuperAdminTenant");

            migrationBuilder.DropTable(
                name: "TenantAuditLog");

            migrationBuilder.DropTable(
                name: "SuperAdmin");

            migrationBuilder.DropTable(
                name: "Tenant");

            migrationBuilder.DropTable(
                name: "PlanLicencia");
        }
    }
}
