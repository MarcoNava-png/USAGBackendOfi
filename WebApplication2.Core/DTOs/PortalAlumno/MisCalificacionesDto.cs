namespace WebApplication2.Core.DTOs.PortalAlumno
{
    public class MisCalificacionesDto
    {
        public decimal? PromedioGeneral { get; set; }
        public int MateriasAprobadas { get; set; }
        public int MateriasReprobadas { get; set; }
        public int MateriasEnCurso { get; set; }
        public List<MiMateriaCalificacionDto> Materias { get; set; } = new();
    }

    public class MiMateriaCalificacionDto
    {
        public int IdMateria { get; set; }
        public string ClaveMateria { get; set; } = string.Empty;
        public string NombreMateria { get; set; } = string.Empty;
        public string? NombreDocente { get; set; }
        public string? GrupoCodigo { get; set; }
        public string? PeriodoAcademico { get; set; }
        public int Creditos { get; set; }
        public decimal? CalificacionFinal { get; set; }
        public string Estatus { get; set; } = string.Empty;
        public List<MiParcialDto> Parciales { get; set; } = new();
    }

    public class MiParcialDto
    {
        public int IdParciales { get; set; }
        public int NumeroParcial { get; set; }
        public decimal? Calificacion { get; set; }
        public bool Publicado { get; set; }
        public DateTime? FechaPublicacion { get; set; }
        public List<MiEvaluacionDto> Evaluaciones { get; set; } = new();
    }

    public class MiEvaluacionDto
    {
        public int IdCalificacionDetalle { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public string? Tipo { get; set; }
        public decimal Peso { get; set; }
        public decimal PuntajeMaximo { get; set; }
        public decimal? Puntaje { get; set; }
        public DateTime? FechaAplicacion { get; set; }
    }
}
