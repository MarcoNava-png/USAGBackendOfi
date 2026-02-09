using WebApplication2.Core.Models;
using WebApplication2.Data.DbContexts;

namespace WebApplication2.Data.Seed
{
    public static class TipoDocumentoSeed
    {
        public static void Seed(ApplicationDbContext context)
        {
            Console.WriteLine("=== INICIANDO TipoDocumentoSeed ===");

            if (context.TiposDocumentoEstudiante.Any())
            {
                Console.WriteLine($"Ya existen {context.TiposDocumentoEstudiante.Count()} tipos de documento.");
                return;
            }

            Console.WriteLine("Insertando tipos de documento iniciales...");

            var tiposDocumento = new List<TipoDocumentoEstudiante>
            {
                new TipoDocumentoEstudiante
                {
                    Clave = "CONSTANCIA_ESTUDIOS",
                    Nombre = "Constancia de Estudios",
                    Descripcion = "Documento que acredita que el alumno se encuentra inscrito y cursando estudios en la institución",
                    Precio = 150.00m,
                    DiasVigencia = 30,
                    RequierePago = true,
                    Activo = true,
                    Orden = 1
                },
                new TipoDocumentoEstudiante
                {
                    Clave = "KARDEX_COMPLETO",
                    Nombre = "Kardex Académico Completo",
                    Descripcion = "Historial académico completo del estudiante con todas las materias cursadas y calificaciones obtenidas",
                    Precio = 200.00m,
                    DiasVigencia = 60,
                    RequierePago = true,
                    Activo = true,
                    Orden = 2
                },
                new TipoDocumentoEstudiante
                {
                    Clave = "KARDEX_PARCIAL",
                    Nombre = "Kardex del Período Actual",
                    Descripcion = "Historial académico del período actual con las materias que está cursando",
                    Precio = 100.00m,
                    DiasVigencia = 30,
                    RequierePago = true,
                    Activo = true,
                    Orden = 3
                },
                new TipoDocumentoEstudiante
                {
                    Clave = "CARTA_BUENA_CONDUCTA",
                    Nombre = "Carta de Buena Conducta",
                    Descripcion = "Documento que acredita el buen comportamiento del estudiante dentro de la institución",
                    Precio = 100.00m,
                    DiasVigencia = 30,
                    RequierePago = true,
                    Activo = true,
                    Orden = 4
                },
                new TipoDocumentoEstudiante
                {
                    Clave = "CONSTANCIA_NO_ADEUDO",
                    Nombre = "Constancia de No Adeudo",
                    Descripcion = "Documento que acredita que el estudiante no tiene adeudos financieros con la institución",
                    Precio = 50.00m,
                    DiasVigencia = 15,
                    RequierePago = true,
                    Activo = true,
                    Orden = 5
                },
                new TipoDocumentoEstudiante
                {
                    Clave = "BOLETA_CALIFICACIONES",
                    Nombre = "Boleta de Calificaciones",
                    Descripcion = "Boleta oficial de calificaciones del período solicitado",
                    Precio = 75.00m,
                    DiasVigencia = 30,
                    RequierePago = true,
                    Activo = true,
                    Orden = 6
                }
            };

            context.TiposDocumentoEstudiante.AddRange(tiposDocumento);
            context.SaveChanges();

            Console.WriteLine($"Se insertaron {tiposDocumento.Count} tipos de documento.");
            Console.WriteLine("=== TipoDocumentoSeed COMPLETADO ===");
        }
    }
}
