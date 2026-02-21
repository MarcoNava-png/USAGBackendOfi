using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication2.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTablasModalidadYModalidadPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Crear tabla Modalidad (modalidad de estudio)
            migrationBuilder.CreateTable(
                name: "Modalidad",
                columns: table => new
                {
                    IdModalidad = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DescModalidad = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Modalidad", x => x.IdModalidad);
                });

            // 2. Crear tabla ModalidadPlan (modalidad de plan de pago)
            migrationBuilder.CreateTable(
                name: "ModalidadPlan",
                columns: table => new
                {
                    IdModalidadPlan = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DescModalidadPlan = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModalidadPlan", x => x.IdModalidadPlan);
                });

            // 3. Indices unicos
            migrationBuilder.CreateIndex(
                name: "UQ_Modalidad",
                table: "Modalidad",
                column: "DescModalidad",
                unique: true,
                filter: "[DescModalidad] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UQ_ModalidadPlan",
                table: "ModalidadPlan",
                column: "DescModalidadPlan",
                unique: true,
                filter: "[DescModalidadPlan] IS NOT NULL");

            // 4. Insertar datos de catálogo
            migrationBuilder.Sql(@"
                SET IDENTITY_INSERT [Modalidad] ON;
                INSERT INTO [Modalidad] ([IdModalidad], [DescModalidad], [Activo]) VALUES (1, N'Presencial', 1);
                INSERT INTO [Modalidad] ([IdModalidad], [DescModalidad], [Activo]) VALUES (2, N'En línea', 1);
                INSERT INTO [Modalidad] ([IdModalidad], [DescModalidad], [Activo]) VALUES (3, N'Mixta', 1);
                SET IDENTITY_INSERT [Modalidad] OFF;

                SET IDENTITY_INSERT [ModalidadPlan] ON;
                INSERT INTO [ModalidadPlan] ([IdModalidadPlan], [DescModalidadPlan], [Activo]) VALUES (1, N'Con Título', 1);
                INSERT INTO [ModalidadPlan] ([IdModalidadPlan], [DescModalidadPlan], [Activo]) VALUES (2, N'Sin Título', 1);
                SET IDENTITY_INSERT [ModalidadPlan] OFF;
            ");

            // 5. Agregar columna IdModalidad a Aspirante (antes de eliminar Modalidad string)
            migrationBuilder.AddColumn<int>(
                name: "IdModalidad",
                table: "Aspirante",
                type: "int",
                nullable: true);

            // 6. Migrar datos: mapear string Modalidad → IdModalidad
            migrationBuilder.Sql(@"
                UPDATE [Aspirante] SET [IdModalidad] = 1 WHERE [Modalidad] = N'Presencial';
                UPDATE [Aspirante] SET [IdModalidad] = 2 WHERE [Modalidad] = N'En línea';
                UPDATE [Aspirante] SET [IdModalidad] = 3 WHERE [Modalidad] = N'Mixta';
            ");

            // 7. Eliminar columna string Modalidad de Aspirante
            migrationBuilder.DropColumn(
                name: "Modalidad",
                table: "Aspirante");

            // 8. PlanPago: agregar columna IdModalidadPlan temporal, migrar datos, eliminar columna vieja
            migrationBuilder.AddColumn<int>(
                name: "IdModalidadPlan",
                table: "PlanPago",
                type: "int",
                nullable: false,
                defaultValue: 1);

            // Migrar datos del enum: CON_TITULO (0) → 1, SIN_TITULO (1) → 2
            migrationBuilder.Sql(@"
                UPDATE [PlanPago] SET [IdModalidadPlan] = [Modalidad] + 1;
            ");

            // Eliminar columna vieja del enum
            migrationBuilder.DropColumn(
                name: "Modalidad",
                table: "PlanPago");

            // 9. Crear indices y foreign keys
            migrationBuilder.CreateIndex(
                name: "IX_Aspirante_IdModalidad",
                table: "Aspirante",
                column: "IdModalidad");

            migrationBuilder.CreateIndex(
                name: "IX_PlanPago_IdModalidadPlan",
                table: "PlanPago",
                column: "IdModalidadPlan");

            migrationBuilder.CreateIndex(
                name: "IX_PlantillasCobro_IdModalidad",
                table: "PlantillasCobro",
                column: "IdModalidad");

            migrationBuilder.AddForeignKey(
                name: "FK_Aspirante_Modalidad",
                table: "Aspirante",
                column: "IdModalidad",
                principalTable: "Modalidad",
                principalColumn: "IdModalidad");

            migrationBuilder.AddForeignKey(
                name: "FK_PlanPago_ModalidadPlan",
                table: "PlanPago",
                column: "IdModalidadPlan",
                principalTable: "ModalidadPlan",
                principalColumn: "IdModalidadPlan",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PlantillaCobro_Modalidad",
                table: "PlantillasCobro",
                column: "IdModalidad",
                principalTable: "Modalidad",
                principalColumn: "IdModalidad",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Aspirante_Modalidad",
                table: "Aspirante");

            migrationBuilder.DropForeignKey(
                name: "FK_PlanPago_ModalidadPlan",
                table: "PlanPago");

            migrationBuilder.DropForeignKey(
                name: "FK_PlantillaCobro_Modalidad",
                table: "PlantillasCobro");

            migrationBuilder.DropIndex(
                name: "IX_PlantillasCobro_IdModalidad",
                table: "PlantillasCobro");

            migrationBuilder.DropIndex(
                name: "IX_PlanPago_IdModalidadPlan",
                table: "PlanPago");

            migrationBuilder.DropIndex(
                name: "IX_Aspirante_IdModalidad",
                table: "Aspirante");

            // Restaurar columna string Modalidad en Aspirante
            migrationBuilder.AddColumn<string>(
                name: "Modalidad",
                table: "Aspirante",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            // Migrar datos de vuelta
            migrationBuilder.Sql(@"
                UPDATE [Aspirante] SET [Modalidad] = N'Presencial' WHERE [IdModalidad] = 1;
                UPDATE [Aspirante] SET [Modalidad] = N'En línea' WHERE [IdModalidad] = 2;
                UPDATE [Aspirante] SET [Modalidad] = N'Mixta' WHERE [IdModalidad] = 3;
            ");

            migrationBuilder.DropColumn(
                name: "IdModalidad",
                table: "Aspirante");

            // Restaurar columna enum Modalidad en PlanPago
            migrationBuilder.AddColumn<int>(
                name: "Modalidad",
                table: "PlanPago",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(@"
                UPDATE [PlanPago] SET [Modalidad] = [IdModalidadPlan] - 1;
            ");

            migrationBuilder.DropColumn(
                name: "IdModalidadPlan",
                table: "PlanPago");

            migrationBuilder.DropTable(
                name: "Modalidad");

            migrationBuilder.DropTable(
                name: "ModalidadPlan");
        }
    }
}
