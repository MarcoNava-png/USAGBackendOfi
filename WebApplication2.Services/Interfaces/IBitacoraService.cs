using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication2.Core.Common;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.Models;

namespace WebApplication2.Services.Interfaces
{
    public interface IBitacoraService
    {
        Task<long> AgregarAsync(BitacoraCreateDto dto, CancellationToken ct);
        Task<PagedResult<BitacoraDto>> GetBitacora(int page, int pageSize, string filter);
    }
}
