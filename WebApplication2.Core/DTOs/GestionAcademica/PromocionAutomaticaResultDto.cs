namespace WebApplication2.Core.DTOs.GestionAcademica
{
    public class PromocionAutomaticaResultDto
    {
        public int IdGrupoOrigen { get; set; }
        public string GrupoOrigen { get; set; } = string.Empty;
        public int CuatrimestreOrigen { get; set; }
        public int IdGrupoDestino { get; set; }
        public string GrupoDestino { get; set; } = string.Empty;
        public int CuatrimestreDestino { get; set; }
        public int TotalEstudiantesPromovidos { get; set; }
        public int TotalEstudiantesNoPromovidos { get; set; }
        public List<EstudiantePromocionDto> Estudiantes { get; set; } = new();
        public string Mensaje { get; set; } = string.Empty;
    }
}
