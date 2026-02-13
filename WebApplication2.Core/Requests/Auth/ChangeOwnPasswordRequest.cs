namespace WebApplication2.Core.Requests.Auth
{
    public class ChangeOwnPasswordRequest
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }
}
