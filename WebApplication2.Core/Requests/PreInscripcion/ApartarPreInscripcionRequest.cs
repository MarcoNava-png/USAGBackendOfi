namespace WebApplication2.Core.Requests.PreInscripcion
{
    public class ApartarPreInscripcionRequest
    {
        public int IdEstudiante { get; set; }
        public int IdPlanEstudios { get; set; }
        public int IdPeriodoAcademicoDestino { get; set; }
        public byte NumeroCuatrimestreObjetivo { get; set; }
        public string? Nota { get; set; }
    }
}
