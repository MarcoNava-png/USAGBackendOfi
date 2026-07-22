using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using WebApplication2.Data.DbContexts;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Services
{
    public class MatriculaService : IMatriculaService
    {
        private readonly ApplicationDbContext _dbContext;

        public MatriculaService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<string> GenerarMatriculaAsync(string nombrePlanEstudios)
        {
            var prefijo = ObtenerPrefijo(nombrePlanEstudios);
            var prefijoEscapado = Regex.Escape(prefijo);

            var matriculas = await _dbContext.Estudiante
                .Where(e => e.Matricula.StartsWith(prefijo))
                .Select(e => e.Matricula)
                .ToListAsync();

            int siguienteNumero = 1;

            var numerosUsados = matriculas
                .Select(m =>
                {
                    var match = Regex.Match(m, $"^{prefijoEscapado}(\\d{{5,6}})$");
                    return match.Success && int.TryParse(match.Groups[1].Value, out var n) ? n : -1;
                })
                .Where(n => n > 0)
                .ToList();

            if (numerosUsados.Count > 0)
            {
                siguienteNumero = numerosUsados.Max() + 1;
            }

            var nuevaMatricula = $"{prefijo}{siguienteNumero:D5}";

            while (await ExisteMatriculaAsync(nuevaMatricula))
            {
                siguienteNumero++;
                nuevaMatricula = $"{prefijo}{siguienteNumero:D5}";
            }

            return nuevaMatricula;
        }

        public string ObtenerPrefijo(string nombrePlanEstudios)
        {
            if (string.IsNullOrWhiteSpace(nombrePlanEstudios))
                return "L";

            var nombre = nombrePlanEstudios.ToUpperInvariant().Trim();

            if (nombre.Contains("BACHILLERATO"))
            {
                if (nombre.Contains("TECNOL") || nombre.Contains("TÉCNICA") || nombre.Contains("TECNICA"))
                    return "LBT";
                return "B";
            }

            if (nombre.Contains("PREPARATORIA") || nombre.Contains("PREPA"))
                return "B";

            if (nombre.Contains("AUXILIAR"))
                return "LA";

            if (nombre.Contains("EQUIVALENCIA"))
                return "LE";

            if (nombre.Contains("INGENIERÍA DE SOFTWARE Y SISTEMAS") ||
                nombre.Contains("INGENIERÍA INDUSTRIAL") ||
                nombre.Contains("PEDAGOGÍA") ||
                nombre.Contains("PSICOLOGÍA") ||
                nombre.Contains("DERECHO"))
                return "LC";

            if (nombre.Contains("TÉCNICO SUPERIOR UNIVERSITARIO") || nombre.Contains("TSU"))
                return "L";

            if (nombre.Contains("ESPECIALIDAD") ||
                nombre.Contains("LICENCIATURA") ||
                nombre.Contains("LIC."))
                return "L";

            return "L";
        }

        public bool ValidarFormatoMatricula(string matricula)
        {
            if (string.IsNullOrWhiteSpace(matricula))
                return false;

            var regex = new Regex(@"^[A-Z]{1,3}\d{5}$");
            return regex.IsMatch(matricula);
        }

        public async Task<bool> ExisteMatriculaAsync(
            string matricula,
            int? excluirEstudianteId = null)
        {
            var query = _dbContext.Estudiante
                .Where(e => e.Matricula == matricula);

            if (excluirEstudianteId.HasValue)
            {
                query = query.Where(e => e.IdEstudiante != excluirEstudianteId.Value);
            }

            return await query.AnyAsync();
        }
    }
}
