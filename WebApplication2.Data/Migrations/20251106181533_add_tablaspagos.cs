using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication2.Data.Migrations
{
    /// <inheritdoc />
    public partial class add_tablaspagos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConceptoPago",
                columns: table => new
                {
                    IdConceptoPago = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Clave = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    AplicaA = table.Column<int>(type: "int", nullable: false),
                    EsObligatorio = table.Column<bool>(type: "bit", nullable: false),
                    PeriodicidadMeses = table.Column<byte>(type: "tinyint", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConceptoPago", x => x.IdConceptoPago);
                });

            migrationBuilder.CreateTable(
                name: "MedioPago",
                columns: table => new
                {
                    IdMedioPago = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Clave = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedioPago", x => x.IdMedioPago);
                });

            migrationBuilder.CreateTable(
                name: "PlanPago",
                columns: table => new
                {
                    IdPlanPago = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdPeriodicidad = table.Column<int>(type: "int", nullable: false),
                    IdPeriodoAcademico = table.Column<int>(type: "int", nullable: false),
                    IdPlanEstudios = table.Column<int>(type: "int", nullable: true),
                    Modalidad = table.Column<int>(type: "int", nullable: false),
                    Moneda = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    VigenciaDesde = table.Column<DateOnly>(type: "date", nullable: false),
                    VigenciaHasta = table.Column<DateOnly>(type: "date", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanPago", x => x.IdPlanPago);
                });

            migrationBuilder.CreateTable(
                name: "RecargoPolitica",
                columns: table => new
                {
                    IdRecargoPolitica = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdCampus = table.Column<int>(type: "int", nullable: true),
                    IdPlanEstudios = table.Column<int>(type: "int", nullable: true),
                    TasaDiaria = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: false),
                    DiaInicioGracia = table.Column<byte>(type: "tinyint", nullable: false),
                    DiaFinGracia = table.Column<byte>(type: "tinyint", nullable: false),
                    RecargoMinimo = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: true),
                    RecargoMaximo = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: true),
                    TopeDiasMora = table.Column<int>(type: "int", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecargoPolitica", x => x.IdRecargoPolitica);
                });

            migrationBuilder.CreateTable(
                name: "Recibo",
                columns: table => new
                {
                    IdRecibo = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Folio = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdAspirante = table.Column<int>(type: "int", nullable: true),
                    IdEstudiante = table.Column<int>(type: "int", nullable: true),
                    IdPeriodoAcademico = table.Column<int>(type: "int", nullable: true),
                    FechaEmision = table.Column<DateOnly>(type: "date", nullable: false),
                    FechaVencimiento = table.Column<DateOnly>(type: "date", nullable: false),
                    Estatus = table.Column<int>(type: "int", nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    Descuento = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    Recargos = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false, computedColumnSql: "ROUND([Subtotal]-[Descuento]+[Recargos],2)", stored: true),
                    Saldo = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    Notas = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recibo", x => x.IdRecibo);
                });

            migrationBuilder.CreateTable(
                name: "ConceptoPrecio",
                columns: table => new
                {
                    IdConceptoPrecio = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdConceptoPago = table.Column<int>(type: "int", nullable: false),
                    IdCampus = table.Column<int>(type: "int", nullable: true),
                    IdPlanEstudios = table.Column<int>(type: "int", nullable: true),
                    Moneda = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Importe = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    VigenciaDesde = table.Column<DateOnly>(type: "date", nullable: false),
                    VigenciaHasta = table.Column<DateOnly>(type: "date", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    ConceptoPagoIdConceptoPago = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConceptoPrecio", x => x.IdConceptoPrecio);
                    table.ForeignKey(
                        name: "FK_ConceptoPrecio_ConceptoPago_ConceptoPagoIdConceptoPago",
                        column: x => x.ConceptoPagoIdConceptoPago,
                        principalTable: "ConceptoPago",
                        principalColumn: "IdConceptoPago");
                });

            migrationBuilder.CreateTable(
                name: "Pago",
                columns: table => new
                {
                    IdPago = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FechaPagoUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IdMedioPago = table.Column<int>(type: "int", nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    Moneda = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Referencia = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notas = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Estatus = table.Column<int>(type: "int", nullable: false),
                    MedioPagoIdMedioPago = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pago", x => x.IdPago);
                    table.ForeignKey(
                        name: "FK_Pago_MedioPago_MedioPagoIdMedioPago",
                        column: x => x.MedioPagoIdMedioPago,
                        principalTable: "MedioPago",
                        principalColumn: "IdMedioPago");
                });

            migrationBuilder.CreateTable(
                name: "PlanPagoAsignacion",
                columns: table => new
                {
                    IdPlanPagoAsignacion = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdPlanPago = table.Column<int>(type: "int", nullable: false),
                    IdEstudiante = table.Column<int>(type: "int", nullable: false),
                    FechaAsignacionUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PlanPagoIdPlanPago = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanPagoAsignacion", x => x.IdPlanPagoAsignacion);
                    table.ForeignKey(
                        name: "FK_PlanPagoAsignacion_PlanPago_PlanPagoIdPlanPago",
                        column: x => x.PlanPagoIdPlanPago,
                        principalTable: "PlanPago",
                        principalColumn: "IdPlanPago");
                });

            migrationBuilder.CreateTable(
                name: "PlanPagoDetalle",
                columns: table => new
                {
                    IdPlanPagoDetalle = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdPlanPago = table.Column<int>(type: "int", nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    IdConceptoPago = table.Column<int>(type: "int", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Cantidad = table.Column<decimal>(type: "decimal(9,2)", precision: 9, scale: 2, nullable: false),
                    Importe = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    EsInscripcion = table.Column<bool>(type: "bit", nullable: false),
                    EsMensualidad = table.Column<bool>(type: "bit", nullable: false),
                    MesOffset = table.Column<int>(type: "int", nullable: false),
                    DiaPago = table.Column<byte>(type: "tinyint", nullable: true),
                    PintaInternet = table.Column<bool>(type: "bit", nullable: false),
                    PlanPagoIdPlanPago = table.Column<int>(type: "int", nullable: true),
                    ConceptoPagoIdConceptoPago = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanPagoDetalle", x => x.IdPlanPagoDetalle);
                    table.ForeignKey(
                        name: "FK_PlanPagoDetalle_ConceptoPago_ConceptoPagoIdConceptoPago",
                        column: x => x.ConceptoPagoIdConceptoPago,
                        principalTable: "ConceptoPago",
                        principalColumn: "IdConceptoPago");
                    table.ForeignKey(
                        name: "FK_PlanPagoDetalle_PlanPago_PlanPagoIdPlanPago",
                        column: x => x.PlanPagoIdPlanPago,
                        principalTable: "PlanPago",
                        principalColumn: "IdPlanPago");
                });

            migrationBuilder.CreateTable(
                name: "BitacoraRecibo",
                columns: table => new
                {
                    IdBitacora = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdRecibo = table.Column<long>(type: "bigint", nullable: false),
                    TipoRecibo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Usuario = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Accion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Origen = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notas = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReciboIdRecibo = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BitacoraRecibo", x => x.IdBitacora);
                    table.ForeignKey(
                        name: "FK_BitacoraRecibo_Recibo_ReciboIdRecibo",
                        column: x => x.ReciboIdRecibo,
                        principalTable: "Recibo",
                        principalColumn: "IdRecibo");
                });

            migrationBuilder.CreateTable(
                name: "LigaPago",
                columns: table => new
                {
                    IdLigaPago = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TipoRecibo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdRecibo = table.Column<long>(type: "bigint", nullable: false),
                    Folio = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaGeneracionUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaPrimeraVistaUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IPPrimeraVista = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReciboIdRecibo = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LigaPago", x => x.IdLigaPago);
                    table.ForeignKey(
                        name: "FK_LigaPago_Recibo_ReciboIdRecibo",
                        column: x => x.ReciboIdRecibo,
                        principalTable: "Recibo",
                        principalColumn: "IdRecibo");
                });

            migrationBuilder.CreateTable(
                name: "ReciboDetalle",
                columns: table => new
                {
                    IdReciboDetalle = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdRecibo = table.Column<long>(type: "bigint", nullable: false),
                    IdConceptoPago = table.Column<int>(type: "int", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Cantidad = table.Column<decimal>(type: "decimal(9,2)", precision: 9, scale: 2, nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    Importe = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false, computedColumnSql: "ROUND([Cantidad]*[PrecioUnitario],2)", stored: true),
                    RefTabla = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RefId = table.Column<long>(type: "bigint", nullable: true),
                    ReciboIdRecibo = table.Column<long>(type: "bigint", nullable: true),
                    ConceptoPagoIdConceptoPago = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReciboDetalle", x => x.IdReciboDetalle);
                    table.ForeignKey(
                        name: "FK_ReciboDetalle_ConceptoPago_ConceptoPagoIdConceptoPago",
                        column: x => x.ConceptoPagoIdConceptoPago,
                        principalTable: "ConceptoPago",
                        principalColumn: "IdConceptoPago");
                    table.ForeignKey(
                        name: "FK_ReciboDetalle_Recibo_ReciboIdRecibo",
                        column: x => x.ReciboIdRecibo,
                        principalTable: "Recibo",
                        principalColumn: "IdRecibo");
                });

            migrationBuilder.CreateTable(
                name: "PagoAplicacion",
                columns: table => new
                {
                    IdPagoAplicacion = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdPago = table.Column<long>(type: "bigint", nullable: false),
                    IdReciboDetalle = table.Column<long>(type: "bigint", nullable: false),
                    MontoAplicado = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    PagoIdPago = table.Column<long>(type: "bigint", nullable: true),
                    ReciboDetalleIdReciboDetalle = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PagoAplicacion", x => x.IdPagoAplicacion);
                    table.ForeignKey(
                        name: "FK_PagoAplicacion_Pago_PagoIdPago",
                        column: x => x.PagoIdPago,
                        principalTable: "Pago",
                        principalColumn: "IdPago");
                    table.ForeignKey(
                        name: "FK_PagoAplicacion_ReciboDetalle_ReciboDetalleIdReciboDetalle",
                        column: x => x.ReciboDetalleIdReciboDetalle,
                        principalTable: "ReciboDetalle",
                        principalColumn: "IdReciboDetalle");
                });

            migrationBuilder.CreateIndex(
                name: "IX_BitacoraRecibo_ReciboIdRecibo",
                table: "BitacoraRecibo",
                column: "ReciboIdRecibo");

            migrationBuilder.CreateIndex(
                name: "IX_ConceptoPrecio_ConceptoPagoIdConceptoPago",
                table: "ConceptoPrecio",
                column: "ConceptoPagoIdConceptoPago");

            migrationBuilder.CreateIndex(
                name: "IX_LigaPago_ReciboIdRecibo",
                table: "LigaPago",
                column: "ReciboIdRecibo");

            migrationBuilder.CreateIndex(
                name: "IX_Pago_MedioPagoIdMedioPago",
                table: "Pago",
                column: "MedioPagoIdMedioPago");

            migrationBuilder.CreateIndex(
                name: "IX_PagoAplicacion_PagoIdPago",
                table: "PagoAplicacion",
                column: "PagoIdPago");

            migrationBuilder.CreateIndex(
                name: "IX_PagoAplicacion_ReciboDetalleIdReciboDetalle",
                table: "PagoAplicacion",
                column: "ReciboDetalleIdReciboDetalle");

            migrationBuilder.CreateIndex(
                name: "IX_PlanPagoAsignacion_PlanPagoIdPlanPago",
                table: "PlanPagoAsignacion",
                column: "PlanPagoIdPlanPago");

            migrationBuilder.CreateIndex(
                name: "IX_PlanPagoDetalle_ConceptoPagoIdConceptoPago",
                table: "PlanPagoDetalle",
                column: "ConceptoPagoIdConceptoPago");

            migrationBuilder.CreateIndex(
                name: "IX_PlanPagoDetalle_PlanPagoIdPlanPago",
                table: "PlanPagoDetalle",
                column: "PlanPagoIdPlanPago");

            migrationBuilder.CreateIndex(
                name: "IX_ReciboDetalle_ConceptoPagoIdConceptoPago",
                table: "ReciboDetalle",
                column: "ConceptoPagoIdConceptoPago");

            migrationBuilder.CreateIndex(
                name: "IX_ReciboDetalle_ReciboIdRecibo",
                table: "ReciboDetalle",
                column: "ReciboIdRecibo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BitacoraRecibo");

            migrationBuilder.DropTable(
                name: "ConceptoPrecio");

            migrationBuilder.DropTable(
                name: "LigaPago");

            migrationBuilder.DropTable(
                name: "PagoAplicacion");

            migrationBuilder.DropTable(
                name: "PlanPagoAsignacion");

            migrationBuilder.DropTable(
                name: "PlanPagoDetalle");

            migrationBuilder.DropTable(
                name: "RecargoPolitica");

            migrationBuilder.DropTable(
                name: "Pago");

            migrationBuilder.DropTable(
                name: "ReciboDetalle");

            migrationBuilder.DropTable(
                name: "PlanPago");

            migrationBuilder.DropTable(
                name: "MedioPago");

            migrationBuilder.DropTable(
                name: "ConceptoPago");

            migrationBuilder.DropTable(
                name: "Recibo");
        }
    }
}
