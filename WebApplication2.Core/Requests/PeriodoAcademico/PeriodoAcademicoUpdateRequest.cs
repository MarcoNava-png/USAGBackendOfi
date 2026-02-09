using WebApplication2.Core.Enums;

namespace WebApplication2.Core.Requests.PeriodoAcademico
{
    public class PeriodoAcademicoUpdateRequest : PeriodoAcademicoRequest
    {
        public int IdPeriodoAcademico { get; set; }
        public StatusEnum Status { get; set; }
    }
}
