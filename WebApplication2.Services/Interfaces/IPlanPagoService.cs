using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication2.Core.DTOs;

namespace WebApplication2.Services.Interfaces
{
    public interface IPlanPagoService
    {
        Task<int> CrearPlanAsync(CrearPlanPagoDto dto, CancellationToken ct);
        Task<int> AgregarDetalleAsync(int idPlanPago, CrearPlanDetalleDto dto, CancellationToken ct);
        Task<long> AsignarPlanAsync(AsignarPlanDto dto, CancellationToken ct);
    }
}
