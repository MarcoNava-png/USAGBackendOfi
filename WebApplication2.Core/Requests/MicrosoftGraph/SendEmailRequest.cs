namespace WebApplication2.Core.Requests.MicrosoftGraph
{
    public class SendEmailRequest
    {
        public string To { get; set; } = null!;
        public List<string>? Cc { get; set; }
        public string Subject { get; set; } = null!;
        public string Body { get; set; } = null!;
        public bool IsHtml { get; set; } = true;
    }
}
