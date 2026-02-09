namespace WebApplication2.Core.DTOs.MicrosoftGraph
{
    public class EmailDto
    {
        public string Id { get; set; } = null!;
        public string? Subject { get; set; }
        public string? From { get; set; }
        public string? FromEmail { get; set; }
        public List<string> ToRecipients { get; set; } = new();
        public DateTime? ReceivedDateTime { get; set; }
        public string? BodyPreview { get; set; }
        public string? BodyContent { get; set; }
        public bool IsRead { get; set; }
        public bool HasAttachments { get; set; }
        public string? Importance { get; set; }
    }
}
