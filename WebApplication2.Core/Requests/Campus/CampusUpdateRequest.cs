using WebApplication2.Core.Enums;

namespace WebApplication2.Core.Requests.Campus
{
    public class CampusUpdateRequest : CampusRequest
    {
        public int IdCampus { get; set; }
        public StatusEnum Status { get; set; } = StatusEnum.Active;
    }
}
