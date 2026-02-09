using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.Requests.Aspirante;

namespace WebApplication2.Services.Interfaces
{
    public interface IAspiranteQueryService
    {
        Task<(IReadOnlyList<AspiranteGridItemDto> Items, int TotalItems)>
            GetGridAsync(GetAspirantesRequest req);
    }
}
