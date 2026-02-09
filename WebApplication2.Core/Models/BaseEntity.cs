using WebApplication2.Core.Enums;

namespace WebApplication2.Core.Models
{
    public abstract class BaseEntity
    {
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public string CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }

        public StatusEnum Status { get; set; } = StatusEnum.Active;
    }
}
