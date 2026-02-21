using Microsoft.AspNetCore.Hosting;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.DTOs.Admision;
using WebApplication2.Core.DTOs.Comprobante;
using WebApplication2.Core.DTOs.Documentos;
using WebApplication2.Core.DTOs.Recibo;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Services;

public class PdfService : IPdfService
{
    private readonly string _logoPath;
    private static readonly string FontePrincipal = DetectarFuenteDisponible();
    private static readonly string ColorAzulOscuro = "#003366";
    private static readonly string ColorAzulClaro = "#0088CC";
    private static readonly string ColorGris = "#666666";
    private static readonly string ColorGrisClaro = "#F5F5F5";

    public PdfService(IWebHostEnvironment env)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        _logoPath = Path.Combine(env.ContentRootPath, "..", "Logousag.png");

        if (!File.Exists(_logoPath))
        {
            _logoPath = Path.Combine(env.ContentRootPath, "Logousag.png");
        }

        // Configurar fuente de respaldo para evitar errores cuando una fuente no está disponible
        QuestPDF.Settings.CheckIfAllTextGlyphsAreAvailable = false;
    }

    private static string DetectarFuenteDisponible()
    {
        // Configurar licencia antes de probar fuentes
        QuestPDF.Settings.License = LicenseType.Community;
        QuestPDF.Settings.CheckIfAllTextGlyphsAreAvailable = false;

        string[] fuentesPreferidas = ["Arial", "Liberation Sans", "DejaVu Sans", "Helvetica"];

        foreach (var fuente in fuentesPreferidas)
        {
            try
            {
                var testDoc = Document.Create(c =>
                {
                    c.Page(p =>
                    {
                        p.Content().Text("test").FontFamily(fuente);
                    });
                });
                testDoc.GeneratePdf();
                Console.WriteLine($"[PdfService] Fuente seleccionada: {fuente}");
                return fuente;
            }
            catch
            {
                Console.WriteLine($"[PdfService] Fuente '{fuente}' no disponible, probando siguiente...");
                continue;
            }
        }

        Console.WriteLine("[PdfService] Usando fuente por defecto: DejaVu Sans");
        return "DejaVu Sans";
    }

    private static readonly string ColorTitulo = "#2F5496";
    private static readonly string ColorSeccionHeader = "#B4C6E7";
    private static readonly string ColorBorde = "#000000";

    public byte[] GenerarHojaInscripcion(FichaAdmisionDto ficha)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.MarginTop(48);
                page.MarginBottom(42);
                page.MarginHorizontal(57);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily(FontePrincipal).Bold());

                page.Content().Column(col =>
                {
                    col.Item().Element(c => FichaHeader(c, ficha));
                    col.Item().Element(c => FichaTableBody(c, ficha));
                    col.Item().Element(c => FichaFirmas(c, ficha));
                    col.Item().Element(c => FichaAvisoPrivacidad(c));
                });
            });
        });

        return document.GeneratePdf();
    }

    private void FichaHeader(IContainer container, FichaAdmisionDto ficha)
    {
        var folio = ficha.Folio ?? $"ASP-{ficha.IdAspirante:D6}";
        var periodo = ficha.InformacionAcademica?.PeriodoAcademico
            ?? ficha.InformacionAcademica?.Periodicidad
            ?? "";
        var fecha = DateTime.Now.ToString("dd/MM/yyyy");

        container.Row(row =>
        {
            row.ConstantItem(77).BorderRight(0.5f).BorderColor(ColorBorde)
                .AlignCenter().AlignMiddle().Padding(4).Column(logoCol =>
                {
                    if (File.Exists(_logoPath))
                        logoCol.Item().MaxHeight(45).Image(_logoPath).FitArea();
                });

            row.ConstantItem(274).Border(0.5f).BorderColor(ColorBorde)
                .Background(ColorTitulo).AlignCenter().AlignMiddle()
                .Text("FORMATO DEL ASPIRANTE").Bold().FontSize(14).FontColor(Colors.White);

            row.RelativeItem().Column(infoCol =>
            {
                infoCol.Item().Border(0.5f).BorderColor(ColorBorde).MinHeight(20)
                    .PaddingHorizontal(5).AlignMiddle().Row(r =>
                    {
                        r.AutoItem().Text("FOLIO: ").Bold();
                        r.RelativeItem().Text(folio).Bold();
                    });
                infoCol.Item().Border(0.5f).BorderColor(ColorBorde).MinHeight(20)
                    .PaddingHorizontal(5).AlignMiddle().Row(r =>
                    {
                        r.AutoItem().Text("PERIODO: ").Bold().FontSize(8);
                        r.RelativeItem().Text(periodo).Bold().FontSize(8);
                    });
                infoCol.Item().Border(0.5f).BorderColor(ColorBorde).MinHeight(20)
                    .PaddingHorizontal(5).AlignMiddle().Row(r =>
                    {
                        r.AutoItem().Text("FECHA: ").Bold();
                        r.RelativeItem().Text(fecha).Bold();
                    });
            });
        });
    }

    private void FichaTableBody(IContainer container, FichaAdmisionDto ficha)
    {
        container.Border(0.5f).BorderColor(ColorBorde)
            .Column(col =>
            {
                FichaFilasDatosGenerales(col, ficha);
                FichaFilasProgramaEducativo(col, ficha);
                FichaFilasSocioeconomicos(col, ficha);
                FichaFilasFinancieros(col, ficha);
            });
    }

    private void FichaFilasDatosGenerales(ColumnDescriptor col, FichaAdmisionDto ficha)
    {
        var dp = ficha.DatosPersonales;
        var dc = ficha.DatosContacto;
        var dir = dc?.Direccion;
        col.Item().Background(ColorSeccionHeader).MinHeight(20)
            .AlignCenter().AlignMiddle()
            .Text("DATOS GENERALES DEL ALUMNO");

        col.Item().BorderTop(0.5f).BorderColor(ColorBorde).MinHeight(20).Row(row =>
        {
            row.ConstantItem(323).BorderRight(0.5f).BorderColor(ColorBorde)
                .PaddingHorizontal(5).AlignMiddle()
                .Text($"NOMBRE: {dp?.NombreCompleto ?? ""}");
            row.ConstantItem(98).BorderRight(0.5f).BorderColor(ColorBorde)
                .PaddingHorizontal(5).AlignMiddle()
                .Text($"GENERO: {dp?.Genero ?? ""}");
            row.RelativeItem().PaddingHorizontal(5).AlignMiddle()
                .Text($"EDAD: {dp?.Edad?.ToString() ?? ""}");
        });

        col.Item().BorderTop(0.5f).BorderColor(ColorBorde).MinHeight(20).Row(row =>
        {
            row.ConstantItem(141).BorderRight(0.5f).BorderColor(ColorBorde)
                .PaddingHorizontal(5).AlignMiddle()
                .Text($"ESTADO CIVIL: {dp?.EstadoCivil ?? ""}");
            row.ConstantItem(183).BorderRight(0.5f).BorderColor(ColorBorde)
                .PaddingHorizontal(5).AlignMiddle()
                .Text($"CALLE: {dir?.Calle ?? ""}");
            row.RelativeItem().PaddingHorizontal(5).AlignMiddle()
                .Text($"COLONIA: {dir?.Colonia ?? ""}");
        });

        col.Item().BorderTop(0.5f).BorderColor(ColorBorde).MinHeight(20).Row(row =>
        {
            row.ConstantItem(77).BorderRight(0.5f).BorderColor(ColorBorde)
                .PaddingHorizontal(5).AlignMiddle()
                .Text($"C.P. {dir?.CodigoPostal ?? ""}");
            row.ConstantItem(175).BorderRight(0.5f).BorderColor(ColorBorde)
                .PaddingHorizontal(5).AlignMiddle()
                .Text($"TELEFONO: {dc?.Telefono ?? ""}");
            row.RelativeItem().PaddingHorizontal(5).AlignMiddle()
                .Text($"CORREO: {dc?.Email ?? ""}");
        });

        col.Item().BorderTop(0.5f).BorderColor(ColorBorde).MinHeight(20).Row(row =>
        {
            row.ConstantItem(190).BorderRight(0.5f).BorderColor(ColorBorde)
                .PaddingHorizontal(5).AlignMiddle()
                .Text($"NACIONALIDAD: {dp?.Nacionalidad ?? ""}");
            row.RelativeItem().PaddingHorizontal(5).AlignMiddle()
                .Text($"CIUDAD Y ESTADO: {(dir != null ? $"{dir.Municipio}, {dir.Estado}" : "")}");
        });

        col.Item().BorderTop(0.5f).BorderColor(ColorBorde).MinHeight(20).Row(row =>
        {
            row.RelativeItem().PaddingHorizontal(5).AlignMiddle()
                .Text($"CURP: {dp?.CURP ?? ""}");
        });

        col.Item().BorderTop(0.5f).BorderColor(ColorBorde).MinHeight(20).Row(row =>
        {
            row.ConstantItem(253).BorderRight(0.5f).BorderColor(ColorBorde)
                .PaddingHorizontal(5).AlignMiddle()
                .Text($"TELEFONO ALTERNO: {dc?.Celular ?? ""}");
            row.RelativeItem().PaddingHorizontal(5).AlignMiddle()
                .Text($"PARENTESCO: {dc?.ParentescoContactoEmergencia ?? ""}");
        });
    }

    private void FichaFilasProgramaEducativo(ColumnDescriptor col, FichaAdmisionDto ficha)
    {
        var ia = ficha.InformacionAcademica;
        var seg = ficha.Seguimiento;
        var recorrido = ia?.RecorridoPlantel;

        col.Item().BorderTop(0.5f).BorderColor(ColorBorde)
            .Background(ColorSeccionHeader).MinHeight(20)
            .AlignCenter().AlignMiddle()
            .Text("DATOS DEL PROGRAMA EDUCATIVO");

        col.Item().BorderTop(0.5f).BorderColor(ColorBorde).MinHeight(20).Row(row =>
        {
            row.RelativeItem().PaddingHorizontal(5).AlignMiddle()
                .Text($"PROGRAMA DE INTERES: {ia?.NombrePlan ?? ""}");
        });

        col.Item().BorderTop(0.5f).BorderColor(ColorBorde).MinHeight(20).Row(row =>
        {
            row.RelativeItem().PaddingHorizontal(5).AlignMiddle()
                .Text($"INSTITUCION DE PROCEDENCIA: {ia?.InstitucionProcedencia ?? ""}");
        });

        col.Item().BorderTop(0.5f).BorderColor(ColorBorde).MinHeight(20).Row(row =>
        {
            row.ConstantItem(63).BorderRight(0.5f).BorderColor(ColorBorde)
                .PaddingHorizontal(5).AlignMiddle().Text("CAMPUS:");
            row.ConstantItem(112).BorderRight(0.5f).BorderColor(ColorBorde)
                .PaddingHorizontal(5).AlignMiddle().Text(ia?.Campus ?? "");
            row.ConstantItem(126).BorderRight(0.5f).BorderColor(ColorBorde)
                .PaddingHorizontal(5).AlignMiddle()
                .Text($"TURNO: {ia?.Turno ?? ""}");
            row.RelativeItem().PaddingHorizontal(5).AlignMiddle()
                .Text($"MODALIDAD: {ia?.Modalidad ?? ""}");
        });

        col.Item().BorderTop(0.5f).BorderColor(ColorBorde).MinHeight(20).Row(row =>
        {
            row.ConstantItem(140).BorderRight(0.5f).BorderColor(ColorBorde)
                .PaddingHorizontal(5).AlignMiddle()
                .Text($"HORARIO: {ia?.Turno ?? ""}");
            row.ConstantItem(140).BorderRight(0.5f).BorderColor(ColorBorde)
                .PaddingHorizontal(5).AlignMiddle().Text($"DIAS: {ia?.Dias ?? ""}");
            row.ConstantItem(121).BorderRight(0.5f).BorderColor(ColorBorde)
                .PaddingHorizontal(3).AlignMiddle()
                .Text("RECORRIDO POR EL CAMPUS").FontSize(8);
            row.ConstantItem(28).BorderRight(0.5f).BorderColor(ColorBorde)
                .AlignMiddle().AlignCenter()
                .Text(recorrido == true ? "SI X" : "SI").FontSize(9);
            row.RelativeItem().AlignMiddle().AlignCenter()
                .Text(recorrido == false ? "NO X" : "NO").FontSize(9);
        });

        col.Item().BorderTop(0.5f).BorderColor(ColorBorde).MinHeight(20).Row(row =>
        {
            row.RelativeItem().PaddingHorizontal(5).AlignMiddle()
                .Text($"COMO NOS CONOCISTE: {seg?.MedioContacto ?? ""}");
        });
    }

    private void FichaFilasSocioeconomicos(ColumnDescriptor col, FichaAdmisionDto ficha)
    {
        var se = ficha.DatosSocioeconomicos;

        col.Item().BorderTop(0.5f).BorderColor(ColorBorde)
            .Background(ColorSeccionHeader).MinHeight(20)
            .AlignCenter().AlignMiddle()
            .Text("DATOS SOCIOECONOMICOS");

        col.Item().BorderTop(0.5f).BorderColor(ColorBorde).MinHeight(20).Row(row =>
        {
            row.ConstantItem(70).BorderRight(0.5f).BorderColor(ColorBorde)
                .PaddingHorizontal(5).AlignMiddle().Text("TRABAJAS:");
            row.ConstantItem(27).BorderRight(0.5f).BorderColor(ColorBorde)
                .AlignMiddle().AlignCenter()
                .Text(se?.Trabaja == true ? "SI X" : "SI").FontSize(9);
            row.ConstantItem(36).BorderRight(0.5f).BorderColor(ColorBorde)
                .AlignMiddle().AlignCenter()
                .Text(se?.Trabaja == false ? "NO X" : "NO").FontSize(9);
            row.RelativeItem().PaddingHorizontal(5).AlignMiddle()
                .Text($"NOMBRE DE LA EMPRESA: {se?.NombreEmpresa ?? ""}");
        });

        col.Item().BorderTop(0.5f).BorderColor(ColorBorde).MinHeight(20).Row(row =>
        {
            row.ConstantItem(323).BorderRight(0.5f).BorderColor(ColorBorde)
                .PaddingHorizontal(5).AlignMiddle()
                .Text($"DOMICILIO: {se?.DomicilioEmpresa ?? ""}");
            row.RelativeItem().PaddingHorizontal(5).AlignMiddle()
                .Text($"PUESTO: {se?.PuestoEmpresa ?? ""}");
        });

        col.Item().BorderTop(0.5f).BorderColor(ColorBorde).MinHeight(20).Row(row =>
        {
            row.RelativeItem().PaddingHorizontal(5).AlignMiddle()
                .Text($"QUIEN CUBRIRA TUS GASTOS: {se?.QuienCubreGastos ?? ""}");
        });
    }

    private void FichaFilasFinancieros(ColumnDescriptor col, FichaAdmisionDto ficha)
    {
        var pagos = ficha.InformacionPagos;
        var costos = pagos?.CostosDesglose ?? new List<CostoDesglosePdfDto>();
        var tieneConvenio = pagos?.TieneConvenio ?? false;

        col.Item().BorderTop(0.5f).BorderColor(ColorBorde)
            .Background(ColorSeccionHeader).MinHeight(20)
            .AlignCenter().AlignMiddle()
            .Text("DATOS FINANCIEROS");

        col.Item().BorderTop(0.5f).BorderColor(ColorBorde).MinHeight(18).Row(row =>
        {
            row.ConstantItem(203).BorderRight(0.5f).BorderColor(ColorBorde)
                .AlignMiddle().AlignCenter().Text("COSTOS");
            row.RelativeItem().AlignMiddle().AlignCenter()
                .Text("DETALLES");
        });

        col.Item().BorderTop(0.5f).BorderColor(ColorBorde).Row(mainRow =>
        {
            mainRow.ConstantItem(203).BorderRight(0.5f).BorderColor(ColorBorde).Column(costCol =>
            {
                for (int i = 0; i < costos.Count; i++)
                {
                    var costo = costos[i];
                    var item = i > 0
                        ? costCol.Item().BorderTop(0.5f).BorderColor(ColorBorde)
                        : costCol.Item();

                    item.MinHeight(17).Row(r =>
                    {
                        r.ConstantItem(133).BorderRight(0.5f).BorderColor(ColorBorde)
                            .PaddingHorizontal(5).AlignMiddle()
                            .Text(costo.Concepto).FontSize(9);
                        r.RelativeItem().PaddingHorizontal(3).AlignMiddle().AlignRight()
                            .Text(costo.Monto.HasValue ? $"${costo.Monto.Value:N2}" : "N/A").FontSize(9);
                    });
                }

                // Fila de CONVENIO
                costCol.Item().BorderTop(0.5f).BorderColor(ColorBorde).MinHeight(17).Row(r =>
                {
                    r.ConstantItem(133).BorderRight(0.5f).BorderColor(ColorBorde)
                        .PaddingHorizontal(5).AlignMiddle()
                        .Text("CONVENIO").FontSize(9);
                    r.RelativeItem().PaddingHorizontal(3).AlignMiddle().AlignCenter()
                        .Text(tieneConvenio ? "SI" : "NO").FontSize(9);
                });
            });

            mainRow.RelativeItem().Padding(5)
                .Text(ficha.Observaciones ?? "").FontSize(9);
        });
    }

    private void FichaFirmas(IContainer container, FichaAdmisionDto ficha)
    {
        var nombreAspirante = ficha.DatosPersonales?.NombreCompleto ?? "";
        var nombreEntrevistador = ficha.Seguimiento?.AsesorAsignado?.NombreCompleto ?? "";

        container.PaddingTop(25).Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().PaddingBottom(35).Text("");
                col.Item().PaddingHorizontal(20).LineHorizontal(0.5f).LineColor(Colors.Black);
                col.Item().AlignCenter().PaddingTop(3).Text(nombreAspirante).Bold().FontSize(9);
                col.Item().AlignCenter().PaddingTop(1).Text("Firma del aspirante").FontSize(8);
            });

            row.ConstantItem(60);

            row.RelativeItem().Column(col =>
            {
                col.Item().PaddingBottom(35).Text("");
                col.Item().PaddingHorizontal(20).LineHorizontal(0.5f).LineColor(Colors.Black);
                col.Item().AlignCenter().PaddingTop(3).Text(nombreEntrevistador).Bold().FontSize(9);
                col.Item().AlignCenter().PaddingTop(1).Text("Entrevistador").FontSize(8);
            });
        });
    }

    private void FichaAvisoPrivacidad(IContainer container)
    {
        container.PaddingTop(6).Column(col =>
        {
            col.Item().Text("Declaro bajo protesta de decir verdad que la información y documentación proporcionada es verídica, por lo que, en caso de existir falsedad en ella, tengo pleno conocimiento que se aplicarán las sanciones administrativas y penas establecidas en los ordenamientos respectivos para quienes se conducen con falsedad ante la autoridad competente.")
                .FontSize(6);
            col.Item().PaddingTop(2)
                .Text("Usted puede consultar en cualquier momento nuestro Aviso de Privacidad en la página de internet https://usaguanajuato.edu.mx; o https://usaguanajuato.edu.mx/docs/AVISO%DE%20PRIVACIDAD.PDF")
                .FontSize(6);
        });
    }

    #region Kardex PDF

    public Task<byte[]> GenerarKardexPdf(KardexEstudianteDto kardex, string folioDocumento, Guid codigoVerificacion, string urlVerificacion)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.MarginVertical(30);
                page.MarginHorizontal(40);
                page.DefaultTextStyle(x => x.FontSize(9).FontFamily(FontePrincipal));

                page.Header().Element(c => ComposeKardexHeader(c, kardex, folioDocumento));
                page.Content().Element(c => ComposeKardexContent(c, kardex));
                page.Footer().Element(c => ComposeDocumentoFooter(c, codigoVerificacion, urlVerificacion));
            });
        });

        return Task.FromResult(document.GeneratePdf());
    }

    private void ComposeKardexHeader(IContainer container, KardexEstudianteDto kardex, string folio)
    {
        container.Column(column =>
        {
            column.Item().Row(row =>
            {
                if (File.Exists(_logoPath))
                {
                    row.ConstantItem(100).Height(50).Image(_logoPath).FitArea();
                }
                else
                {
                    row.ConstantItem(100).Height(50).Background(ColorGrisClaro)
                        .AlignCenter().AlignMiddle().Text("LOGO").FontSize(10).Bold();
                }

                row.RelativeItem().Column(col =>
                {
                    col.Item().AlignCenter().Text("UNIVERSIDAD SAN ANDRÉS DE GUANAJUATO")
                        .FontSize(14).Bold().FontColor(ColorAzulOscuro);
                    col.Item().AlignCenter().Text("KARDEX ACADÉMICO")
                        .FontSize(12).SemiBold().FontColor(ColorAzulClaro);
                });

                row.ConstantItem(100).AlignRight().Column(col =>
                {
                    col.Item().Text($"Folio: {folio}").FontSize(8).Bold();
                    col.Item().Text($"Fecha: {DateTime.Now:dd/MM/yyyy}").FontSize(8);
                });
            });

            column.Item().PaddingTop(8).LineHorizontal(2).LineColor(ColorAzulOscuro);

            column.Item().PaddingTop(10).Background(ColorGrisClaro).Padding(10).Column(dataCol =>
            {
                dataCol.Item().Row(row =>
                {
                    row.RelativeItem().Text(t => { t.Span("Matrícula: ").Bold(); t.Span(kardex.Matricula); });
                    row.RelativeItem().Text(t => { t.Span("Nombre: ").Bold(); t.Span(kardex.NombreCompleto); });
                });
                dataCol.Item().PaddingTop(3).Row(row =>
                {
                    row.RelativeItem().Text(t => { t.Span("Carrera: ").Bold(); t.Span(kardex.Carrera); });
                    row.RelativeItem().Text(t => { t.Span("Plan: ").Bold(); t.Span(kardex.PlanEstudios); });
                });
                dataCol.Item().PaddingTop(3).Row(row =>
                {
                    row.RelativeItem().Text(t => { t.Span("RVOE: ").Bold(); t.Span(kardex.RVOE ?? "N/A"); });
                    row.RelativeItem().Text(t => { t.Span("Ingreso: ").Bold(); t.Span(kardex.FechaIngreso.ToString("dd/MM/yyyy")); });
                    row.RelativeItem().Text(t => { t.Span("Estatus: ").Bold(); t.Span(kardex.Estatus); });
                });
            });

            column.Item().PaddingTop(8).Row(row =>
            {
                row.RelativeItem().Background("#E3F2FD").Padding(8).Column(col =>
                {
                    col.Item().AlignCenter().Text("Promedio General").FontSize(8).FontColor(ColorGris);
                    col.Item().AlignCenter().Text($"{kardex.PromedioGeneral:F2}").FontSize(16).Bold().FontColor(ColorAzulOscuro);
                });
                row.ConstantItem(10);
                row.RelativeItem().Background("#E8F5E9").Padding(8).Column(col =>
                {
                    col.Item().AlignCenter().Text("Créditos Cursados").FontSize(8).FontColor(ColorGris);
                    col.Item().AlignCenter().Text($"{kardex.CreditosCursados}/{kardex.CreditosTotales}").FontSize(16).Bold().FontColor("#2E7D32");
                });
                row.ConstantItem(10);
                row.RelativeItem().Background("#FFF3E0").Padding(8).Column(col =>
                {
                    col.Item().AlignCenter().Text("Avance").FontSize(8).FontColor(ColorGris);
                    col.Item().AlignCenter().Text($"{kardex.PorcentajeAvance:F1}%").FontSize(16).Bold().FontColor("#E65100");
                });
            });

            column.Item().PaddingTop(8).LineHorizontal(1).LineColor(ColorGris);
        });
    }

    private void ComposeKardexContent(IContainer container, KardexEstudianteDto kardex)
    {
        container.PaddingTop(10).Column(column =>
        {
            foreach (var periodo in kardex.Periodos)
            {
                column.Item().Element(c => ComposeKardexPeriodo(c, periodo));
                column.Item().PaddingVertical(5);
            }
        });
    }

    private void ComposeKardexPeriodo(IContainer container, KardexPeriodoDto periodo)
    {
        container.Column(column =>
        {
            column.Item().Background(ColorAzulOscuro).Padding(6).Row(row =>
            {
                row.RelativeItem().Text($"Período: {periodo.Periodo}").FontColor(Colors.White).Bold();
                row.ConstantItem(150).AlignRight().Text($"Promedio: {periodo.PromedioPeriodo:F2} | Créditos: {periodo.CreditosPeriodo}")
                    .FontColor(Colors.White).FontSize(8);
            });

            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(60);
                    columns.RelativeColumn(3);
                    columns.ConstantColumn(50);
                    columns.ConstantColumn(70);
                    columns.ConstantColumn(70);
                });

                table.Header(header =>
                {
                    header.Cell().Background(ColorGrisClaro).Padding(4).Text("Clave").Bold().FontSize(8);
                    header.Cell().Background(ColorGrisClaro).Padding(4).Text("Materia").Bold().FontSize(8);
                    header.Cell().Background(ColorGrisClaro).Padding(4).AlignCenter().Text("Créditos").Bold().FontSize(8);
                    header.Cell().Background(ColorGrisClaro).Padding(4).AlignCenter().Text("Calificación").Bold().FontSize(8);
                    header.Cell().Background(ColorGrisClaro).Padding(4).AlignCenter().Text("Estatus").Bold().FontSize(8);
                });

                foreach (var materia in periodo.Materias)
                {
                    var colorEstatus = materia.Estatus == "Aprobada" ? Colors.Green.Medium :
                                      materia.Estatus == "Reprobada" ? Colors.Red.Medium : Colors.Orange.Medium;

                    table.Cell().BorderBottom(1).BorderColor(ColorGrisClaro).Padding(4).Text(materia.ClaveMateria).FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor(ColorGrisClaro).Padding(4).Text(materia.NombreMateria).FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor(ColorGrisClaro).Padding(4).AlignCenter().Text(materia.Creditos.ToString()).FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor(ColorGrisClaro).Padding(4).AlignCenter()
                        .Text(materia.CalificacionFinal?.ToString("F1") ?? "-").FontSize(8).Bold();
                    table.Cell().BorderBottom(1).BorderColor(ColorGrisClaro).Padding(4).AlignCenter()
                        .Text(materia.Estatus).FontSize(8).FontColor(colorEstatus);
                }
            });
        });
    }

    #endregion

    #region Constancia PDF

    public Task<byte[]> GenerarConstanciaPdf(ConstanciaEstudiosDto constancia)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.MarginVertical(50);
                page.MarginHorizontal(60);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily(FontePrincipal));

                page.Header().Element(c => ComposeConstanciaHeader(c));
                page.Content().Element(c => ComposeConstanciaContent(c, constancia));
                page.Footer().Element(c => ComposeDocumentoFooter(c, constancia.CodigoVerificacion, constancia.UrlVerificacion));
            });
        });

        return Task.FromResult(document.GeneratePdf());
    }

    private void ComposeConstanciaHeader(IContainer container)
    {
        container.Column(column =>
        {
            column.Item().Row(row =>
            {
                if (File.Exists(_logoPath))
                {
                    row.ConstantItem(120).Height(60).Image(_logoPath).FitArea();
                }
                else
                {
                    row.ConstantItem(120).Height(60).Background(ColorGrisClaro)
                        .AlignCenter().AlignMiddle().Text("LOGO").FontSize(12).Bold();
                }

                row.RelativeItem().Column(col =>
                {
                    col.Item().AlignCenter().Text("UNIVERSIDAD SAN ANDRÉS DE GUANAJUATO")
                        .FontSize(16).Bold().FontColor(ColorAzulOscuro);
                    col.Item().AlignCenter().Text("CONSTANCIA DE ESTUDIOS")
                        .FontSize(14).SemiBold().FontColor(ColorAzulClaro);
                    col.Item().AlignCenter().PaddingTop(3).Text("\"Veni Vidi Vici\"")
                        .FontSize(9).Italic().FontColor(ColorGris);
                });

                row.ConstantItem(120);
            });

            column.Item().PaddingTop(15).LineHorizontal(2).LineColor(ColorAzulOscuro);
        });
    }

    private void ComposeConstanciaContent(IContainer container, ConstanciaEstudiosDto constancia)
    {
        container.PaddingTop(30).Column(column =>
        {
            column.Item().Row(row =>
            {
                row.RelativeItem().Text(t => { t.Span("Folio: ").Bold(); t.Span(constancia.FolioDocumento); });
                row.RelativeItem().AlignRight().Text(t => { t.Span("Guanajuato, Gto. a ").FontColor(ColorGris); t.Span(DateTime.Now.ToString("dd 'de' MMMM 'de' yyyy")); });
            });

            column.Item().PaddingTop(30).Text("A QUIEN CORRESPONDA:").Bold().FontSize(12);

            column.Item().PaddingTop(20).Text(text =>
            {
                text.Span("Por medio de la presente se hace constar que ");
                text.Span(constancia.NombreCompleto.ToUpper()).Bold();
                text.Span(", con matrícula ");
                text.Span(constancia.Matricula).Bold();
                text.Span(", se encuentra actualmente inscrito(a) como alumno(a) regular de esta institución en la carrera de ");
                text.Span(constancia.Carrera).Bold();
                text.Span(", con plan de estudios ");
                text.Span(constancia.PlanEstudios).Bold();
                if (!string.IsNullOrEmpty(constancia.RVOE))
                {
                    text.Span(", con RVOE: ");
                    text.Span(constancia.RVOE).Bold();
                }
                text.Span(".");
            });

            column.Item().PaddingTop(15).Text(text =>
            {
                text.Span("El alumno(a) se encuentra cursando el período académico ");
                text.Span(constancia.PeriodoActual).Bold();
                text.Span(" en el turno ");
                text.Span(constancia.Turno).Bold();
                text.Span(", en el campus ");
                text.Span(constancia.Campus).Bold();
                text.Span(".");
            });

            if (constancia.IncluyeMaterias && constancia.Materias.Count > 0)
            {
                column.Item().PaddingTop(20).Text("Materias que cursa actualmente:").Bold();
                column.Item().PaddingTop(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(60);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background(ColorAzulOscuro).Padding(5).Text("Clave").FontColor(Colors.White).Bold().FontSize(9);
                        header.Cell().Background(ColorAzulOscuro).Padding(5).Text("Materia").FontColor(Colors.White).Bold().FontSize(9);
                        header.Cell().Background(ColorAzulOscuro).Padding(5).Text("Profesor").FontColor(Colors.White).Bold().FontSize(9);
                    });

                    foreach (var materia in constancia.Materias)
                    {
                        table.Cell().BorderBottom(1).BorderColor(ColorGrisClaro).Padding(4).Text(materia.ClaveMateria).FontSize(9);
                        table.Cell().BorderBottom(1).BorderColor(ColorGrisClaro).Padding(4).Text(materia.NombreMateria).FontSize(9);
                        table.Cell().BorderBottom(1).BorderColor(ColorGrisClaro).Padding(4).Text(materia.Profesor).FontSize(9);
                    }
                });
            }

            column.Item().PaddingTop(25).Text(text =>
            {
                text.Span("Se extiende la presente constancia para los fines legales que al interesado convengan, en la ciudad de Guanajuato, Gto., a los ");
                text.Span(DateTime.Now.ToString("dd")).Bold();
                text.Span(" días del mes de ");
                text.Span(DateTime.Now.ToString("MMMM")).Bold();
                text.Span(" del año ");
                text.Span(DateTime.Now.ToString("yyyy")).Bold();
                text.Span(".");
            });

            column.Item().PaddingTop(15).Background("#FFF8E1").Padding(10).Text(text =>
            {
                text.Span("Vigencia del documento: ").Bold().FontColor("#F57C00");
                text.Span($"Esta constancia tiene validez hasta el {constancia.FechaVencimiento:dd/MM/yyyy}").FontColor("#F57C00");
            });

            column.Item().PaddingTop(50).AlignCenter().Column(col =>
            {
                col.Item().AlignCenter().Text("ATENTAMENTE").Bold();
                col.Item().PaddingTop(40).AlignCenter().LineHorizontal(1).LineColor(Colors.Black);
                col.Item().PaddingTop(5).AlignCenter().Text("Director(a) Académico").FontSize(10);
                col.Item().AlignCenter().Text("Universidad San Andrés de Guanajuato").FontSize(9).FontColor(ColorGris);
            });
        });
    }

    #endregion

    #region Footer con QR

    private void ComposeDocumentoFooter(IContainer container, Guid codigoVerificacion, string urlVerificacion)
    {
        container.Column(column =>
        {
            column.Item().LineHorizontal(1).LineColor(ColorGris);
            column.Item().PaddingTop(8).Row(row =>
            {
                row.ConstantItem(60).Height(60).Border(1).BorderColor(ColorGris).AlignCenter().AlignMiddle()
                    .Column(col =>
                    {
                        col.Item().AlignCenter().Text("QR").FontSize(8).FontColor(ColorGris);
                        col.Item().AlignCenter().Text(codigoVerificacion.ToString().Substring(0, 8)).FontSize(6).FontColor(ColorGris);
                    });

                row.ConstantItem(10);

                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("Documento verificable").FontSize(8).Bold().FontColor(ColorAzulOscuro);
                    col.Item().Text(text =>
                    {
                        text.Span("Código de verificación: ").FontSize(7).FontColor(ColorGris);
                        text.Span(codigoVerificacion.ToString()).FontSize(7);
                    });
                    col.Item().Text(text =>
                    {
                        text.Span("Verificar en: ").FontSize(7).FontColor(ColorGris);
                        text.Span(urlVerificacion).FontSize(7).FontColor(ColorAzulClaro);
                    });
                });

                row.RelativeItem().AlignRight().Column(col =>
                {
                    col.Item().AlignRight().Text(text =>
                    {
                        text.CurrentPageNumber().FontSize(8).FontColor(ColorGris);
                        text.Span(" de ").FontSize(8).FontColor(ColorGris);
                        text.TotalPages().FontSize(8).FontColor(ColorGris);
                    });
                    col.Item().AlignRight().Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(7).FontColor(ColorGris);
                });
            });
        });
    }

    #endregion

    #region Comprobante de Pago PDF

    public byte[] GenerarComprobantePago(ComprobantePagoDto comprobante)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(226, 800, Unit.Point);
                page.MarginVertical(15);
                page.MarginHorizontal(10);
                page.DefaultTextStyle(x => x.FontSize(9).FontFamily(FontePrincipal));

                page.Content().Element(c => ComposeComprobanteContent(c, comprobante));
            });
        });

        return document.GeneratePdf();
    }

    private void ComposeComprobanteContent(IContainer container, ComprobantePagoDto comprobante)
    {
        container.Column(column =>
        {
            column.Item().Column(headerCol =>
            {
                if (File.Exists(_logoPath))
                {
                    headerCol.Item().AlignCenter().Height(40).Image(_logoPath).FitArea();
                }

                headerCol.Item().AlignCenter().Text(comprobante.Institucion.Nombre)
                    .FontSize(10).Bold().FontColor(ColorAzulOscuro);
                headerCol.Item().AlignCenter().Text(comprobante.Institucion.NombreCorto)
                    .FontSize(8).FontColor(ColorGris);

                if (!string.IsNullOrEmpty(comprobante.Institucion.RFC))
                {
                    headerCol.Item().AlignCenter().Text($"RFC: {comprobante.Institucion.RFC}")
                        .FontSize(7).FontColor(ColorGris);
                }
            });

            column.Item().PaddingVertical(5).LineHorizontal(1).LineColor(ColorGris);

            column.Item().AlignCenter().Text("COMPROBANTE DE PAGO")
                .FontSize(11).Bold().FontColor(ColorAzulOscuro);

            column.Item().PaddingVertical(3).LineHorizontal(1).LineColor(ColorGris);

            column.Item().PaddingTop(5).Column(pagoCol =>
            {
                pagoCol.Item().Row(row =>
                {
                    row.RelativeItem().Text(t => { t.Span("Folio: ").Bold(); t.Span(comprobante.Pago.FolioPago); });
                });
                pagoCol.Item().PaddingTop(2).Row(row =>
                {
                    row.RelativeItem().Text(t =>
                    {
                        t.Span("Fecha: ").Bold();
                        t.Span(comprobante.Pago.FechaPago.ToString("dd/MM/yyyy"));
                    });
                    row.RelativeItem().AlignRight().Text(t =>
                    {
                        t.Span("Hora: ").Bold();
                        t.Span(comprobante.Pago.HoraPago);
                    });
                });
            });

            column.Item().PaddingVertical(5).LineHorizontal(1).LineColor(ColorGrisClaro);

            column.Item().Column(estCol =>
            {
                estCol.Item().Text("DATOS DEL ESTUDIANTE").FontSize(8).Bold().FontColor(ColorAzulOscuro);
                estCol.Item().PaddingTop(3).Text(t =>
                {
                    t.Span("Matrícula: ").Bold().FontSize(8);
                    var matricula = !string.IsNullOrEmpty(comprobante.Estudiante.Matricula)
                        ? comprobante.Estudiante.Matricula
                        : "Sin información";
                    t.Span(matricula).FontSize(8);
                });
                estCol.Item().Text(t =>
                {
                    t.Span("Nombre: ").Bold().FontSize(8);
                    var nombre = !string.IsNullOrEmpty(comprobante.Estudiante.NombreCompleto)
                        ? comprobante.Estudiante.NombreCompleto
                        : "Sin información";
                    t.Span(nombre).FontSize(8);
                });
                if (!string.IsNullOrEmpty(comprobante.Estudiante.Carrera))
                {
                    estCol.Item().Text(t =>
                    {
                        t.Span("Carrera: ").Bold().FontSize(8);
                        t.Span(comprobante.Estudiante.Carrera).FontSize(8);
                    });
                }
            });

            column.Item().PaddingVertical(5).LineHorizontal(1).LineColor(ColorGrisClaro);

            column.Item().Column(recibosCol =>
            {
                recibosCol.Item().Text("CONCEPTOS PAGADOS").FontSize(8).Bold().FontColor(ColorAzulOscuro);

                foreach (var recibo in comprobante.RecibosPagados)
                {
                    recibosCol.Item().PaddingTop(4).Background(ColorGrisClaro).Padding(5).Column(detalleCol =>
                    {
                        detalleCol.Item().Row(row =>
                        {
                            row.RelativeItem().Text(recibo.Concepto).FontSize(8).Bold();
                            row.ConstantItem(60).AlignRight().Text($"${recibo.MontoPagado:N2}").FontSize(8).Bold();
                        });
                        detalleCol.Item().Text($"Recibo: {recibo.Folio}").FontSize(7).FontColor(ColorGris);
                        if (!string.IsNullOrEmpty(recibo.Periodo))
                        {
                            detalleCol.Item().Text($"Período: {recibo.Periodo}").FontSize(7).FontColor(ColorGris);
                        }
                        if (recibo.Descuento > 0)
                        {
                            detalleCol.Item().Text($"Descuento aplicado: -${recibo.Descuento:N2}").FontSize(7).FontColor(Colors.Green.Medium);
                        }
                        if (recibo.Recargos > 0)
                        {
                            detalleCol.Item().Text($"Recargos: +${recibo.Recargos:N2}").FontSize(7).FontColor(Colors.Orange.Medium);
                        }
                        detalleCol.Item().Row(row =>
                        {
                            row.RelativeItem().Text($"Saldo anterior: ${recibo.SaldoAnterior:N2}").FontSize(7);
                            row.RelativeItem().AlignRight().Text($"Saldo nuevo: ${recibo.SaldoNuevo:N2}").FontSize(7);
                        });
                        var colorEstatus = recibo.Estatus.ToUpper() == "PAGADO" ? Colors.Green.Medium : Colors.Orange.Medium;
                        detalleCol.Item().Text(recibo.Estatus.ToUpper()).FontSize(7).Bold().FontColor(colorEstatus);
                    });
                }
            });

            column.Item().PaddingVertical(5).LineHorizontal(1).LineColor(ColorGris);

            column.Item().Background(ColorAzulOscuro).Padding(8).Row(row =>
            {
                row.RelativeItem().Text("TOTAL PAGADO").FontColor(Colors.White).Bold();
                row.ConstantItem(80).AlignRight().Text($"${comprobante.Pago.Monto:N2} {comprobante.Pago.Moneda}")
                    .FontColor(Colors.White).Bold().FontSize(11);
            });

            column.Item().PaddingTop(5).Column(metodoCol =>
            {
                metodoCol.Item().Row(row =>
                {
                    row.RelativeItem().Text(t =>
                    {
                        t.Span("Método de pago: ").Bold().FontSize(8);
                        t.Span(comprobante.Pago.MedioPago).FontSize(8);
                    });
                });
                if (!string.IsNullOrEmpty(comprobante.Pago.Referencia))
                {
                    metodoCol.Item().Text(t =>
                    {
                        t.Span("Referencia: ").Bold().FontSize(8);
                        t.Span(comprobante.Pago.Referencia).FontSize(8);
                    });
                }
            });

            if (comprobante.Cajero != null)
            {
                column.Item().PaddingTop(5).Text(t =>
                {
                    t.Span("Atendió: ").Bold().FontSize(7).FontColor(ColorGris);
                    t.Span(comprobante.Cajero.NombreCompleto).FontSize(7).FontColor(ColorGris);
                });
            }

            column.Item().PaddingVertical(8).LineHorizontal(1).LineColor(ColorGris);

            column.Item().AlignCenter().Text("¡Gracias por su pago!")
                .FontSize(9).Bold().FontColor(ColorAzulClaro);

            column.Item().PaddingTop(3).AlignCenter().Text("Conserve este comprobante para cualquier aclaración")
                .FontSize(7).FontColor(ColorGris);

            if (!string.IsNullOrEmpty(comprobante.Pago.Notas))
            {
                column.Item().PaddingTop(8).Background("#FFF8E1").Padding(5).Column(notasCol =>
                {
                    notasCol.Item().Text("Notas:").FontSize(7).Bold();
                    notasCol.Item().Text(comprobante.Pago.Notas).FontSize(7);
                });
            }

            column.Item().PaddingTop(10).AlignCenter().Text($"Documento generado el {DateTime.Now:dd/MM/yyyy HH:mm:ss}")
                .FontSize(6).FontColor(ColorGris);

            column.Item().AlignCenter().Text($"ID: {comprobante.Pago.IdPago}")
                .FontSize(6).FontColor(ColorGris);
        });
    }

    #endregion

    #region Recibo de Pago (Formato USAG)

    public byte[] GenerarReciboPdf(ReciboPdfDto recibo)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.MarginVertical(30);
                page.MarginHorizontal(40);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily(FontePrincipal));

                page.Header().Element(c => ComposeReciboHeader(c, recibo));
                page.Content().Element(c => ComposeReciboContent(c, recibo));
                page.Footer().Element(c => ComposeReciboFooter(c, recibo));
            });
        });

        return document.GeneratePdf();
    }

    private void ComposeReciboHeader(IContainer container, ReciboPdfDto recibo)
    {
        container.Column(column =>
        {
            column.Item().Row(row =>
            {
                if (File.Exists(_logoPath))
                {
                    row.ConstantItem(100).Height(60).Image(_logoPath).FitArea();
                }
                else
                {
                    row.ConstantItem(100).Height(60).Background(ColorGrisClaro)
                        .AlignCenter().AlignMiddle()
                        .Text("USAG").FontSize(12).Bold().FontColor(ColorAzulOscuro);
                }

                row.RelativeItem().Column(col =>
                {
                    col.Item().AlignCenter().Text(recibo.Institucion?.Nombre ?? "UNIVERSIDAD SAN ANDRÉS DE GUANAJUATO")
                        .FontSize(14).Bold().FontColor(ColorAzulOscuro);
                    col.Item().AlignCenter().Text(recibo.Institucion?.Campus ?? "CAMPUS LEÓN")
                        .FontSize(11).SemiBold().FontColor(ColorAzulClaro);
                    col.Item().AlignCenter().Text(recibo.Institucion?.Direccion ?? "República de Cuba #201 Col. Bellavista León, Gto.")
                        .FontSize(8).FontColor(ColorGris);
                });

                row.ConstantItem(100).Column(folioCol =>
                {
                    folioCol.Item().AlignRight().Text("FOLIO").FontSize(8).FontColor(ColorGris);
                    folioCol.Item().AlignRight().Text(recibo.Folio ?? "N/A")
                        .FontSize(14).Bold().FontColor(ColorAzulOscuro);
                    folioCol.Item().Height(30).AlignCenter().AlignMiddle()
                        .Text("🇲🇽").FontSize(20);
                });
            });

            column.Item().PaddingTop(8).LineHorizontal(2).LineColor(ColorAzulOscuro);

            column.Item().PaddingTop(10).Background(ColorGrisClaro).Padding(10).Column(dataCol =>
            {
                dataCol.Item().Row(row =>
                {
                    row.RelativeItem().Text(t =>
                    {
                        t.Span("Fecha: ").Bold();
                        t.Span(recibo.FechaEmision.ToString("dd 'de' MMMM 'de' yyyy", new System.Globalization.CultureInfo("es-MX")));
                    });
                    row.RelativeItem().AlignRight().Text(t =>
                    {
                        t.Span("Vencimiento: ").Bold();
                        t.Span(recibo.FechaVencimiento.ToString("dd/MM/yyyy"));
                    });
                });
                dataCol.Item().PaddingTop(5).Row(row =>
                {
                    row.RelativeItem(2).Text(t =>
                    {
                        t.Span("Alumno: ").Bold();
                        t.Span(recibo.NombreEstudiante ?? "N/A");
                    });
                    row.RelativeItem().Text(t =>
                    {
                        t.Span("Matrícula: ").Bold();
                        t.Span(recibo.Matricula ?? "N/A");
                    });
                });
                dataCol.Item().PaddingTop(5).Row(row =>
                {
                    row.RelativeItem(2).Text(t =>
                    {
                        t.Span("Carrera: ").Bold();
                        t.Span(recibo.Carrera ?? "N/A");
                    });
                    row.RelativeItem().Text(t =>
                    {
                        t.Span("Periodo: ").Bold();
                        t.Span(recibo.Periodo ?? "N/A");
                    });
                });
            });

            column.Item().PaddingTop(8).LineHorizontal(1).LineColor(ColorGris);
        });
    }

    private void ComposeReciboContent(IContainer container, ReciboPdfDto recibo)
    {
        container.PaddingTop(10).Column(column =>
        {
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(50);
                    columns.RelativeColumn(3);
                    columns.ConstantColumn(80);
                    columns.ConstantColumn(80);
                });

                table.Header(header =>
                {
                    header.Cell().Background(ColorAzulOscuro).Padding(8)
                        .Text("PAGO No.").FontColor(Colors.White).Bold().FontSize(9).AlignCenter();
                    header.Cell().Background(ColorAzulOscuro).Padding(8)
                        .Text("DESCRIPCIÓN").FontColor(Colors.White).Bold().FontSize(9);
                    header.Cell().Background(ColorAzulOscuro).Padding(8)
                        .Text("PRECIO UNITARIO").FontColor(Colors.White).Bold().FontSize(9).AlignCenter();
                    header.Cell().Background(ColorAzulOscuro).Padding(8)
                        .Text("IMPORTE").FontColor(Colors.White).Bold().FontSize(9).AlignRight();
                });

                var index = 1;
                foreach (var detalle in recibo.Detalles ?? new List<ReciboDetallePdfDto>())
                {
                    var bgColor = index % 2 == 0 ? "#FFFFFF" : ColorGrisClaro;

                    table.Cell().Background(bgColor).Padding(6).AlignCenter()
                        .Text(index.ToString()).FontSize(9);
                    table.Cell().Background(bgColor).Padding(6)
                        .Text(detalle.Descripcion ?? "").FontSize(9);
                    table.Cell().Background(bgColor).Padding(6).AlignRight()
                        .Text($"${detalle.PrecioUnitario:N2}").FontSize(9);
                    table.Cell().Background(bgColor).Padding(6).AlignRight()
                        .Text($"${detalle.Importe:N2}").FontSize(9);
                    index++;
                }

                for (int i = index; i <= 5; i++)
                {
                    var bgColor = i % 2 == 0 ? "#FFFFFF" : ColorGrisClaro;
                    table.Cell().Background(bgColor).Padding(6).Text("");
                    table.Cell().Background(bgColor).Padding(6).Text("");
                    table.Cell().Background(bgColor).Padding(6).Text("");
                    table.Cell().Background(bgColor).Padding(6).Text("");
                }
            });

            column.Item().PaddingTop(5);

            column.Item().AlignRight().Width(250).Table(totalesTable =>
            {
                totalesTable.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn();
                    columns.ConstantColumn(100);
                });

                totalesTable.Cell().Padding(5).AlignRight().Text("Subtotal:").Bold();
                totalesTable.Cell().Padding(5).AlignRight().Text($"${recibo.Subtotal:N2}");

                if (recibo.Descuento > 0)
                {
                    totalesTable.Cell().Padding(5).AlignRight().Text("Descuento (Beca):").Bold().FontColor(Colors.Green.Medium);
                    totalesTable.Cell().Padding(5).AlignRight().Text($"-${recibo.Descuento:N2}").FontColor(Colors.Green.Medium);
                }

                if (recibo.Recargos > 0)
                {
                    totalesTable.Cell().Padding(5).AlignRight().Text("Recargos:").Bold().FontColor(Colors.Red.Medium);
                    totalesTable.Cell().Padding(5).AlignRight().Text($"+${recibo.Recargos:N2}").FontColor(Colors.Red.Medium);
                }

                totalesTable.Cell().Background(ColorAzulOscuro).Padding(8).AlignRight()
                    .Text("TOTAL:").FontColor(Colors.White).Bold();
                totalesTable.Cell().Background(ColorAzulOscuro).Padding(8).AlignRight()
                    .Text($"${recibo.Total:N2}").FontColor(Colors.White).Bold().FontSize(12);
            });

            column.Item().PaddingTop(15).Border(1).BorderColor(ColorGris).Padding(10).Column(letraCol =>
            {
                letraCol.Item().Text("CANTIDAD CON LETRA:").FontSize(8).Bold().FontColor(ColorAzulOscuro);
                letraCol.Item().PaddingTop(3).Text(ConvertirNumeroALetras(recibo.Total) + " M.N.")
                    .FontSize(10).Italic();
            });

            if (recibo.Saldo > 0)
            {
                column.Item().PaddingTop(10).Background("#FFF3E0").Padding(10).Row(row =>
                {
                    row.RelativeItem().Text(t =>
                    {
                        t.Span("Saldo Pendiente: ").Bold().FontColor("#E65100");
                        t.Span($"${recibo.Saldo:N2}").FontColor("#E65100").Bold().FontSize(12);
                    });
                });
            }

            if (recibo.EstaPagado)
            {
                column.Item().PaddingTop(20).AlignCenter().Layers(layers =>
                {
                    layers.PrimaryLayer().Element(stampContainer =>
                    {
                        stampContainer.Border(4).BorderColor(Colors.Green.Medium).Padding(15)
                            .Text("PAGADO").FontSize(32).Bold().FontColor(Colors.Green.Medium);
                    });
                });

                column.Item().PaddingTop(5).AlignCenter().Text(t =>
                {
                    t.Span("Pagado el: ").FontSize(9).FontColor(ColorGris);
                    t.Span(recibo.FechaPago?.ToString("dd/MM/yyyy HH:mm") ?? "").FontSize(9).Bold();
                });
            }

            if (!string.IsNullOrEmpty(recibo.Notas))
            {
                column.Item().PaddingTop(15).Column(notasCol =>
                {
                    notasCol.Item().Text("Observaciones:").FontSize(8).Bold().FontColor(ColorGris);
                    notasCol.Item().PaddingTop(3).Text(recibo.Notas).FontSize(9);
                });
            }
        });
    }

    private void ComposeReciboFooter(IContainer container, ReciboPdfDto recibo)
    {
        container.Column(column =>
        {
            column.Item().LineHorizontal(1).LineColor(ColorGris);

            column.Item().PaddingTop(8).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("INFORMACIÓN DE CONTACTO").FontSize(8).Bold().FontColor(ColorAzulOscuro);
                    col.Item().Text(recibo.Institucion?.Telefono ?? "Tel: (477) 123-4567")
                        .FontSize(8).FontColor(ColorGris);
                    col.Item().Text(recibo.Institucion?.Email ?? "cobranza@usag.edu.mx")
                        .FontSize(8).FontColor(ColorGris);
                });

                row.RelativeItem().AlignRight().Column(col =>
                {
                    col.Item().AlignRight().Text("Este documento es un comprobante oficial")
                        .FontSize(7).Italic().FontColor(ColorGris);
                    col.Item().AlignRight().Text(t =>
                    {
                        t.Span("Generado: ").FontSize(7).FontColor(ColorGris);
                        t.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm")).FontSize(7).FontColor(ColorGris);
                    });
                    col.Item().AlignRight().Text(t =>
                    {
                        t.CurrentPageNumber().FontSize(7).FontColor(ColorGris);
                        t.Span(" de ").FontSize(7).FontColor(ColorGris);
                        t.TotalPages().FontSize(7).FontColor(ColorGris);
                    });
                });
            });
        });
    }

    private string ConvertirNumeroALetras(decimal numero)
    {
        if (numero == 0)
            return "CERO PESOS 00/100";

        string[] unidades = { "", "UN", "DOS", "TRES", "CUATRO", "CINCO", "SEIS", "SIETE", "OCHO", "NUEVE" };
        string[] decenas = { "", "DIEZ", "VEINTE", "TREINTA", "CUARENTA", "CINCUENTA", "SESENTA", "SETENTA", "OCHENTA", "NOVENTA" };
        string[] especiales = { "DIEZ", "ONCE", "DOCE", "TRECE", "CATORCE", "QUINCE", "DIECISÉIS", "DIECISIETE", "DIECIOCHO", "DIECINUEVE" };
        string[] centenas = { "", "CIENTO", "DOSCIENTOS", "TRESCIENTOS", "CUATROCIENTOS", "QUINIENTOS", "SEISCIENTOS", "SETECIENTOS", "OCHOCIENTOS", "NOVECIENTOS" };

        long parteEntera = (long)Math.Floor(numero);
        int centavos = (int)Math.Round((numero - parteEntera) * 100);

        string resultado = "";

        if (parteEntera >= 1000)
        {
            long miles = parteEntera / 1000;
            parteEntera %= 1000;

            if (miles == 1)
                resultado += "MIL ";
            else
            {
                resultado += ConvertirCientos(miles, unidades, decenas, especiales, centenas);
                resultado += " MIL ";
            }
        }

        if (parteEntera > 0)
        {
            if (parteEntera == 100)
                resultado += "CIEN";
            else
                resultado += ConvertirCientos(parteEntera, unidades, decenas, especiales, centenas);
        }

        resultado = resultado.Trim();
        if (string.IsNullOrEmpty(resultado))
            resultado = "CERO";

        return $"{resultado} PESOS {centavos:D2}/100";
    }

    private string ConvertirCientos(long numero, string[] unidades, string[] decenas, string[] especiales, string[] centenas)
    {
        string resultado = "";

        int c = (int)(numero / 100);
        int d = (int)((numero % 100) / 10);
        int u = (int)(numero % 10);

        if (c > 0)
            resultado += centenas[c] + " ";

        if (d == 1)
        {
            resultado += especiales[u];
            return resultado.Trim();
        }

        if (d == 2 && u > 0)
        {
            resultado += "VEINTI" + unidades[u].ToLower();
            return resultado.Trim();
        }

        if (d > 0)
        {
            resultado += decenas[d];
            if (u > 0)
                resultado += " Y ";
        }

        if (u > 0)
            resultado += unidades[u];

        return resultado.Trim();
    }

    #endregion
}
