namespace WebApplication2.Core.DTOs
{
    public class ResumenAsistenciasDto
    {
        public int IdEstudiante { get; set; }
        public string Matricula { get; set; }
        public string NombreCompleto { get; set; }
        public int TotalClases { get; set; }
        public int Asistencias { get; set; }
        public int Faltas { get; set; }
        public int FaltasJustificadas { get; set; }
        public int FaltasInjustificadas { get; set; }
        public decimal PorcentajeAsistencia { get; set; }
        public bool Alerta { get; set; }
    }
}
