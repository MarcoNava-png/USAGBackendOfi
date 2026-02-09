using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebApplication2.Core.Common;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.DTOs.Grupo;
using WebApplication2.Core.Models;
using WebApplication2.Core.Requests.Asistencia;

namespace WebApplication2.Services.Interfaces
{
    public interface IAsistenciaService
    {
        Task<Asistencia> RegistrarAsistencia(Asistencia asistencia, int profesorId);
        Task<List<Asistencia>> RegistrarAsistenciaMasiva(List<Asistencia> asistencias, int profesorId);
        Task<Asistencia> ActualizarAsistencia(Asistencia asistencia, string usuario);
        Task<Asistencia> GetAsistenciaById(int idAsistencia);
        Task<List<Asistencia>> GetAsistenciasPorGrupoMateria(int grupoMateriaId, DateTime? fechaInicio = null, DateTime? fechaFin = null);
        Task<List<Asistencia>> GetAsistenciasPorInscripcion(int inscripcionId);
        Task<PagedResult<Asistencia>> GetAsistenciasConFiltros(int? grupoMateriaId, int? inscripcionId, DateTime? fechaInicio, DateTime? fechaFin, int? estadoAsistencia, int page, int pageSize);
        Task<(int TotalSesiones, int Presentes, int Ausentes, int Retardos, int Justificadas, decimal PorcentajeAsistencia)> GetEstadisticasAsistencia(int inscripcionId);
        Task<List<Asistencia>> RegistrarAsistenciasPorFecha(int idGrupoMateria, DateTime fecha, List<AsistenciaItemRequest> asistencias, int profesorId);
        Task<List<AsistenciaEstudianteDto>> GetAsistenciasPorGrupoMateriaYFecha(int idGrupoMateria, DateTime fecha);
        Task<DiasClaseMateriaDto> GetDiasClaseMateria(int idGrupoMateria);
        Task<List<ResumenAsistenciasDto>> GetResumenAsistenciasPorGrupoMateria(int idGrupoMateria);
    }
}
