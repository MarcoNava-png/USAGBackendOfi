using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication2.Core.Requests.Aspirante
{
    public class GetAspirantesRequest
    {
        public int Page { get; set; } = 1;        
        public int PageSize { get; set; } = 20;
        public string Filter { get; set; } = "";   
    }
}

