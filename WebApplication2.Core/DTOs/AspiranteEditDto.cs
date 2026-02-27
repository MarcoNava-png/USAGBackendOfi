namespace WebApplication2.Core.DTOs
{
    public class AspiranteEditDto
    {
        public int IdAspirante { get; set; }
        public string? Nombre { get; set; }
        public string? ApellidoPaterno { get; set; }
        public string? ApellidoMaterno { get; set; }
        public DateOnly? FechaNacimiento { get; set; }
        public int? GeneroId { get; set; }
        public string? Correo { get; set; }
        public string? Telefono { get; set; }
        public string? Celular { get; set; }
        public string? CURP { get; set; }
        public string? Calle { get; set; }
        public string? NumeroExterior { get; set; }
        public string? NumeroInterior { get; set; }
        public int? CodigoPostalId { get; set; }
        public int? IdEstadoCivil { get; set; }
        public string? Nacionalidad { get; set; }
        public int? CampusId { get; set; }
        public int PlanEstudiosId { get; set; }
        public int MedioContactoId { get; set; }
        public string? Notas { get; set; }
        public int? HorarioId { get; set; }
        public int? CuatrimestreInteres { get; set; }
        public string? InstitucionProcedencia { get; set; }
        public int? IdModalidad { get; set; }
        public int? IdPeriodoAcademico { get; set; }
        public bool? RecorridoPlantel { get; set; }
        public bool? Trabaja { get; set; }
        public string? NombreEmpresa { get; set; }
        public string? DomicilioEmpresa { get; set; }
        public string? PuestoEmpresa { get; set; }
        public string? QuienCubreGastos { get; set; }
        public string? AtendidoPorUsuarioId { get; set; }
        public string? NombreContactoEmergencia { get; set; }
        public string? TelefonoContactoEmergencia { get; set; }
        public string? ParentescoContactoEmergencia { get; set; }
        public int IdAspiranteEstatus { get; set; }
        public string? EstadoId { get; set; }
        public string? MunicipioId { get; set; }
    }
}
