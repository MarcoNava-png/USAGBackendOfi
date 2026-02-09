using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication2.Core.Enums;

namespace WebApplication2.Core.Requests.Parciales
{
    public class ParcialesUpdateRequest : ParcialesRequest
    {
        public int Id { get; set; }
        public StatusEnum Status { get; set; } = StatusEnum.Active;
    }
}
