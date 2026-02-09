using WebApplication2.Core.Enums;

namespace WebApplication2.Core.Requests.NewFolder
{
    public class MateriaPlanUpdateRequest : MateriaPlanRequest
    {
        public int IdMateriaPlan { get; set; }
        public StatusEnum Status { get; set; } = StatusEnum.Active;
    }
}
