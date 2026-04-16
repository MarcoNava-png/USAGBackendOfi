using System.Text.Json;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Core.Enums;
using WebApplication2.Core.Models;
using WebApplication2.Data.DbContexts;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Services
{
    public class PlantillaReporteService : IPlantillaReporteService
    {
        private readonly ApplicationDbContext _db;
        private readonly string _uploadDir;

        public PlantillaReporteService(ApplicationDbContext db)
        {
            _db = db;
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

        public async Task<PlantillaReporte> CrearAsync(string nombre, string codigo, string categoria, string? descripcion, Stream archivo, string nombreArchivo, string variablesJson, CancellationToken ct = default)
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
                Activa = true,
                CreatedAt = DateTime.UtcNow,
                Status = StatusEnum.Active
            };

            _db.PlantillaReportes.Add(plantilla);
            await _db.SaveChangesAsync(ct);
            return plantilla;
        }

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

                var templateRows = new List<TableRow>();
                templateRows.Add(rows[templateRowIndex]);

                if (templateRowIndex + 1 < rows.Count)
                {
                    var nextRowText = string.Concat(rows[templateRowIndex + 1].Descendants<Text>().Select(t => t.Text));
                    if (!nextRowText.Contains("{{") || string.IsNullOrWhiteSpace(nextRowText))
                    {
                        templateRows.Add(rows[templateRowIndex + 1]);
                    }
                }

                foreach (var tr in templateRows) tr.Remove();

                var remainingEmpty = rows.Skip(templateRowIndex + templateRows.Count)
                    .Where(r => string.IsNullOrWhiteSpace(string.Concat(r.Descendants<Text>().Select(t => t.Text))))
                    .ToList();
                foreach (var emptyRow in remainingEmpty) emptyRow.Remove();

                var refreshedRows = table.Elements<TableRow>().ToList();
                var insertAfter = refreshedRows.Count > 0 ? refreshedRows.Last() : null;

                for (int i = 0; i < filas.Count; i++)
                {
                    var templateIndex = templateRows.Count > 1 ? (i % 2) : 0;
                    var newRow = (TableRow)templateRows[templateIndex].CloneNode(true);

                    foreach (var text in newRow.Descendants<Text>())
                    {
                        var t = text.Text;
                        t = t.Replace(marcador, "");
                        foreach (var (key, value) in filas[i])
                        {
                            t = t.Replace($"{{{{{key}}}}}", value ?? "");
                        }
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
