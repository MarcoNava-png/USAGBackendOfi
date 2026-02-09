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

            var ultimaMatricula = await _dbContext.Estudiante
                .Where(e => e.Matricula.StartsWith(prefijo))
                .OrderByDescending(e => e.Matricula)
                .Select(e => e.Matricula)
                .FirstOrDefaultAsync();

            int siguienteNumero = 1;

            if (ultimaMatricula != null)
            {
                var numeroStr = ultimaMatricula.Substring(prefijo.Length);
                if (int.TryParse(numeroStr, out int numero))
                {
                    siguienteNumero = numero + 1;
                }
            }

            var nuevaMatricula = $"{prefijo}{siguienteNumero:D6}";

            while (await ExisteMatriculaAsync(nuevaMatricula))
            {
                siguienteNumero++;
                nuevaMatricula = $"{prefijo}{siguienteNumero:D6}";
            }

            return nuevaMatricula;
        }

        public string ObtenerPrefijo(string nombrePlanEstudios)
        {
            if (string.IsNullOrWhiteSpace(nombrePlanEstudios))
                return "L";

            var nombre = nombrePlanEstudios.ToUpperInvariant().Trim();

            if (nombre.Contains("BACHILLERATO"))
                return "B";

            if (nombre.Contains("AUXILIAR"))
                return "LA";

            if (nombre.Contains("EQUIVALENCIA"))
                return "LE";

            if (nombre.Contains("INGENIERÍA DE SOFTWARE Y SISTEMAS") ||
                nombre.Contains("INGENIERÍA INDUSTRIAL") ||
                nombre.Contains("PEDAGOGÍA") ||
                nombre.Contains("PSICOLOGÍA") ||
                nombre.Contains("DERECHO") ||
                nombre.Contains("TRABAJO SOCIAL"))
                return "LC";

            if (nombre.Contains("TÉCNICO SUPERIOR UNIVERSITARIO") || nombre.Contains("TSU"))
                return "T";

            if (nombre.Contains("ESPECIALIDAD"))
                return "E";

            if (nombre.Contains("LICENCIATURA") || nombre.Contains("LIC."))
                return "L";

            return "L";
        }

        public bool ValidarFormatoMatricula(string matricula)
        {
            if (string.IsNullOrWhiteSpace(matricula))
                return false;

            var regex = new Regex(@"^[A-Z]{1,3}\d{6}$");
            return regex.IsMatch(matricula);
        }

        public async Task<bool> ExisteMatriculaAsync(string matricula)
        {
            return await _dbContext.Estudiante
                .AnyAsync(e => e.Matricula == matricula);
        }
    }
}
