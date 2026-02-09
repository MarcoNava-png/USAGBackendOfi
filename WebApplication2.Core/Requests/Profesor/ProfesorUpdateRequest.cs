using WebApplication2.Core.Enums;

namespace WebApplication2.Core.Requests.Profesor
{
    public class ProfesorUpdateRequest : ProfesorRequest
    {
        public int IdProfesor { get; set; }
        public StatusEnum Status { get; set; }
    }
}
