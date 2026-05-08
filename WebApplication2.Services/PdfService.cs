using Microsoft.AspNetCore.Hosting;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.DTOs.Admision;
using WebApplication2.Core.DTOs.Comprobante;
using WebApplication2.Core.DTOs.Documentos;
using WebApplication2.Core.DTOs.Recibo;
using WebApplication2.Core.DTOs.TarifaAdmision;
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
                page.MarginTop(20);
                page.MarginBottom(15);
                page.MarginHorizontal(25);
                page.DefaultTextStyle(x => x.FontSize(8).FontFamily(FontePrincipal).Bold());

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
            row.ConstantItem(60).BorderRight(0.5f).BorderColor(ColorBorde)
                .AlignCenter().AlignMiddle().Padding(2).Column(logoCol =>
                {
                    if (File.Exists(_logoPath))
                        logoCol.Item().MaxHeight(35).Image(_logoPath).FitArea();
                });

            row.ConstantItem(350).Border(0.5f).BorderColor(ColorBorde)
                .Background(ColorTitulo).AlignCenter().AlignMiddle()
                .Text("FORMATO DEL ASPIRANTE").Bold().FontSize(13).FontColor(Colors.White);

            row.RelativeItem().Column(infoCol =>
            {
                infoCol.Item().Border(0.5f).BorderColor(ColorBorde).MinHeight(14)
                    .PaddingHorizontal(4).AlignMiddle().Row(r =>
                    {
                        r.AutoItem().Text("FOLIO: ").Bold().FontSize(7);
                        r.RelativeItem().Text(folio).Bold().FontSize(7);
                    });
                infoCol.Item().Border(0.5f).BorderColor(ColorBorde).MinHeight(14)
                    .PaddingHorizontal(4).AlignMiddle().Row(r =>
                    {
                        r.AutoItem().Text("PERIODO: ").Bold().FontSize(7);
                        r.RelativeItem().Text(periodo).Bold().FontSize(7);
                    });
                infoCol.Item().Border(0.5f).BorderColor(ColorBorde).MinHeight(14)
                    .PaddingHorizontal(4).AlignMiddle().Row(r =>
                    {
                        r.AutoItem().Text("FECHA: ").Bold().FontSize(7);
                        r.RelativeItem().Text(fecha).Bold().FontSize(7);
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
                FichaFilasNotas(col, ficha);
            });
    }

    private void FichaFilasNotas(ColumnDescriptor col, FichaAdmisionDto ficha)
    {
        if (string.IsNullOrWhiteSpace(ficha.Observaciones))
            return;

        col.Item().BorderTop(0.5f).BorderColor(ColorBorde)
            .Background(ColorSeccionHeader).MinHeight(14)
            .AlignCenter().AlignMiddle()
            .Text("NOTAS ADICIONALES").FontSize(8);

        col.Item().BorderTop(0.5f).BorderColor(ColorBorde)
            .PaddingHorizontal(6).PaddingVertical(4)
            .Text(ficha.Observaciones).FontSize(7);
    }

    private void FichaFilasDatosGenerales(ColumnDescriptor col, FichaAdmisionDto ficha)
    {
        var dp = ficha.DatosPersonales;
        var dc = ficha.DatosContacto;
        var dir = dc?.Direccion;
        col.Item().Background(ColorSeccionHeader).MinHeight(14)
            .AlignCenter().AlignMiddle()
            .Text("DATOS GENERALES DEL ALUMNO").FontSize(8);

        col.Item().BorderTop(0.5f).BorderColor(ColorBorde).MinHeight(14).Row(row =>
        {
            row.ConstantItem(370).BorderRight(0.5f).BorderColor(ColorBorde)
                .PaddingHorizontal(3).AlignMiddle()
                .Text($"NOMBRE: {dp?.NombreCompleto ?? ""}").FontSize(7);
            row.ConstantItem(100).BorderRight(0.5f).BorderColor(ColorBorde)
                .PaddingHorizontal(3).AlignMiddle()
                .Text($"GENERO: {dp?.Genero ?? ""}").FontSize(7);
            row.RelativeItem().PaddingHorizontal(3).AlignMiddle()
                .Text($"EDAD: {dp?.Edad?.ToString() ?? ""}").FontSize(7);
        });

        col.Item().BorderTop(0.5f).BorderColor(ColorBorde).MinHeight(14).Row(row =>
        {
            row.ConstantItem(141).BorderRight(0.5f).BorderColor(ColorBorde)
                .PaddingHorizontal(3).AlignMiddle()
                .Text($"ESTADO CIVIL: {dp?.EstadoCivil ?? ""}").FontSize(7);
            row.ConstantItem(200).BorderRight(0.5f).BorderColor(ColorBorde)
                .PaddingHorizontal(3).AlignMiddle()
                .Text($"CALLE: {dir?.Calle ?? ""}").FontSize(7);
            row.ConstantItem(60).BorderRight(0.5f).BorderColor(ColorBorde)
                .PaddingHorizontal(3).AlignMiddle()
                .Text($"NO. EXT: {dir?.NumeroExterior ?? ""}").FontSize(7);
            row.ConstantItem(60).BorderRight(0.5f).BorderColor(ColorBorde)
                .PaddingHorizontal(3).AlignMiddle()
                .Text($"NO. INT: {dir?.NumeroInterior ?? ""}").FontSize(7);
            row.RelativeItem().PaddingHorizontal(3).AlignMiddle()
                .Text($"COLONIA: {dir?.Colonia ?? ""}").FontSize(7);
        });

        col.Item().BorderTop(0.5f).BorderColor(ColorBorde).MinHeight(14).Row(row =>
        {
            row.ConstantItem(77).BorderRight(0.5f).BorderColor(ColorBorde)
                .PaddingHorizontal(3).AlignMiddle()
                .Text($"C.P. {dir?.CodigoPostal ?? ""}").FontSize(7);
            row.ConstantItem(175).BorderRight(0.5f).BorderColor(ColorBorde)
                .PaddingHorizontal(3).AlignMiddle()
                .Text($"TELEFONO: {dc?.Telefono ?? ""}").FontSize(7);
            row.RelativeItem().PaddingHorizontal(3).AlignMiddle()
                .Text($"CORREO: {dc?.Email ?? ""}").FontSize(7);
        });

        col.Item().BorderTop(0.5f).BorderColor(ColorBorde).MinHeight(14).Row(row =>
        {
            row.ConstantItem(190).BorderRight(0.5f).BorderColor(ColorBorde)
                .PaddingHorizontal(3).AlignMiddle()
                .Text($"NACIONALIDAD: {dp?.Nacionalidad ?? ""}").FontSize(7);
            row.ConstantItem(200).BorderRight(0.5f).BorderColor(ColorBorde)
                .PaddingHorizontal(3).AlignMiddle()
                .Text($"CURP: {dp?.CURP ?? ""}").FontSize(7);
            row.RelativeItem().PaddingHorizontal(3).AlignMiddle()
                .Text($"CIUDAD Y ESTADO: {(dir != null ? $"{dir.Municipio}, {dir.Estado}" : "")}").FontSize(7);
        });

        col.Item().BorderTop(0.5f).BorderColor(ColorBorde).MinHeight(14).Row(row =>
        {
            row.ConstantItem(253).BorderRight(0.5f).BorderColor(ColorBorde)
                .PaddingHorizontal(3).AlignMiddle()
                .Text($"TELEFONO ALTERNO: {dc?.Celular ?? ""}").FontSize(7);
            row.RelativeItem().PaddingHorizontal(3).AlignMiddle()
                .Text($"PARENTESCO: {dc?.ParentescoContactoEmergencia ?? ""}").FontSize(7);
        });

    }

    private void FichaFilasProgramaEducativo(ColumnDescriptor col, FichaAdmisionDto ficha)
    {
        var ia = ficha.InformacionAcademica;
        var seg = ficha.Seguimiento;
        var recorrido = ia?.RecorridoPlantel;

        col.Item().BorderTop(0.5f).BorderColor(ColorBorde)
            .Background(ColorSeccionHeader).MinHeight(14)
            .AlignCenter().AlignMiddle()
            .Text("DATOS DEL PROGRAMA EDUCATIVO").FontSize(8);

        col.Item().BorderTop(0.5f).BorderColor(ColorBorde).MinHeight(14).Row(row =>
        {
            row.RelativeItem().PaddingHorizontal(3).AlignMiddle()
                .Text($"PROGRAMA DE INTERES: {ia?.NombrePlan ?? ""}").FontSize(7);
        });

        col.Item().BorderTop(0.5f).BorderColor(ColorBorde).MinHeight(14).Row(row =>
        {
            row.RelativeItem().PaddingHorizontal(3).AlignMiddle()
                .Text($"INSTITUCION DE PROCEDENCIA: {ia?.InstitucionProcedencia ?? ""}").FontSize(7);
        });

        col.Item().BorderTop(0.5f).BorderColor(ColorBorde).MinHeight(14).Row(row =>
        {
            row.ConstantItem(63).BorderRight(0.5f).BorderColor(ColorBorde)
                .PaddingHorizontal(3).AlignMiddle().Text("CAMPUS:").FontSize(7);
            row.ConstantItem(112).BorderRight(0.5f).BorderColor(ColorBorde)
                .PaddingHorizontal(3).AlignMiddle().Text((ia?.Campus ?? "").Replace("Campus ", "", StringComparison.OrdinalIgnoreCase)).FontSize(7);
            row.ConstantItem(126).BorderRight(0.5f).BorderColor(ColorBorde)
                .PaddingHorizontal(3).AlignMiddle()
                .Text($"TURNO: {ia?.Turno ?? ""}").FontSize(7);
            row.RelativeItem().PaddingHorizontal(3).AlignMiddle()
                .Text($"MODALIDAD: {ia?.Modalidad ?? ""}").FontSize(7);
        });

        col.Item().BorderTop(0.5f).BorderColor(ColorBorde).MinHeight(14).Row(row =>
        {
            row.ConstantItem(200).BorderRight(0.5f).BorderColor(ColorBorde)
                .PaddingHorizontal(3).AlignMiddle()
                .Text($"DIAS: {ia?.Dias ?? ""}").FontSize(7);
            row.ConstantItem(121).BorderRight(0.5f).BorderColor(ColorBorde)
                .PaddingHorizontal(2).AlignMiddle()
                .Text("RECORRIDO POR PLANTEL").FontSize(6);
            row.ConstantItem(28).BorderRight(0.5f).BorderColor(ColorBorde)
                .AlignMiddle().AlignCenter()
                .Text(recorrido == true ? "SI X" : "SI").FontSize(7);
            row.RelativeItem().AlignMiddle().AlignCenter()
                .Text(recorrido == false ? "NO X" : "NO").FontSize(7);
        });

        col.Item().BorderTop(0.5f).BorderColor(ColorBorde).MinHeight(14).Row(row =>
        {
            row.RelativeItem().PaddingHorizontal(3).AlignMiddle()
                .Text($"COMO NOS CONOCISTE: {seg?.MedioContacto ?? ""}").FontSize(7);
        });
    }

    private void FichaFilasSocioeconomicos(ColumnDescriptor col, FichaAdmisionDto ficha)
    {
        var se = ficha.DatosSocioeconomicos;

        col.Item().BorderTop(0.5f).BorderColor(ColorBorde)
            .Background(ColorSeccionHeader).MinHeight(14)
            .AlignCenter().AlignMiddle()
            .Text("DATOS SOCIOECONOMICOS").FontSize(8);

        col.Item().BorderTop(0.5f).BorderColor(ColorBorde).MinHeight(14).Row(row =>
        {
            row.ConstantItem(70).BorderRight(0.5f).BorderColor(ColorBorde)
                .PaddingHorizontal(3).AlignMiddle().Text("TRABAJAS:").FontSize(7);
            row.ConstantItem(27).BorderRight(0.5f).BorderColor(ColorBorde)
                .AlignMiddle().AlignCenter()
                .Text(se?.Trabaja == true ? "SI X" : "SI").FontSize(7);
            row.ConstantItem(36).BorderRight(0.5f).BorderColor(ColorBorde)
                .AlignMiddle().AlignCenter()
                .Text(se?.Trabaja == false ? "NO X" : "NO").FontSize(7);
            row.RelativeItem().PaddingHorizontal(3).AlignMiddle()
                .Text($"EMPRESA: {se?.NombreEmpresa ?? ""}").FontSize(7);
        });

        col.Item().BorderTop(0.5f).BorderColor(ColorBorde).MinHeight(14).Row(row =>
        {
            row.ConstantItem(323).BorderRight(0.5f).BorderColor(ColorBorde)
                .PaddingHorizontal(3).AlignMiddle()
                .Text($"DOMICILIO: {se?.DomicilioEmpresa ?? ""}").FontSize(7);
            row.RelativeItem().PaddingHorizontal(3).AlignMiddle()
                .Text($"PUESTO: {se?.PuestoEmpresa ?? ""}").FontSize(7);
        });

        col.Item().BorderTop(0.5f).BorderColor(ColorBorde).MinHeight(14).Row(row =>
        {
            row.RelativeItem().PaddingHorizontal(3).AlignMiddle()
                .Text($"QUIEN CUBRIRA TUS GASTOS: {se?.QuienCubreGastos ?? ""}").FontSize(7);
        });
    }

    private void FichaFilasFinancieros(ColumnDescriptor col, FichaAdmisionDto ficha)
    {
        var pagos = ficha.InformacionPagos;
        var costos = pagos?.CostosDesglose ?? new List<CostoDesglosePdfDto>();
        var tieneConvenio = pagos?.TieneConvenio ?? false;

        var tituloFinanciero = tieneConvenio ? "DATOS FINANCIEROS — CONVENIO" : "DATOS FINANCIEROS";

        col.Item().BorderTop(0.5f).BorderColor(ColorBorde)
            .Background(ColorSeccionHeader).MinHeight(14)
            .AlignCenter().AlignMiddle()
            .Text(tituloFinanciero).FontSize(8);

        col.Item().BorderTop(0.5f).BorderColor(ColorBorde).MinHeight(13).Row(row =>
        {
            row.ConstantItem(250).BorderRight(0.5f).BorderColor(ColorBorde)
                .AlignMiddle().AlignCenter().Text("CONCEPTO").FontSize(7);
            row.ConstantItem(80).BorderRight(0.5f).BorderColor(ColorBorde)
                .AlignMiddle().AlignCenter().Text("MONTO").FontSize(7);
            row.RelativeItem().AlignMiddle().AlignCenter()
                .Text("NOTAS").FontSize(7);
        });

        col.Item().BorderTop(0.5f).BorderColor(ColorBorde).Column(allRows =>
        {
            decimal totalCostos = 0;
            for (int i = 0; i < costos.Count; i++)
            {
                var costo = costos[i];
                if (costo.Monto.HasValue) totalCostos += costo.Monto.Value;

                var item = i > 0
                    ? allRows.Item().BorderTop(0.5f).BorderColor(ColorBorde)
                    : allRows.Item();

                var notaTexto = costo.Nota ?? "";
                var notaColor = "#666666";
                if (notaTexto.StartsWith("Pagado")) notaColor = "#16a34a";
                else if (notaTexto.StartsWith("Pendiente")) notaColor = "#d97706";
                else if (notaTexto.StartsWith("Vencido")) notaColor = "#dc2626";
                else if (notaTexto.StartsWith("Cancelado")) notaColor = "#9ca3af";
                else if (notaTexto.Contains("Descuento")) notaColor = "#2563eb";

                item.MinHeight(13).Row(r =>
                {
                    r.ConstantItem(250).BorderRight(0.5f).BorderColor(ColorBorde)
                        .PaddingHorizontal(4).AlignMiddle()
                        .Text(costo.Concepto).FontSize(7);
                    r.ConstantItem(80).BorderRight(0.5f).BorderColor(ColorBorde)
                        .PaddingHorizontal(4).AlignMiddle().AlignRight()
                        .Text(costo.Monto.HasValue ? $"${costo.Monto.Value:N2}" : "$0.00").FontSize(7);
                    r.RelativeItem().PaddingHorizontal(4).AlignMiddle()
                        .Text(notaTexto).FontSize(6).FontColor(notaColor);
                });
            }

            allRows.Item().BorderTop(1f).BorderColor(ColorBorde).MinHeight(14)
                .Background(ColorGrisClaro).Row(r =>
            {
                r.ConstantItem(250).BorderRight(0.5f).BorderColor(ColorBorde)
                    .PaddingHorizontal(4).AlignMiddle().AlignRight()
                    .Text("TOTAL:").Bold().FontSize(8);
                r.ConstantItem(80).BorderRight(0.5f).BorderColor(ColorBorde)
                    .PaddingHorizontal(4).AlignMiddle().AlignRight()
                    .Text($"${totalCostos:N2}").Bold().FontSize(8);
                r.RelativeItem();
            });

            allRows.Item().BorderTop(0.5f).BorderColor(ColorBorde).MinHeight(13).Row(r =>
            {
                r.ConstantItem(250).BorderRight(0.5f).BorderColor(ColorBorde)
                    .PaddingHorizontal(4).AlignMiddle()
                    .Text("CONVENIO").FontSize(7);
                r.ConstantItem(80).BorderRight(0.5f).BorderColor(ColorBorde)
                    .PaddingHorizontal(4).AlignMiddle().AlignCenter()
                    .Text(tieneConvenio ? "SI" : "NO").Bold().FontSize(7);
                r.RelativeItem();
            });
        });
    }

    private void FichaFirmas(IContainer container, FichaAdmisionDto ficha)
    {
        var nombreAspirante = ficha.DatosPersonales?.NombreCompleto ?? "";
        var nombreEntrevistador = ficha.Seguimiento?.AsesorAsignado?.NombreCompleto ?? "";

        container.PaddingTop(15).Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().PaddingBottom(20).Text("");
                col.Item().PaddingHorizontal(15).LineHorizontal(0.5f).LineColor(Colors.Black);
                col.Item().AlignCenter().PaddingTop(2).Text(nombreAspirante).Bold().FontSize(7);
                col.Item().AlignCenter().PaddingTop(1).Text("Firma del aspirante").FontSize(6);
            });

            row.ConstantItem(40);

            row.RelativeItem().Column(col =>
            {
                col.Item().PaddingBottom(20).Text("");
                col.Item().PaddingHorizontal(15).LineHorizontal(0.5f).LineColor(Colors.Black);
                col.Item().AlignCenter().PaddingTop(2).Text(nombreEntrevistador).Bold().FontSize(7);
                col.Item().AlignCenter().PaddingTop(1).Text("Entrevistador").FontSize(6);
            });
        });
    }

    private void FichaAvisoPrivacidad(IContainer container)
    {
        container.PaddingTop(4).Column(col =>
        {
            col.Item().Text("Declaro bajo protesta de decir verdad que la información y documentación proporcionada es verídica, por lo que, en caso de existir falsedad en ella, tengo pleno conocimiento que se aplicarán las sanciones administrativas y penas establecidas en los ordenamientos respectivos para quienes se conducen con falsedad ante la autoridad competente.")
                .FontSize(5);
            col.Item().PaddingTop(1)
                .Text("Usted puede consultar en cualquier momento nuestro Aviso de Privacidad en la página de internet https://usaguanajuato.edu.mx; o https://usaguanajuato.edu.mx/docs/AVISO%DE%20PRIVACIDAD.PDF")
                .FontSize(5);
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
                    col.Item().AlignCenter().Text("DEPARTAMENTO DE CONTROL ESCOLAR")
                        .FontSize(10).SemiBold().FontColor(ColorAzulClaro);
                });

                row.ConstantItem(100).AlignRight().Column(col =>
                {
                    col.Item().Text($"Folio: {folio}").FontSize(8).Bold();
                    col.Item().Text($"Fecha: {DateTime.Now:dd/MM/yyyy}").FontSize(8);
                });
            });

            column.Item().PaddingTop(8).LineHorizontal(2).LineColor(ColorAzulOscuro);

            column.Item().PaddingTop(10).Column(dataCol =>
            {
                dataCol.Item().Text(t => { t.Span("RVOE: ").Bold(); t.Span(kardex.RVOE ?? "________________"); });
                dataCol.Item().PaddingTop(5).Text(t => { t.Span("NOMBRE DEL ALUMNO: ").Bold(); t.Span(kardex.NombreCompleto.ToUpper()); });
                dataCol.Item().PaddingTop(5).Text(t => { t.Span("CARRERA: ").Bold(); t.Span(kardex.Carrera.ToUpper()); });
                dataCol.Item().PaddingTop(5).Row(row =>
                {
                    row.RelativeItem().Text(t => { t.Span("CICLO ESCOLAR DE INGRESO: ").Bold(); t.Span(kardex.FechaIngreso.ToString("dd/MM/yyyy")); });
                    row.RelativeItem().Text(t => { t.Span("MATRÍCULA: ").Bold(); t.Span(kardex.Matricula); });
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

            column.Item().PaddingTop(30).AlignCenter().Column(col =>
            {
                col.Item().AlignCenter().LineHorizontal(1).LineColor(Colors.Black);
                col.Item().PaddingTop(5).AlignCenter().Text("DIRECCIÓN DE SERVICIOS ESCOLARES").Bold().FontSize(9);
            });
        });
    }

    private static readonly string[] CuatrimestreNombres = {
        "", "PRIMER", "SEGUNDO", "TERCER", "CUARTO", "QUINTO", "SEXTO",
        "SÉPTIMO", "OCTAVO", "NOVENO", "DÉCIMO", "UNDÉCIMO", "DUODÉCIMO"
    };

    private void ComposeKardexPeriodo(IContainer container, KardexPeriodoDto periodo)
    {
        container.Column(column =>
        {
            int.TryParse(periodo.Periodo, out var cuatNum);

            var cuatLabel = cuatNum > 0 && cuatNum < CuatrimestreNombres.Length
                ? $"{CuatrimestreNombres[cuatNum]} CUATRIMESTRE"
                : $"{periodo.Periodo}° CUATRIMESTRE";

            column.Item().Background(ColorAzulOscuro).Padding(5).Text(cuatLabel).FontColor(Colors.White).Bold().FontSize(9);

            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);
                    columns.ConstantColumn(65);
                    columns.ConstantColumn(55);
                    columns.ConstantColumn(40);
                    columns.ConstantColumn(40);
                    columns.ConstantColumn(40);
                    columns.ConstantColumn(40);
                    columns.ConstantColumn(50);
                });

                table.Header(header =>
                {
                    header.Cell().RowSpan(2).Background(ColorGrisClaro).Padding(3).AlignMiddle().Text("MATERIA").Bold().FontSize(7);
                    header.Cell().RowSpan(2).Background(ColorGrisClaro).Padding(3).AlignMiddle().AlignCenter().Text("CLAVE").Bold().FontSize(7);
                    header.Cell().RowSpan(2).Background(ColorGrisClaro).Padding(3).AlignMiddle().AlignCenter().Text("SERIACIÓN").Bold().FontSize(7);
                    header.Cell().ColumnSpan(2).Background(ColorGrisClaro).Padding(2).AlignCenter().Text("EXAMEN ORDINARIO").Bold().FontSize(6);
                    header.Cell().ColumnSpan(2).Background(ColorGrisClaro).Padding(2).AlignCenter().Text("EXAMEN EXTRAORDINARIO").Bold().FontSize(6);
                    header.Cell().RowSpan(2).Background(ColorGrisClaro).Padding(3).AlignMiddle().AlignCenter().Text("PROMEDIO").Bold().FontSize(7);

                    header.Cell().Background("#E8EAF6").Padding(2).AlignCenter().Text("CICLO").FontSize(6);
                    header.Cell().Background("#E8EAF6").Padding(2).AlignCenter().Text("CALIF.").FontSize(6);
                    header.Cell().Background("#E8EAF6").Padding(2).AlignCenter().Text("CICLO").FontSize(6);
                    header.Cell().Background("#E8EAF6").Padding(2).AlignCenter().Text("CALIF.").FontSize(6);
                });

                foreach (var materia in periodo.Materias)
                {
                    var calif = materia.CalificacionFinal?.ToString("F1") ?? "";
                    var ciclo = materia.Ciclo ?? "";
                    var promedio = materia.CalificacionFinal?.ToString("F1") ?? "";

                    table.Cell().BorderBottom(1).BorderColor(ColorGrisClaro).Padding(3).Text(materia.NombreMateria).FontSize(7);
                    table.Cell().BorderBottom(1).BorderColor(ColorGrisClaro).Padding(3).AlignCenter().Text(materia.ClaveMateria).FontSize(7);
                    table.Cell().BorderBottom(1).BorderColor(ColorGrisClaro).Padding(3).AlignCenter().Text("").FontSize(7);
                    table.Cell().BorderBottom(1).BorderColor(ColorGrisClaro).Padding(3).AlignCenter().Text(ciclo).FontSize(6);
                    table.Cell().BorderBottom(1).BorderColor(ColorGrisClaro).Padding(3).AlignCenter().Text(calif).FontSize(7).Bold();
                    table.Cell().BorderBottom(1).BorderColor(ColorGrisClaro).Padding(3).AlignCenter().Text("").FontSize(7);
                    table.Cell().BorderBottom(1).BorderColor(ColorGrisClaro).Padding(3).AlignCenter().Text("").FontSize(7);
                    table.Cell().BorderBottom(1).BorderColor(ColorGrisClaro).Padding(3).AlignCenter().Text(promedio).FontSize(7).Bold();
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
                text.Span("Por medio de la presente la que suscribe, en mi carácter de Directora de Servicios Escolares de la ");
                text.Span("Universidad San Andrés de Guanajuato").Bold();
                text.Span(", con clave de centro de trabajo C.C.T: ");
                text.Span("11PSU0329U").Bold();
                text.Span(".");
            });

            column.Item().PaddingTop(20).AlignCenter().Text("H A C E   C O N S T A R").Bold().FontSize(13).FontColor(ColorAzulOscuro);

            column.Item().PaddingTop(20).Text(text =>
            {
                text.Span("Según Historial Académico que obra en el departamento de Dirección de Servicios Escolares, que el (la) Alumno(a) C. ");
                text.Span(constancia.NombreCompleto.ToUpper()).Bold();
                if (!string.IsNullOrEmpty(constancia.Curp))
                {
                    text.Span(", CURP: ");
                    text.Span(constancia.Curp.ToUpper()).Bold();
                }
                text.Span(", se encuentra inscrito(a) en la ");
                text.Span(constancia.Carrera).Bold();
                if (!string.IsNullOrEmpty(constancia.RVOE))
                {
                    text.Span(", RVOE: ");
                    text.Span(constancia.RVOE).Bold();
                }
                text.Span(", ");
                text.Span(constancia.Grado).Bold();
                text.Span(", en el periodo comprendido ");
                text.Span(constancia.PeriodoActual).Bold();
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
                col.Item().AlignCenter().Text("Atentamente").Bold();
                col.Item().PaddingTop(40).AlignCenter().LineHorizontal(1).LineColor(Colors.Black);
                col.Item().PaddingTop(5).AlignCenter().Text("Lic. Margarita Anda Valdez").Bold().FontSize(10);
                col.Item().AlignCenter().Text("Directora de Servicios Escolares").FontSize(10);
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

                if (!string.IsNullOrEmpty(recibo.NombreEmpresa))
                {
                    dataCol.Item().PaddingTop(5).Row(row =>
                    {
                        row.RelativeItem().Text(t =>
                        {
                            t.Span("Empresa: ").Bold();
                            t.Span(recibo.NombreEmpresa);
                        });
                    });
                }
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
                    // Extraer nombre real de la promoción desde Notas si existe
                    var etiquetaDescuento = "Descuento:";
                    if (!string.IsNullOrEmpty(recibo.Notas))
                    {
                        var idx = recibo.Notas.IndexOf("Descuento por ", StringComparison.OrdinalIgnoreCase);
                        if (idx >= 0)
                        {
                            var resto = recibo.Notas.Substring(idx + 14);
                            var finIdx = resto.IndexOf(':');
                            if (finIdx > 0)
                            {
                                var nombrePromo = resto.Substring(0, finIdx).Trim();
                                etiquetaDescuento = $"Descuento ({nombrePromo}):";
                            }
                        }
                    }

                    totalesTable.Cell().Padding(5).AlignRight().Text(etiquetaDescuento).Bold().FontColor(Colors.Green.Medium);
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

            if (recibo.EstaCancelado)
            {
                column.Item().PaddingTop(20).AlignCenter().Layers(layers =>
                {
                    layers.PrimaryLayer().Element(stampContainer =>
                    {
                        stampContainer.Border(4).BorderColor(Colors.Red.Medium).Padding(15)
                            .Text("CANCELADO").FontSize(32).Bold().FontColor(Colors.Red.Medium);
                    });
                });
            }
            else if (recibo.EstaPagado)
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
                    col.Item().Text(recibo.Institucion?.Email ?? "cobranza@usaguanajuato.edu.mx")
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

    #region Cotización de Admisión

    public byte[] GenerarCotizacionAdmisionPdf(CotizacionAdmisionPdfDto cotizacion)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.MarginVertical(30);
                page.MarginHorizontal(40);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily(FontePrincipal));

                page.Header().Element(c => ComposeCotizacionHeader(c, cotizacion));
                page.Content().Element(c => ComposeCotizacionContent(c, cotizacion));
                page.Footer().Element(c => ComposeCotizacionFooter(c, cotizacion));
            });
        });

        return document.GeneratePdf();
    }

    private void ComposeCotizacionHeader(IContainer container, CotizacionAdmisionPdfDto cotizacion)
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
                    col.Item().AlignCenter().Text(cotizacion.Institucion?.Nombre ?? "UNIVERSIDAD SAN ANDRÉS DE GUANAJUATO")
                        .FontSize(13).Bold().FontColor(ColorAzulOscuro);
                    col.Item().AlignCenter().Text(cotizacion.Institucion?.Campus ?? "CAMPUS LEÓN")
                        .FontSize(10).SemiBold().FontColor(ColorAzulClaro);
                    col.Item().PaddingTop(4).AlignCenter().Text("COTIZACIÓN DE COSTOS DE ADMISIÓN")
                        .FontSize(11).Bold().FontColor(ColorAzulOscuro);
                });

                row.ConstantItem(100).Column(fechaCol =>
                {
                    fechaCol.Item().AlignRight().Text("FECHA").FontSize(8).FontColor(ColorGris);
                    fechaCol.Item().AlignRight().Text(cotizacion.Fecha.ToString("dd/MM/yyyy"))
                        .FontSize(11).Bold().FontColor(ColorAzulOscuro);
                });
            });

            column.Item().PaddingTop(8).LineHorizontal(2).LineColor(ColorAzulOscuro);

            column.Item().PaddingTop(8).Background(ColorGrisClaro).Padding(10).Column(dataCol =>
            {
                dataCol.Item().Row(row =>
                {
                    row.RelativeItem(2).Text(t =>
                    {
                        t.Span("Aspirante: ").Bold();
                        t.Span(cotizacion.NombreAspirante);
                    });
                });
                dataCol.Item().PaddingTop(4).Row(row =>
                {
                    row.RelativeItem(2).Text(t =>
                    {
                        t.Span("Licenciatura: ").Bold();
                        t.Span(cotizacion.Licenciatura);
                    });
                    row.RelativeItem().AlignRight().Text(t =>
                    {
                        t.Span("Clave: ").Bold();
                        t.Span(cotizacion.ClavePlan);
                    });
                });
                dataCol.Item().PaddingTop(4).Text(t =>
                {
                    t.Span("Tarifa: ").Bold();
                    t.Span(cotizacion.NombreTarifa);
                });
            });

            column.Item().PaddingTop(8).LineHorizontal(1).LineColor(ColorGris);
        });
    }

    private void ComposeCotizacionContent(IContainer container, CotizacionAdmisionPdfDto cotizacion)
    {
        var tienePromociones = cotizacion.Conceptos.Any(c => c.NombrePromocion != null);
        var tieneTotales = cotizacion.TotalOriginal > 0;
        var tieneNotas = !tienePromociones && cotizacion.Conceptos.Any(c => !string.IsNullOrWhiteSpace(c.Notas));

        container.PaddingTop(15).Column(column =>
        {
            // Empresa si aplica
            if (!string.IsNullOrEmpty(cotizacion.NombreEmpresa))
            {
                column.Item().PaddingBottom(8).Background("#F3E8FF").Border(1).BorderColor("#9333EA").Padding(8).Row(row =>
                {
                    row.RelativeItem().Text(t =>
                    {
                        t.Span("EMPRESA: ").Bold().FontSize(9).FontColor("#7C3AED");
                        t.Span(cotizacion.NombreEmpresa).FontSize(9).FontColor("#7C3AED");
                    });
                });
            }

            column.Item().Table(table =>
            {
                if (tienePromociones)
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3);   // Concepto
                        columns.RelativeColumn(1.5f); // Monto original
                        columns.RelativeColumn(3);   // Promoción
                        columns.RelativeColumn(1.5f); // Monto final
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background(ColorAzulOscuro).Padding(8)
                            .Text("CONCEPTO").FontColor(Colors.White).Bold().FontSize(9);
                        header.Cell().Background(ColorAzulOscuro).Padding(8)
                            .Text("MONTO").FontColor(Colors.White).Bold().FontSize(9).AlignRight();
                        header.Cell().Background(ColorAzulOscuro).Padding(8)
                            .Text("CONVENIO").FontColor(Colors.White).Bold().FontSize(9);
                        header.Cell().Background(ColorAzulOscuro).Padding(8)
                            .Text("MONTO FINAL").FontColor(Colors.White).Bold().FontSize(9).AlignRight();
                    });
                }
                else if (tieneNotas)
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(1.5f);
                        columns.RelativeColumn(3);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background(ColorAzulOscuro).Padding(8)
                            .Text("CONCEPTO").FontColor(Colors.White).Bold().FontSize(9);
                        header.Cell().Background(ColorAzulOscuro).Padding(8)
                            .Text("MONTO").FontColor(Colors.White).Bold().FontSize(9).AlignRight();
                        header.Cell().Background(ColorAzulOscuro).Padding(8)
                            .Text("NOTAS").FontColor(Colors.White).Bold().FontSize(9);
                    });
                }
                else
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(2);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background(ColorAzulOscuro).Padding(8)
                            .Text("CONCEPTO").FontColor(Colors.White).Bold().FontSize(9);
                        header.Cell().Background(ColorAzulOscuro).Padding(8)
                            .Text("MONTO").FontColor(Colors.White).Bold().FontSize(9).AlignRight();
                    });
                }

                for (int i = 0; i < cotizacion.Conceptos.Count; i++)
                {
                    var concepto = cotizacion.Conceptos[i];
                    var bgColor = i % 2 == 0 ? ColorGrisClaro : "#FFFFFF";
                    var textColor = concepto.Incluido ? "#000000" : ColorGris;

                    // Columna: Nombre concepto
                    table.Cell().Background(bgColor).Padding(8)
                        .Text(concepto.Nombre).FontSize(9).Bold().FontColor(textColor);

                    if (tienePromociones)
                    {
                        // Columna: Monto original
                        table.Cell().Background(bgColor).Padding(8).AlignRight()
                            .Text(concepto.Incluido ? $"${concepto.Monto:N2}" : "—").FontSize(9).FontColor(textColor);

                        // Columna: Promoción
                        table.Cell().Background(bgColor).Padding(8).Text(t =>
                        {
                            if (concepto.NombrePromocion != null && concepto.Incluido)
                            {
                                t.Span(concepto.NombrePromocion).FontSize(8).FontColor("#16A34A").Italic();
                                t.Span($"  -${concepto.MontoDescuento:N2}").FontSize(8).Bold().FontColor("#DC2626");
                            }
                            else if (concepto.Incluido)
                            {
                                t.Span("Sin promoción").FontSize(8).FontColor(ColorGris).Italic();
                            }
                            else
                            {
                                t.Span("No incluido").FontSize(8).FontColor(ColorGris).Italic();
                            }
                        });

                        // Columna: Monto final
                        table.Cell().Background(bgColor).Padding(8).AlignRight()
                            .Text(concepto.Incluido ? $"${concepto.MontoFinal:N2}" : "—")
                            .FontSize(9).Bold().FontColor(concepto.MontoDescuento > 0 ? "#16A34A" : textColor);
                    }
                    else if (tieneNotas)
                    {
                        table.Cell().Background(bgColor).Padding(8).AlignRight()
                            .Text(concepto.Valor).FontSize(9).FontColor(textColor);
                        table.Cell().Background(bgColor).Padding(8)
                            .Text(concepto.Notas ?? "").FontSize(9).FontColor(textColor);
                    }
                    else
                    {
                        table.Cell().Background(bgColor).Padding(8).AlignRight()
                            .Text(concepto.Valor).FontSize(9).FontColor(textColor);
                    }
                }

                // Fila de totales (solo si hay datos numéricos)
                if (tieneTotales)
                {
                    if (tienePromociones)
                    {
                        // Fila separadora
                        table.Cell().ColumnSpan(4).PaddingTop(2).LineHorizontal(1).LineColor(ColorAzulOscuro);

                        if (cotizacion.TotalDescuento > 0)
                        {
                            // Fila subtotal
                            table.Cell().ColumnSpan(2).Background("#FFFFFF").PaddingHorizontal(8).PaddingVertical(4)
                                .Text("SUBTOTAL").FontSize(9).Bold().FontColor(ColorAzulOscuro);
                            table.Cell().Background("#FFFFFF").PaddingHorizontal(8).PaddingVertical(4);
                            table.Cell().Background("#FFFFFF").PaddingHorizontal(8).PaddingVertical(4).AlignRight()
                                .Text($"${cotizacion.TotalOriginal:N2}").FontSize(9).FontColor(ColorAzulOscuro);

                            // Fila descuento
                            table.Cell().ColumnSpan(2).Background("#FFFFFF").PaddingHorizontal(8).PaddingVertical(4)
                                .Text("DESCUENTO POR CONVENIO").FontSize(9).Bold().FontColor("#DC2626");
                            table.Cell().Background("#FFFFFF").PaddingHorizontal(8).PaddingVertical(4);
                            table.Cell().Background("#FFFFFF").PaddingHorizontal(8).PaddingVertical(4).AlignRight()
                                .Text($"-${cotizacion.TotalDescuento:N2}").FontSize(9).Bold().FontColor("#DC2626");
                        }

                        // Fila total final
                        table.Cell().ColumnSpan(2).Background(ColorAzulOscuro).Padding(8)
                            .Text("TOTAL A PAGAR").FontColor(Colors.White).Bold().FontSize(10);
                        table.Cell().Background(ColorAzulOscuro).Padding(8);
                        table.Cell().Background(ColorAzulOscuro).Padding(8).AlignRight()
                            .Text($"${cotizacion.TotalFinal:N2}").FontColor(Colors.White).Bold().FontSize(10);
                    }
                    else
                    {
                        table.Cell().ColumnSpan(2).PaddingTop(2).LineHorizontal(1).LineColor(ColorAzulOscuro);

                        table.Cell().Background(ColorAzulOscuro).Padding(8)
                            .Text("TOTAL A PAGAR").FontColor(Colors.White).Bold().FontSize(10);
                        table.Cell().Background(ColorAzulOscuro).Padding(8).AlignRight()
                            .Text($"${cotizacion.TotalFinal:N2}").FontColor(Colors.White).Bold().FontSize(10);
                    }
                }
            });

            column.Item().PaddingTop(20).Background("#EFF6FF").Border(1).BorderColor(ColorAzulClaro).Padding(12).Column(notaCol =>
            {
                notaCol.Item().Text("NOTA IMPORTANTE:").FontSize(8).Bold().FontColor(ColorAzulOscuro);
                notaCol.Item().PaddingTop(3).Text(
                    "Los montos indicados en esta cotización son referenciales y están sujetos a cambios sin previo aviso. " +
                    "Esta cotización no representa un compromiso de pago hasta que sea formalizada mediante recibo oficial.")
                    .FontSize(8).FontColor(ColorGris).Italic();
            });
        });
    }

    private void ComposeCotizacionFooter(IContainer container, CotizacionAdmisionPdfDto cotizacion)
    {
        container.Column(column =>
        {
            column.Item().LineHorizontal(1).LineColor(ColorGris);

            column.Item().PaddingTop(8).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("INFORMACIÓN DE CONTACTO").FontSize(8).Bold().FontColor(ColorAzulOscuro);
                    col.Item().Text(cotizacion.Institucion?.Telefono ?? "Tel: (477) 123-4567")
                        .FontSize(8).FontColor(ColorGris);
                    col.Item().Text(cotizacion.Institucion?.Email ?? "cobranza@usaguanajuato.edu.mx")
                        .FontSize(8).FontColor(ColorGris);
                });

                row.RelativeItem().AlignRight().Column(col =>
                {
                    col.Item().AlignRight().Text("Documento generado automáticamente — no requiere firma")
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

    #endregion

    #region Comprobante de Inscripción

    public byte[] GenerarComprobanteInscripcion(ComprobanteInscripcionDto c)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(40);
                page.DefaultTextStyle(t => t.FontFamily(FontePrincipal).FontSize(10));

                page.Header().Column(col =>
                {
                    col.Item().AlignCenter().Text("UNIVERSIDAD SAN ANDRÉS DE GUANAJUATO").Bold().FontSize(14);
                    col.Item().AlignCenter().Text("Comprobante de Inscripción").FontSize(12).FontColor(ColorGris);
                    col.Item().PaddingTop(4).LineHorizontal(1).LineColor(ColorGris);
                });

                page.Content().PaddingVertical(12).Column(col =>
                {
                    col.Spacing(10);

                    col.Item().Background(Colors.Grey.Lighten4).Padding(10).Column(b =>
                    {
                        b.Spacing(4);
                        b.Item().Text("Datos del Estudiante").Bold().FontSize(11);
                        b.Item().Row(r =>
                        {
                            r.RelativeItem().Text(t => { t.Span("Matrícula: ").SemiBold(); t.Span(c.Matricula); });
                            r.RelativeItem().Text(t => { t.Span("Fecha de ingreso: ").SemiBold(); t.Span(c.FechaIngreso.ToString("dd/MM/yyyy")); });
                        });
                        b.Item().Text(t => { t.Span("Nombre: ").SemiBold(); t.Span(c.NombreCompleto); });
                        if (!string.IsNullOrEmpty(c.Curp))
                            b.Item().Text(t => { t.Span("CURP: ").SemiBold(); t.Span(c.Curp); });
                    });

                    col.Item().Background(Colors.Grey.Lighten4).Padding(10).Column(b =>
                    {
                        b.Spacing(4);
                        b.Item().Text("Información Académica").Bold().FontSize(11);
                        b.Item().Text(t => { t.Span("Plan de estudios: ").SemiBold(); t.Span(c.PlanEstudios); });
                        if (!string.IsNullOrEmpty(c.ClavePlanEstudios))
                            b.Item().Text(t => { t.Span("Clave del plan: ").SemiBold(); t.Span(c.ClavePlanEstudios); });
                        if (!string.IsNullOrEmpty(c.Campus))
                            b.Item().Text(t => { t.Span("Campus: ").SemiBold(); t.Span(c.Campus); });
                        if (!string.IsNullOrEmpty(c.Turno))
                            b.Item().Text(t => { t.Span("Turno: ").SemiBold(); t.Span(c.Turno); });
                        if (!string.IsNullOrEmpty(c.PeriodoAcademico))
                            b.Item().Text(t => { t.Span("Periodo académico: ").SemiBold(); t.Span(c.PeriodoAcademico); });
                        if (!string.IsNullOrEmpty(c.GrupoCodigo))
                            b.Item().Text(t =>
                            {
                                t.Span("Grupo: ").SemiBold();
                                t.Span($"{c.GrupoCodigo}");
                                if (!string.IsNullOrEmpty(c.GrupoNombre)) t.Span($" — {c.GrupoNombre}");
                                if (c.NumeroCuatrimestre.HasValue) t.Span($" · Cuatrimestre {c.NumeroCuatrimestre}");
                            });
                    });

                    col.Item().Background("#EEF7FF").Border(1).BorderColor("#4A90E2").Padding(10).Column(b =>
                    {
                        b.Spacing(4);
                        b.Item().Text("Credenciales de Acceso").Bold().FontSize(11).FontColor("#2760A0");
                        b.Item().Text(t => { t.Span("Correo institucional: ").SemiBold(); t.Span(c.CorreoInstitucional); });
                        if (c.IncluyeCredenciales && !string.IsNullOrEmpty(c.PasswordTemporal))
                        {
                            b.Item().Text(t => { t.Span("Contraseña temporal: ").SemiBold(); t.Span(c.PasswordTemporal).FontFamily(FontePrincipal); });
                            b.Item().Text("Importante: el estudiante debe cambiar la contraseña en su primer inicio de sesión.")
                                .FontSize(8).FontColor(ColorGris).Italic();
                        }
                        else
                        {
                            b.Item().Text("La contraseña no se muestra en reimpresiones. Si el estudiante la olvidó, solicite un restablecimiento desde Accesos de Alumnos y Docentes.")
                                .FontSize(8).FontColor(ColorGris).Italic();
                        }
                        if (!string.IsNullOrEmpty(c.UrlPortal))
                            b.Item().Text(t => { t.Span("Portal: ").SemiBold(); t.Span(c.UrlPortal); });
                    });

                    col.Item().PaddingTop(20).Column(f =>
                    {
                        f.Item().LineHorizontal(0.5f).LineColor(ColorGris);
                        f.Item().PaddingTop(30).AlignCenter().Text("_____________________________________").FontColor(ColorGris);
                        f.Item().AlignCenter().Text("Firma del Estudiante").FontSize(9);
                    });
                });

                page.Footer().Column(f =>
                {
                    f.Item().LineHorizontal(0.5f).LineColor(ColorGris);
                    f.Item().PaddingTop(4).Row(r =>
                    {
                        r.RelativeItem().Text(t => { t.Span("Generado: ").FontSize(7).FontColor(ColorGris); t.Span(c.FechaGeneracion.ToString("dd/MM/yyyy HH:mm")).FontSize(7).FontColor(ColorGris); });
                        r.RelativeItem().AlignRight().Text(t => { t.CurrentPageNumber().FontSize(7).FontColor(ColorGris); t.Span(" de ").FontSize(7).FontColor(ColorGris); t.TotalPages().FontSize(7).FontColor(ColorGris); });
                    });
                });
            });
        });

        return document.GeneratePdf();
    }

    #endregion
}
