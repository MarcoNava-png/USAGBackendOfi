using WebApplication2.Core.Enums;

namespace WebApplication2.Core.Requests.PlanEstudios
{
    public class PlanEstudiosUpdateRequest : PlanEstudiosRequest
    {
        public int IdPlanEstudios { get; set; }
        public StatusEnum Status { get; set; }
    }
}
