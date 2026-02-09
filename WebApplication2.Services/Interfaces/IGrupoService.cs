using WebApplication2.Core.Common;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.DTOs.GestionAcademica;
using WebApplication2.Core.DTOs.Grupo;
using WebApplication2.Core.DTOs.Inscripcion;
using WebApplication2.Core.Requests.GestionAcademica;
using WebApplication2.Core.Requests.Grupo;
using WebApplication2.Core.Responses.Grupo;
using WebApplication2.Core.Models;

namespace WebApplication2.Services.Interfaces
{
    public interface IGrupoService
    {
        Task<PagedResult<Grupo>> GetGrupos(int page, int pageSize, int? idPeriodoAcademico = null);
        Task<Grupo> GetDetalleGrupo(int idGrupo);
        Task<Grupo> CrearGrupo(Grupo grupo);
        Task<IEnumerable<GrupoMateria>> CargarMateriasGrupo(IEnumerable<GrupoMateria> grupoMaterias);
        Task<Grupo> ActualizarGrupo(Grupo newGrupo);

        Task<(bool Exito, string Mensaje)> EliminarGrupoAsync(int idGrupo, CancellationToken ct = default);

        Task<GrupoMateria?> GetGrupoMateriaByNameAsync(string nombreGrupoMateria);

        Task<Grupo?> GetGrupoPorCodigoAsync(string codigoGrupo);

        Task<InscripcionGrupoResultDto> InscribirEstudianteGrupoAsync(int idGrupo, int idEstudiante, bool forzarInscripcion = false, string? observaciones = null);

        Task<EstudiantesGrupoDto> GetEstudiantesDelGrupoAsync(int idGrupo);
        Task<List<Grupo>> BuscarGruposPorCriteriosAsync(int? numeroCuatrimestre = null, int? idTurno = null, int? numeroGrupo = null, int? idPlanEstudios = null);

        string GenerarCodigoGrupo(byte numeroCuatrimestre, int idTurno, byte numeroGrupo);

        Task<List<GrupoMateriaDisponibleDto>> GetGruposMateriasDisponiblesAsync(int? idEstudiante = null, int? idPeriodoAcademico = null);

        Task<List<EstudianteInscritoDto>> GetEstudiantesPorGrupoMateriaAsync(int idGrupoMateria);

        Task<GestionGruposPlanDto> ObtenerGruposPorPlanAsync(int idPlanEstudios, int? idPeriodoAcademico = null, CancellationToken ct = default);

        Task<GrupoResumenDto> CrearGrupoConMateriasAsync(CrearGrupoAcademicoRequest request, CancellationToken ct = default);

        Task<GrupoMateria> AgregarMateriaAlGrupoAsync(int idGrupo, int idMateriaPlan, int? idProfesor = null, string? aula = null, short? cupo = null, CancellationToken ct = default);

        Task<bool> QuitarMateriaDelGrupoAsync(int idGrupoMateria, CancellationToken ct = default);

        Task<List<GrupoMateriaDetalleDto>> ObtenerMateriasDelGrupoAsync(int idGrupo, CancellationToken ct = default);

        Task<GrupoMateria?> ObtenerGrupoMateriaPorIdAsync(int idGrupoMateria, CancellationToken ct = default);

        Task<PromocionAutomaticaResultDto> PromoverEstudiantesAsync(PromoverEstudiantesRequest request, CancellationToken ct = default);

        Task<PreviewPromocionResultDto> PreviewPromocionAsync(PreviewPromocionRequest request, CancellationToken ct = default);

        Task<(bool PuedePromover, string Motivo)> ValidarPromocionEstudianteAsync(int idEstudiante, int cuatrimestreActual, decimal promedioMinimo, CancellationToken ct = default);

        Task<Grupo?> ObtenerOCrearGrupoSiguienteAsync(int idGrupoActual, int idPeriodoAcademicoDestino, bool crearSiNoExiste = true, CancellationToken ct = default);

        Task ActualizarHorariosGrupoMateriaAsync(int idGrupoMateria, List<HorarioDto> horarios, CancellationToken ct = default);

        Task<GrupoMateria?> AsignarProfesorAMateriaAsync(int idGrupoMateria, int? idProfesor, CancellationToken ct = default);

        Task<EstudianteGrupoResultDto> InscribirEstudianteAGrupoDirectoAsync(
            int idGrupo,
            int idEstudiante,
            string? observaciones = null,
            CancellationToken ct = default);

        Task<InscribirEstudiantesGrupoResponse> InscribirEstudiantesAGrupoMasivoAsync(
            InscribirEstudiantesGrupoRequest request,
            CancellationToken ct = default);

        Task<EstudiantesDelGrupoResponse> GetEstudiantesDelGrupoDirectoAsync(int idGrupo, CancellationToken ct = default);

        Task<bool> EliminarEstudianteDeGrupoAsync(int idEstudianteGrupo, CancellationToken ct = default);

        Task<ImportarEstudiantesGrupoResponse> ImportarEstudiantesCompletoAsync(
            ImportarEstudiantesGrupoRequest request,
            CancellationToken ct = default);
    }
}
