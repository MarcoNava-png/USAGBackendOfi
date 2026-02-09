namespace WebApplication2.Core.Requests.Estudiante
{
    public class EstudianteRequest
    {
        public string Matricula { get; set; } = null!;

        public int IdPersona { get; set; }

        public DateOnly FechaIngreso { get; set; }

        public int? IdPlanActual { get; set; }

        public bool Activo { get; set; }
    }
}
