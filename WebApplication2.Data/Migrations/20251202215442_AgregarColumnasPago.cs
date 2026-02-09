using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication2.Data.Migrations
{
    /// <inheritdoc />
    public partial class AgregarColumnasPago : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Agregar columnas a Pago solo si no existen
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Pago') AND name = 'FolioPago')
                BEGIN
                    ALTER TABLE Pago ADD FolioPago NVARCHAR(MAX) NULL;
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Pago') AND name = 'IdCaja')
                BEGIN
                    ALTER TABLE Pago ADD IdCaja INT NULL;
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Pago') AND name = 'IdCorteCaja')
                BEGIN
                    ALTER TABLE Pago ADD IdCorteCaja INT NULL;
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Pago') AND name = 'IdUsuarioCaja')
                BEGIN
                    ALTER TABLE Pago ADD IdUsuarioCaja NVARCHAR(MAX) NULL;
                END
            ");

            // Crear tabla CorteCaja solo si no existe
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CorteCaja')
                BEGIN
                    CREATE TABLE [CorteCaja] (
                        [IdCorteCaja] int NOT NULL IDENTITY,
                        [FolioCorteCaja] nvarchar(50) NOT NULL,
                        [FechaInicio] datetime2 NOT NULL,
                        [FechaFin] datetime2 NOT NULL,
                        [IdUsuarioCaja] nvarchar(450) NOT NULL,
                        [IdCaja] int NULL,
                        [MontoInicial] decimal(18,2) NOT NULL,
                        [TotalEfectivo] decimal(18,2) NOT NULL,
                        [TotalTransferencia] decimal(18,2) NOT NULL,
                        [TotalTarjeta] decimal(18,2) NOT NULL,
                        [TotalGeneral] decimal(18,2) NOT NULL,
                        [Cerrado] bit NOT NULL,
                        [FechaCierre] datetime2 NULL,
                        [CerradoPor] nvarchar(450) NULL,
                        [Observaciones] nvarchar(500) NULL,
                        [CreatedAt] datetime2 NOT NULL,
                        [UpdatedAt] datetime2 NULL,
                        [CreatedBy] nvarchar(max) NULL,
                        [UpdatedBy] nvarchar(max) NULL,
                        [Status] int NOT NULL,
                        CONSTRAINT [PK_CorteCaja] PRIMARY KEY ([IdCorteCaja]),
                        CONSTRAINT [FK_CorteCaja_AspNetUsers_CerradoPor] FOREIGN KEY ([CerradoPor]) REFERENCES [AspNetUsers] ([Id]),
                        CONSTRAINT [FK_CorteCaja_AspNetUsers_IdUsuarioCaja] FOREIGN KEY ([IdUsuarioCaja]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
                    );
                    CREATE INDEX [IX_CorteCaja_CerradoPor] ON [CorteCaja] ([CerradoPor]);
                    CREATE INDEX [IX_CorteCaja_IdUsuarioCaja] ON [CorteCaja] ([IdUsuarioCaja]);
                END
            ");

            // Crear tabla PlantillasCobro solo si no existe
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PlantillasCobro')
                BEGIN
                    CREATE TABLE [PlantillasCobro] (
                        [IdPlantillaCobro] int NOT NULL IDENTITY,
                        [NombrePlantilla] nvarchar(200) NOT NULL,
                        [IdPlanEstudios] int NOT NULL,
                        [NumeroCuatrimestre] int NOT NULL,
                        [IdPeriodoAcademico] int NULL,
                        [IdTurno] int NULL,
                        [IdModalidad] int NULL,
                        [Version] int NOT NULL DEFAULT 1,
                        [EsActiva] bit NOT NULL DEFAULT 1,
                        [FechaVigenciaInicio] datetime2 NOT NULL,
                        [FechaVigenciaFin] datetime2 NULL,
                        [EstrategiaEmision] int NOT NULL DEFAULT 0,
                        [NumeroRecibos] int NOT NULL DEFAULT 4,
                        [DiaVencimiento] int NOT NULL DEFAULT 10,
                        [CreadoPor] nvarchar(100) NOT NULL,
                        [FechaCreacion] datetime2 NOT NULL DEFAULT (sysutcdatetime()),
                        [ModificadoPor] nvarchar(100) NULL,
                        [FechaModificacion] datetime2 NULL,
                        [CreatedAt] datetime2 NOT NULL,
                        [UpdatedAt] datetime2 NULL,
                        [CreatedBy] nvarchar(max) NULL,
                        [UpdatedBy] nvarchar(max) NULL,
                        [Status] int NOT NULL,
                        CONSTRAINT [PK_PlantillasCobro] PRIMARY KEY ([IdPlantillaCobro]),
                        CONSTRAINT [FK_PlantillasCobro_PlanEstudios_IdPlanEstudios] FOREIGN KEY ([IdPlanEstudios]) REFERENCES [PlanEstudios] ([IdPlanEstudios])
                    );
                    CREATE INDEX [IX_PlantillasCobro_EsActiva] ON [PlantillasCobro] ([EsActiva]);
                    CREATE INDEX [IX_PlantillasCobro_IdPlanEstudios] ON [PlantillasCobro] ([IdPlanEstudios]);
                    CREATE INDEX [IX_PlantillasCobro_IdPlanEstudios_NumeroCuatrimestre_EsActiva] ON [PlantillasCobro] ([IdPlanEstudios], [NumeroCuatrimestre], [EsActiva]);
                END
            ");

            // Crear tabla PlantillasCobroDetalles solo si no existe
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PlantillasCobroDetalles')
                BEGIN
                    CREATE TABLE [PlantillasCobroDetalles] (
                        [IdPlantillaDetalle] int NOT NULL IDENTITY,
                        [IdPlantillaCobro] int NOT NULL,
                        [IdConceptoPago] int NOT NULL,
                        [Descripcion] nvarchar(300) NOT NULL,
                        [Cantidad] decimal(9,2) NOT NULL DEFAULT 1,
                        [PrecioUnitario] decimal(12,2) NOT NULL,
                        [Orden] int NOT NULL DEFAULT 1,
                        [AplicaEnRecibo] int NULL,
                        CONSTRAINT [PK_PlantillasCobroDetalles] PRIMARY KEY ([IdPlantillaDetalle]),
                        CONSTRAINT [FK_PlantillasCobroDetalles_ConceptoPago_IdConceptoPago] FOREIGN KEY ([IdConceptoPago]) REFERENCES [ConceptoPago] ([IdConceptoPago]),
                        CONSTRAINT [FK_PlantillasCobroDetalles_PlantillasCobro_IdPlantillaCobro] FOREIGN KEY ([IdPlantillaCobro]) REFERENCES [PlantillasCobro] ([IdPlantillaCobro]) ON DELETE CASCADE
                    );
                    CREATE INDEX [IX_PlantillasCobroDetalles_IdConceptoPago] ON [PlantillasCobroDetalles] ([IdConceptoPago]);
                    CREATE INDEX [IX_PlantillasCobroDetalles_IdPlantillaCobro] ON [PlantillasCobroDetalles] ([IdPlantillaCobro]);
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CorteCaja')
                    DROP TABLE [CorteCaja];
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PlantillasCobroDetalles')
                    DROP TABLE [PlantillasCobroDetalles];
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PlantillasCobro')
                    DROP TABLE [PlantillasCobro];
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Pago') AND name = 'FolioPago')
                    ALTER TABLE Pago DROP COLUMN FolioPago;
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Pago') AND name = 'IdCaja')
                    ALTER TABLE Pago DROP COLUMN IdCaja;
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Pago') AND name = 'IdCorteCaja')
                    ALTER TABLE Pago DROP COLUMN IdCorteCaja;
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Pago') AND name = 'IdUsuarioCaja')
                    ALTER TABLE Pago DROP COLUMN IdUsuarioCaja;
            ");
        }
    }
}
