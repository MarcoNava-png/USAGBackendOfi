namespace WebApplication2.Core.Requests.MateriaPlan
{
    public class ImportarMateriasRequest
    {
        public int? IdPlanEstudios { get; set; }
        public string? ClavePlanEstudios { get; set; }
        public List<MateriaImportItem> Materias { get; set; } = new();
    }

    public class MateriaImportItem
    {
        public string Clave { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public decimal Creditos { get; set; }
        public byte HorasTeoria { get; set; } = 0;
        public byte HorasPractica { get; set; } = 0;
        public string Grado { get; set; } = "1";
        public bool EsOptativa { get; set; } = false;
        public string? Campus { get; set; }
        public string? Curso { get; set; }
    }

    public class ImportarMateriasResponse
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public int? IdPlanEstudios { get; set; }
        public string? ClavePlanEstudios { get; set; }
        public string? NombrePlanEstudios { get; set; }
        public int TotalProcesadas { get; set; }
        public int MateriasCreadas { get; set; }
        public int MateriasExistentes { get; set; }
        public int AsignacionesCreadas { get; set; }
        public int AsignacionesExistentes { get; set; }
        public int Errores { get; set; }
        public List<ImportarMateriaResultItem> Detalle { get; set; } = new();
    }

    public class ImportarMateriaResultItem
    {
        public string Clave { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public int Cuatrimestre { get; set; }
        public string Estado { get; set; } = string.Empty; 
        public string? MensajeError { get; set; }
        public int? IdMateria { get; set; }
        public int? IdMateriaPlan { get; set; }
    }
}
