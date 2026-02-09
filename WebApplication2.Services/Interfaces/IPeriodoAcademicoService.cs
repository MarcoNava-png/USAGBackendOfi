using WebApplication2.Core.Common;
using WebApplication2.Core.Models;

namespace WebApplication2.Services.Interfaces
{
    public interface IPeriodoAcademicoService
    {
        Task<PagedResult<PeriodoAcademico>> GetPeriodosAcademicos(int page, int pageSize);
        Task<PeriodoAcademico> CrearPeriodoAcademico(PeriodoAcademico periodoAcademico);
        Task<PeriodoAcademico> ActualizarPeriodoAcademico(PeriodoAcademico newPeriodoAcademico);
        Task<PeriodoAcademico?> GetPeriodoActualAsync();
        Task<PeriodoAcademico> MarcarComoPeriodoActualAsync(int idPeriodoAcademico);
        bool EsPeriodoActivoPorFechas(PeriodoAcademico periodo, DateOnly? fechaReferencia = null);
        Task EliminarPeriodoAcademicoAsync(int idPeriodoAcademico);
        Task<PeriodoAcademico?> GetPeriodoAcademicoPorIdAsync(int idPeriodoAcademico);
    }
}
