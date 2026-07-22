using System.Text.Json;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.EntityFrameworkCore;
using SS = DocumentFormat.OpenXml.Spreadsheet;
using WebApplication2.Core.Enums;
using WebApplication2.Core.Models;
using WebApplication2.Data.DbContexts;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Services
{
    public class PlantillaReporteService : IPlantillaReporteService
    {
        private readonly ApplicationDbContext _db;
        private readonly IFormatoDatosService _formatoDatos;
        private readonly string _uploadDir;

        public PlantillaReporteService(ApplicationDbContext db, IFormatoDatosService formatoDatos)
        {
            _db = db;
            _formatoDatos = formatoDatos;
            _uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "plantillas");
            Directory.CreateDirectory(_uploadDir);
        }

        public async Task<List<PlantillaReporte>> ListarAsync(string? categoria = null, CancellationToken ct = default)
        {
            var query = _db.PlantillaReportes.Where(p => p.Status == StatusEnum.Active);
            if (!string.IsNullOrWhiteSpace(categoria))
                query = query.Where(p => p.Categoria == categoria);
            return await query.OrderBy(p => p.Categoria).ThenBy(p => p.Nombre).ToListAsync(ct);
        }

        public async Task<PlantillaReporte?> ObtenerPorCodigoAsync(string codigo, CancellationToken ct = default)
        {
            return await _db.PlantillaReportes
                .FirstOrDefaultAsync(p => p.Codigo == codigo && p.Status == StatusEnum.Active && p.Activa, ct);
        }

        public async Task<PlantillaReporte> CrearAsync(string nombre, string codigo, string categoria, string? descripcion, Stream archivo, string nombreArchivo, string variablesJson, string? origen, string? rolesGenera, CancellationToken ct = default)
        {
            var ext = Path.GetExtension(nombreArchivo).ToLower();
            var fileName = $"{codigo}_{DateTime.UtcNow:yyyyMMddHHmmss}{ext}";
            var filePath = Path.Combine(_uploadDir, fileName);

            using (var fs = new FileStream(filePath, FileMode.Create))
                await archivo.CopyToAsync(fs, ct);

            var plantilla = new PlantillaReporte
            {
                Nombre = nombre,
                Codigo = codigo,
                Categoria = categoria,
                Descripcion = descripcion,
                RutaArchivo = filePath,
                NombreArchivoOriginal = nombreArchivo,
                VariablesDisponibles = variablesJson,
                Origen = string.IsNullOrWhiteSpace(origen) ? null : origen,
                RolesGenera = string.IsNullOrWhiteSpace(rolesGenera) ? null : rolesGenera,
                Activa = true,
                CreatedAt = DateTime.UtcNow,
                Status = StatusEnum.Active
            };

            _db.PlantillaReportes.Add(plantilla);
            await _db.SaveChangesAsync(ct);
            return plantilla;
        }

        public async Task<PlantillaReporte> ActualizarOrigenAsync(int id, string? origen, CancellationToken ct = default)
        {
            var plantilla = await _db.PlantillaReportes.FindAsync(new object[] { id }, ct)
                ?? throw new InvalidOperationException("Plantilla no encontrada");

            plantilla.Origen = string.IsNullOrWhiteSpace(origen) ? null : origen;
            plantilla.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
            return plantilla;
        }

        public async Task<byte[]> GenerarPorEntidadAsync(string codigo, int idEntidad, bool pdf = true, CancellationToken ct = default)
        {
            var plantilla = await ObtenerPorCodigoAsync(codigo, ct)
                ?? throw new InvalidOperationException($"Plantilla '{codigo}' no encontrada o inactiva");

            if (string.IsNullOrWhiteSpace(plantilla.Origen))
                throw new InvalidOperationException("La plantilla no tiene un origen de datos configurado.");

            var datos = await _formatoDatos.ResolverAsync(plantilla.Origen, idEntidad, ct);
            var docBytes = await GenerarDocumentoAsync(codigo, datos.Variables, datos.Tablas, ct);

            if (!pdf)
                return docBytes;

            var ext = Path.GetExtension(plantilla.RutaArchivo)?.ToLowerInvariant() ?? ".docx";
            return await DocxToPdfConverter.ConvertAsync(docBytes, ext, ct);
        }

        public async Task<PlantillaReporte> ActualizarRolesAsync(int id, string? rolesGenera, CancellationToken ct = default)
        {
            var plantilla = await _db.PlantillaReportes.FindAsync(new object[] { id }, ct)
                ?? throw new InvalidOperationException("Plantilla no encontrada");

            plantilla.RolesGenera = string.IsNullOrWhiteSpace(rolesGenera) ? null : rolesGenera;
            plantilla.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
            return plantilla;
        }

        public async Task<List<PlantillaReporte>> ListarPorRolesAsync(IEnumerable<string> roles, CancellationToken ct = default)
        {
            var rolesSet = roles.Select(r => r.Trim().ToLowerInvariant()).Where(r => r.Length > 0).ToHashSet();

            var candidatas = await _db.PlantillaReportes
                .Where(p => p.Status == StatusEnum.Active && p.Activa && p.RolesGenera != null && p.Origen != null)
                .ToListAsync(ct);

            return candidatas
                .Where(p => RolesDePlantilla(p.RolesGenera).Any(rolesSet.Contains))
                .ToList();
        }

        public async Task<byte[]> GenerarMioAsync(string codigo, string userId, IEnumerable<string> roles, CancellationToken ct = default)
        {
            var plantilla = await ObtenerPorCodigoAsync(codigo, ct)
                ?? throw new InvalidOperationException($"Formato '{codigo}' no encontrado o inactivo");

            var rolesSet = roles.Select(r => r.Trim().ToLowerInvariant()).ToHashSet();
            if (!RolesDePlantilla(plantilla.RolesGenera).Any(rolesSet.Contains))
                throw new UnauthorizedAccessException("No tienes permiso para generar este formato.");

            int idEntidad = plantilla.Origen?.ToLowerInvariant() switch
            {
                "estudiante" => await _db.Estudiante.Where(e => e.UsuarioId == userId).Select(e => e.IdEstudiante).FirstOrDefaultAsync(ct),
                "docente" => await _db.Profesor.Where(p => p.UsuarioId == userId).Select(p => p.IdProfesor).FirstOrDefaultAsync(ct),
                _ => throw new InvalidOperationException("Este formato no es de autoservicio.")
            };

            if (idEntidad == 0)
                throw new InvalidOperationException("No se encontró tu perfil para generar el formato.");

            return await GenerarPorEntidadAsync(codigo, idEntidad, true, ct);
        }

        private static IEnumerable<string> RolesDePlantilla(string? rolesGenera) =>
            (rolesGenera ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(r => r.ToLowerInvariant());

        public async Task<PlantillaReporte> ActualizarArchivoAsync(int id, Stream archivo, string nombreArchivo, CancellationToken ct = default)
        {
            var plantilla = await _db.PlantillaReportes.FindAsync(new object[] { id }, ct)
                ?? throw new InvalidOperationException("Plantilla no encontrada");

            var extUpd = Path.GetExtension(nombreArchivo).ToLower();
            var fileName = $"{plantilla.Codigo}_{DateTime.UtcNow:yyyyMMddHHmmss}{extUpd}";
            var filePath = Path.Combine(_uploadDir, fileName);

            using (var fs = new FileStream(filePath, FileMode.Create))
                await archivo.CopyToAsync(fs, ct);

            plantilla.RutaArchivo = filePath;
            plantilla.NombreArchivoOriginal = nombreArchivo;
            plantilla.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
            return plantilla;
        }

        public async Task EliminarAsync(int id, CancellationToken ct = default)
        {
            var plantilla = await _db.PlantillaReportes.FindAsync(new object[] { id }, ct)
                ?? throw new InvalidOperationException("Plantilla no encontrada");
            plantilla.Status = StatusEnum.Deleted;
            plantilla.Activa = false;
            await _db.SaveChangesAsync(ct);
        }

        public async Task<byte[]> GenerarDocumentoAsync(
            string codigo,
            Dictionary<string, string> variables,
            Dictionary<string, List<Dictionary<string, string>>>? tablas = null,
            CancellationToken ct = default)
        {
            var plantilla = await ObtenerPorCodigoAsync(codigo, ct)
                ?? throw new InvalidOperationException($"Plantilla '{codigo}' no encontrada o inactiva");

            if (!File.Exists(plantilla.RutaArchivo))
                throw new InvalidOperationException("Archivo de plantilla no encontrado en el servidor");

            var templateBytes = await File.ReadAllBytesAsync(plantilla.RutaArchivo, ct);

            var ext = Path.GetExtension(plantilla.RutaArchivo)?.ToLowerInvariant();
            if (ext == ".xlsx")
                return GenerarXlsx(templateBytes, variables, tablas);

            using var ms = new MemoryStream();
            ms.Write(templateBytes, 0, templateBytes.Length);
            ms.Position = 0;

            using (var doc = WordprocessingDocument.Open(ms, true))
            {
                var body = doc.MainDocumentPart?.Document?.Body;
                if (body == null) throw new InvalidOperationException("Documento Word inválido");

                if (tablas != null)
                {
                    foreach (var (tablaKey, filas) in tablas)
                    {
                        ProcesarTabla(body, tablaKey, filas);
                    }
                }

                ReemplazarVariables(body, variables);

                foreach (var headerPart in doc.MainDocumentPart!.HeaderParts)
                {
                    ReemplazarVariables(headerPart.Header, variables);
                }
                foreach (var footerPart in doc.MainDocumentPart.FooterParts)
                {
                    ReemplazarVariables(footerPart.Footer, variables);
                }

                doc.MainDocumentPart.Document.Save();
            }

            return ms.ToArray();
        }

        private static byte[] GenerarXlsx(byte[] templateBytes, Dictionary<string, string> variables, Dictionary<string, List<Dictionary<string, string>>>? tablas)
        {
            using var ms = new MemoryStream();
            ms.Write(templateBytes, 0, templateBytes.Length);
            ms.Position = 0;

            using (var doc = SpreadsheetDocument.Open(ms, true))
            {
                var sst = doc.WorkbookPart?.SharedStringTablePart?.SharedStringTable;

                if (doc.WorkbookPart != null)
                {
                    foreach (var wsPart in doc.WorkbookPart.WorksheetParts)
                    {
                        var sheetData = wsPart.Worksheet.GetFirstChild<SS.SheetData>();
                        if (sheetData == null) continue;

                        if (tablas != null)
                        {
                            foreach (var (clave, filas) in tablas)
                                RellenarTablaExcel(wsPart.Worksheet, sheetData, sst, "{{" + clave + "}}", filas, variables);
                        }

                        foreach (var cell in sheetData.Descendants<SS.Cell>().ToList())
                        {
                            var txt = TextoCelda(cell, sst);
                            if (txt.Contains("{{"))
                                EscribirCeldaInline(cell, AplicarVariables(txt, variables));
                        }

                        wsPart.Worksheet.Save();
                    }
                }
            }

            return ms.ToArray();
        }

        private static string TextoCelda(SS.Cell cell, SS.SharedStringTable? sst)
        {
            if (cell.DataType?.Value == SS.CellValues.SharedString && sst != null
                && int.TryParse(cell.CellValue?.Text, out var idx) && idx >= 0 && idx < sst.Count())
                return sst.ElementAt(idx).InnerText;
            if (cell.DataType?.Value == SS.CellValues.InlineString)
                return cell.InnerText;
            return cell.CellValue?.Text ?? "";
        }

        private static void EscribirCeldaInline(SS.Cell cell, string text)
        {
            cell.RemoveAllChildren();
            cell.DataType = SS.CellValues.InlineString;
            cell.Append(new SS.InlineString(new SS.Text(text)));
        }

        private static string ColumnaDeCelda(SS.Cell cell)
        {
            var r = cell.CellReference?.Value ?? "";
            return new string(r.TakeWhile(char.IsLetter).ToArray());
        }

        private static void RellenarTablaExcel(SS.Worksheet worksheet, SS.SheetData sheetData, SS.SharedStringTable? sst, string marcador, List<Dictionary<string, string>> filas, Dictionary<string, string> variables)
        {
            var allRows = sheetData.Elements<SS.Row>().Where(r => r.RowIndex != null).ToList();
            var templateRow = allRows.FirstOrDefault(r => r.Elements<SS.Cell>().Any(c => TextoCelda(c, sst).Contains(marcador)));
            if (templateRow == null) return;
            uint t = templateRow.RowIndex!.Value;

            if (filas.Count == 0)
            {
                foreach (var c in templateRow.Elements<SS.Cell>())
                    if (TextoCelda(c, sst).Contains("{{")) EscribirCeldaInline(c, "");
                return;
            }

            int k = 1;
            foreach (var r in allRows.Where(r => r.RowIndex!.Value > t).OrderBy(r => r.RowIndex!.Value))
            {
                if (r.Elements<SS.Cell>().All(c => string.IsNullOrWhiteSpace(TextoCelda(c, sst)))) k++;
                else break;
            }

            int m = filas.Count;
            int delta = m - k;
            var mergeCells = worksheet.GetFirstChild<SS.MergeCells>();
            var templateMerges = mergeCells?.Elements<SS.MergeCell>()
                .Where(mc => { var (s, e) = MergeFilas(mc); return s == t && e == t; })
                .Select(MergeColumnas).ToList() ?? new List<(string, string)>();

            if (delta != 0)
            {
                var posteriores = allRows.Where(r => r.RowIndex!.Value >= t + (uint)k).ToList();
                foreach (var r in delta > 0 ? posteriores.OrderByDescending(r => r.RowIndex!.Value) : posteriores.OrderBy(r => r.RowIndex!.Value))
                    DesplazarFila(r, delta);
                if (mergeCells != null)
                    foreach (var mc in mergeCells.Elements<SS.MergeCell>().ToList())
                        if (MergeFilas(mc).inicio >= t + (uint)k) DesplazarMerge(mc, delta);
            }

            foreach (var r in allRows.Where(r => r.RowIndex!.Value > t && r.RowIndex!.Value < t + (uint)k).ToList())
                r.Remove();
            if (mergeCells != null)
                foreach (var mc in mergeCells.Elements<SS.MergeCell>().ToList())
                {
                    var s = MergeFilas(mc).inicio;
                    if (s > t && s < t + (uint)k) mc.Remove();
                }

            var pristina = (SS.Row)templateRow.CloneNode(true);
            var prev = templateRow;
            for (int i = 0; i < m; i++)
            {
                uint rIdx = t + (uint)i;
                SS.Row row;
                if (i == 0) row = templateRow;
                else
                {
                    row = (SS.Row)pristina.CloneNode(true);
                    row.RowIndex = rIdx;
                    foreach (var c in row.Elements<SS.Cell>())
                    {
                        var col = ColumnaDeCelda(c);
                        if (col.Length > 0) c.CellReference = col + rIdx;
                    }
                    prev.InsertAfterSelf(row);
                    if (mergeCells != null)
                        foreach (var (c1, c2) in templateMerges)
                            mergeCells.Append(new SS.MergeCell { Reference = $"{c1}{rIdx}:{c2}{rIdx}" });
                }

                var merged = new Dictionary<string, string>(variables);
                foreach (var kv in filas[i]) merged[kv.Key] = kv.Value;
                foreach (var c in row.Elements<SS.Cell>())
                {
                    var txt = TextoCelda(c, sst);
                    if (txt.Contains("{{")) EscribirCeldaInline(c, AplicarVariables(txt.Replace(marcador, ""), merged));
                }
                prev = row;
            }

            if (mergeCells != null)
                mergeCells.Count = (uint)mergeCells.Elements<SS.MergeCell>().Count();
        }

        private static (uint inicio, uint fin) MergeFilas(SS.MergeCell mc)
        {
            var p = (mc.Reference?.Value ?? "").Split(':');
            uint Fila(string r) { uint.TryParse(new string(r.SkipWhile(char.IsLetter).ToArray()), out var v); return v; }
            return (Fila(p[0]), Fila(p.Length > 1 ? p[1] : p[0]));
        }

        private static (string, string) MergeColumnas(SS.MergeCell mc)
        {
            var p = (mc.Reference?.Value ?? "").Split(':');
            string Col(string r) => new string(r.TakeWhile(char.IsLetter).ToArray());
            return (Col(p[0]), Col(p.Length > 1 ? p[1] : p[0]));
        }

        private static void DesplazarFila(SS.Row r, int delta)
        {
            uint n = (uint)((int)r.RowIndex!.Value + delta);
            r.RowIndex = n;
            foreach (var c in r.Elements<SS.Cell>())
            {
                var col = ColumnaDeCelda(c);
                if (col.Length > 0) c.CellReference = col + n;
            }
        }

        private static void DesplazarMerge(SS.MergeCell mc, int delta)
        {
            var p = (mc.Reference?.Value ?? "").Split(':');
            string Shift(string cellref)
            {
                var col = new string(cellref.TakeWhile(char.IsLetter).ToArray());
                uint.TryParse(new string(cellref.SkipWhile(char.IsLetter).ToArray()), out var row);
                return col + (uint)((int)row + delta);
            }
            mc.Reference = p.Length > 1 ? $"{Shift(p[0])}:{Shift(p[1])}" : Shift(p[0]);
        }

        private static string AplicarVariables(string text, Dictionary<string, string> variables)
        {
            foreach (var (key, value) in variables)
                text = text.Replace("{{" + key + "}}", value ?? string.Empty);
            return text;
        }

        private void ReemplazarVariables(OpenXmlElement element, Dictionary<string, string> variables)
        {
            foreach (var paragraph in element.Descendants<Paragraph>())
            {
                var fullText = string.Concat(paragraph.Descendants<Text>().Select(t => t.Text));

                if (!fullText.Contains("{{")) continue;

                var newText = fullText;
                foreach (var (key, value) in variables)
                {
                    newText = newText.Replace($"{{{{{key}}}}}", value ?? "");
                }

                if (newText != fullText)
                {
                    var texts = paragraph.Descendants<Text>().ToList();
                    if (texts.Count > 0)
                    {
                        var firstRun = texts[0].Parent as Run;
                        texts[0].Text = newText;

                        for (int i = 1; i < texts.Count; i++)
                        {
                            var run = texts[i].Parent as Run;
                            if (run != null && run != firstRun)
                                run.Remove();
                            else
                                texts[i].Text = "";
                        }
                    }
                }
            }
        }

        private void ProcesarTabla(Body body, string tablaKey, List<Dictionary<string, string>> filas)
        {
            var marcador = $"{{{{{tablaKey}}}}}";

            foreach (var table in body.Descendants<Table>().ToList())
            {
                var rows = table.Elements<TableRow>().ToList();

                int templateRowIndex = -1;

                for (int i = 0; i < rows.Count; i++)
                {
                    var rowText = string.Concat(rows[i].Descendants<Text>().Select(t => t.Text));
                    if (rowText.Contains(marcador))
                    {
                        templateRowIndex = i;
                        break;
                    }
                }

                if (templateRowIndex < 0) continue;

                var templateRow = rows[templateRowIndex];
                TableRow? styleRow = null;
                if (templateRowIndex + 1 < rows.Count)
                {
                    var nextRow = rows[templateRowIndex + 1];
                    var nextRowText = string.Concat(nextRow.Descendants<Text>().Select(t => t.Text));
                    if (nextRowText.Contains("{{"))
                    {
                        styleRow = nextRow;
                    }
                    else if (string.IsNullOrWhiteSpace(nextRowText))
                    {
                        styleRow = nextRow;
                        CopiarContenidoCeldas(templateRow, styleRow);
                    }
                }

                var rowBeforeTemplate = templateRowIndex > 0 ? rows[templateRowIndex - 1] : null;

                var skip = styleRow != null ? 2 : 1;
                var emptyRowsCount = rows.Skip(templateRowIndex + skip)
                    .TakeWhile(r => string.IsNullOrWhiteSpace(string.Concat(r.Descendants<Text>().Select(t => t.Text))))
                    .Count();
                var minTotal = skip + emptyRowsCount;

                var totalRows = Math.Max(filas.Count, minTotal);

                var templates = new List<TableRow> { templateRow };
                if (styleRow != null) templates.Add(styleRow);

                templateRow.Remove();
                styleRow?.Remove();

                var emptyAfter = table.Elements<TableRow>().ToList()
                    .Where(r => string.IsNullOrWhiteSpace(string.Concat(r.Descendants<Text>().Select(t => t.Text))))
                    .ToList();
                foreach (var er in emptyAfter)
                {
                    if (rowBeforeTemplate != null && er == rowBeforeTemplate) continue;
                    er.Remove();
                }

                var insertAfter = rowBeforeTemplate ?? table.Elements<TableRow>().LastOrDefault();

                for (int i = 0; i < totalRows; i++)
                {
                    var templateIndex = templates.Count > 1 ? (i % 2) : 0;
                    var newRow = (TableRow)templates[templateIndex].CloneNode(true);

                    var data = i < filas.Count ? filas[i] : null;
                    foreach (var text in newRow.Descendants<Text>())
                    {
                        var t = text.Text;
                        t = t.Replace(marcador, "");
                        if (data != null)
                        {
                            foreach (var (key, value) in data)
                            {
                                t = t.Replace($"{{{{{key}}}}}", value ?? "");
                            }
                        }
                        t = Regex.Replace(t, @"\{\{[^}]+\}\}", "");
                        text.Text = t;
                    }

                    if (insertAfter != null)
                    {
                        insertAfter.InsertAfterSelf(newRow);
                        insertAfter = newRow;
                    }
                    else
                    {
                        table.PrependChild(newRow);
                        insertAfter = newRow;
                    }
                }
            }
        }

        private static void CopiarContenidoCeldas(TableRow from, TableRow to)
        {
            var fromCells = from.Elements<TableCell>().ToList();
            var toCells = to.Elements<TableCell>().ToList();
            var n = Math.Min(fromCells.Count, toCells.Count);
            for (int i = 0; i < n; i++)
            {
                toCells[i].RemoveAllChildren<Paragraph>();
                foreach (var para in fromCells[i].Elements<Paragraph>())
                {
                    toCells[i].AppendChild((Paragraph)para.CloneNode(true));
                }
            }
        }

        public Dictionary<string, string> GetVariablesDisponibles(string codigo)
        {
            return codigo switch
            {
                "listado_grupo" => new Dictionary<string, string>
                {
                    { "carrera", "Nombre de la carrera" },
                    { "periodo", "Periodo académico" },
                    { "grupo", "Código del grupo" },
                    { "tabla_estudiantes", "Tabla: matricula, estatus, nombre" }
                },
                "constancia_estudios" => new Dictionary<string, string>
                {
                    { "nombre_alumno", "Nombre completo" },
                    { "matricula", "Matrícula" },
                    { "curp", "CURP" },
                    { "carrera", "Carrera" },
                    { "rvoe", "RVOE" },
                    { "periodo", "Periodo actual" },
                    { "grado", "Cuatrimestre que cursa" },
                    { "campus", "Campus" },
                    { "fecha", "Fecha de expedición" },
                    { "folio", "Folio del documento" }
                },
                "kardex" => new Dictionary<string, string>
                {
                    { "nombre_alumno", "Nombre completo" },
                    { "matricula", "Matrícula" },
                    { "carrera", "Carrera" },
                    { "rvoe", "RVOE" },
                    { "ciclo_ingreso", "Ciclo de ingreso" },
                    { "tabla_materias", "Tabla: materia, clave, ciclo, calificacion, promedio" }
                },
                "boleta_calificaciones" => new Dictionary<string, string>
                {
                    { "CARRERA", "Nombre de la carrera" },
                    { "PERIODO", "Periodo académico" },
                    { "NOMBRE_ALUMNO", "Nombre completo del alumno" },
                    { "MATRICULA", "Matrícula del alumno" },
                    { "GRUPO", "Código del grupo" },
                    { "tabla_materias", "Tabla: clave, nombre_materia, calificacion" }
                },
                "acta_calificaciones" => new Dictionary<string, string>
                {
                    { "CARRERA", "Nombre de la carrera" },
                    { "PERIODO", "Periodo académico" },
                    { "MATERIA", "Nombre de la materia" },
                    { "CLAVE", "Clave de la materia" },
                    { "DOCENTE", "Nombre del docente" },
                    { "GRUPO", "Código del grupo" },
                    { "tabla_alumnos", "Tabla: matricula, nombre_alumno, calificacion_numero, calificacion_letra" }
                },
                _ => new Dictionary<string, string>()
            };
        }
    }
}
