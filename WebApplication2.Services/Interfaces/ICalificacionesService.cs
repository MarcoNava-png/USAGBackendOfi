using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication2.Core.Common;
using WebApplication2.Core.Models;

namespace WebApplication2.Services.Interfaces
{
    public interface ICalificacionesService
    {
        Task<CalificacionParcial> AbrirParcial(CalificacionParcial acta);
        Task CambiarEstadoParcial(int calificacionParcialId, string nuevoEstado, string usuario);
        Task<CalificacionParcial> GetParcialById(int id);
        Task<IEnumerable<CalificacionParcial>> GetParcialesPorGrupo(int grupoMateriaId, int parcialId);
        Task UpsertDetalle(CalificacionDetalle detalle, string applicationUserName);
        Task<PagedResult<CalificacionDetalle>> GetDetalles(int grupoMateriaId, int parcialId, int inscripcionId, int tipoEvaluacionEnum,
            int page, int pageSize);
        Task<(IList<CalificacionDetalle>, decimal CalificacionFinal)> GetConcentradoAlumno(int inscripcionId);
        Task<IList<(int InscripcionId, decimal AporteParcial)>> GetConcentradoGrupoParcial(int grupoMateriaId, int parcialId);
        Task<(bool EsValido, decimal SumaPesos, string Mensaje)> ValidarPesosEvaluacion(int calificacionParcialId);
        Task<bool> ValidarEstadoParcialParaCaptura(int calificacionParcialId);

    }
}
