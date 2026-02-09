using WebApplication2.Core.Requests.Auth;

namespace WebApplication2.Core.Requests.Profesor
{
    public class ProfesorRequest : PersonaSignupRequest
    {
        public string NoEmpleado { get; set; }
        public string Rfc { get; set; }
        public string EmailInstitucional { get; set; }
        public int CampusId { get; set; }
    }
}
